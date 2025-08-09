using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.UI; // Essential for finding properties by name

namespace AnimaTween
{
    // --- ENUMS FOR THE API ---

    public enum Easing
    {
        InBack,
        InBounce,
        InCirc,
        InCubic,
        InElastic,
        InExpo,
        InOutBack,
        InOutBounce,
        InOutCirc,
        InOutCubic,
        InOutElastic,
        InOutExpo,
        InOutQuad,
        InOutQuart,
        InOutQuint,
        InOutSine,
        InQuad,
        InQuart,
        InQuint,
        InSine,
        Linear,
        OutBack,
        OutBounce,
        OutCirc,
        OutCubic,
        OutElastic,
        OutExpo,
        OutInBack,
        OutInBounce,
        OutInCirc,
        OutInCubic,
        OutInElastic,
        OutInExpo,
        OutInQuad,
        OutInQuart,
        OutInQuint,
        OutInSine,
        OutQuad,
        OutQuart,
        OutQuint,
        OutSine
    }

    public enum Playback
    {
        Forward, 
        Backward,    
        PingPong,    
        // Looping playback modes
        LoopForward, 
        LoopBackward,
        LoopPingPong,
    }

    // --- THE MAIN EXTENSION CLASS ---
    public static class AnimaTweenExtensions
    {
        private static readonly Dictionary<Tuple<object, string>, Coroutine> _activeTweens = new Dictionary<Tuple<object, string>, Coroutine>();
        private static AnimaTweenRunner _runner;

        // --- PUBLIC API METHODS ---

        public static void AnimaTween(this object target, string propertyName, object toValue, float duration, Easing easing = Easing.Linear, Playback playback = Playback.Forward, Action onComplete = null)
        {
            // Interrompe qualquer tween anterior na mesma propriedade
            target.Complete(propertyName, internalCall: true);

            FieldInfo fieldInfo = target.GetType().GetField(propertyName, BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo propertyInfo = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

            if (fieldInfo == null && propertyInfo == null)
            {
                Debug.LogError($"AnimaTween: Propriedade ou Campo '{propertyName}' não encontrado ou não é público em '{target.GetType().Name}'.");
                return;
            }

            // Lógica para animação de caminhos (waypoints)
            if (toValue is IEnumerable path && !(toValue is string))
            {
                var waypoints = new Queue<object>(path.Cast<object>());
                if (waypoints.Count > 0)
                {
                    float segmentDuration = duration / waypoints.Count;
                    AnimatePath(target, propertyName, waypoints, segmentDuration, easing, playback, onComplete);
                }
                return;
            }

            Type propertyType = fieldInfo != null ? fieldInfo.FieldType : propertyInfo.PropertyType;
            object startValue = fieldInfo != null ? fieldInfo.GetValue(target) : propertyInfo.GetValue(target);
            var key = new Tuple<object, string>(target, propertyName);

            // Inicia a corrotina "maestro" que gerenciará o playback e a finalização.
            Coroutine newCoroutine = GetRunner().StartCoroutine(
                TweenConductorCoroutine(target, fieldInfo, propertyInfo, startValue, toValue, duration, easing, playback, onComplete, propertyType)
            );

            if (newCoroutine != null)
            {
                _activeTweens[key] = newCoroutine;
            }
        }
        
        public static void Complete(this object target)
        {
            var keysToRemove = new List<Tuple<object, string>>();
            foreach (var pair in _activeTweens)
            {
                if (pair.Key.Item1 == target)
                {
                    GetRunner().StopCoroutine(pair.Value);
                    keysToRemove.Add(pair.Key);
                }
            }
            foreach (var key in keysToRemove) _activeTweens.Remove(key);
        }
        
        public static void Complete(this object target, string propertyName, bool internalCall = false)
        {
            var key = new Tuple<object, string>(target, propertyName);
            if (_activeTweens.TryGetValue(key, out Coroutine existing))
            {
                if (!internalCall) Debug.Log($"AnimaTween: Tween para '{propertyName}' foi completado manualmente.");
                GetRunner().StopCoroutine(existing);
                _activeTweens.Remove(key);
            }
        }
        
        public static void AnimaFade(this Component target, float toAlpha, float duration, Easing easing = Easing.Linear, Action onComplete = null)
        {
            if (target is Graphic graphic)
            {
                Color targetColor = graphic.color;
                targetColor.a = toAlpha;
                graphic.AnimaTween("color", targetColor, duration, easing, Playback.Forward, onComplete);
            }
            else if (target is CanvasGroup canvasGroup)
            {
                canvasGroup.AnimaTween("alpha", toAlpha, duration, easing, Playback.Forward, onComplete);
            }
            else if (target is SpriteRenderer sprite)
            {
                Color targetColor = sprite.color;
                targetColor.a = toAlpha;
                sprite.AnimaTween("color", targetColor, duration, easing, Playback.Forward, onComplete);
            }
            else
            {
                Debug.LogError($"AnimaFade: Componente '{target.GetType().Name}' não suportado para fade.");
            }
        }
        
        // --- PRIVATE HELPER METHODS ---

        private static AnimaTweenRunner GetRunner()
        {
            if (_runner == null)
            {
                _runner = GameObject.FindObjectOfType<AnimaTweenRunner>();
                if (_runner == null)
                {
                    GameObject runnerObject = new GameObject("AnimaTweenRunner (Auto-Generated)");
                    _runner = runnerObject.AddComponent<AnimaTweenRunner>();
                    UnityEngine.Object.DontDestroyOnLoad(runnerObject);
                }
            }
            return _runner;
        }

        // O "Maestro" que gerencia a lógica de playback
        private static IEnumerator TweenConductorCoroutine(object target, FieldInfo field, PropertyInfo prop, object startValue, object toValue, float duration, Easing easing, Playback playback, Action onComplete, Type propertyType)
        {
            bool isLooping = playback == Playback.LoopForward || playback == Playback.LoopBackward || playback == Playback.LoopPingPong;
            
            do
            {
                // --- Animação FORWARD ---
                if (playback == Playback.Forward || playback == Playback.PingPong || playback == Playback.LoopForward || playback == Playback.LoopPingPong)
                {
                    yield return SelectAnimationCoroutine(target, field, prop, startValue, toValue, duration, easing, propertyType);
                }

                // --- Animação BACKWARD ---
                if (playback == Playback.Backward || playback == Playback.PingPong || playback == Playback.LoopBackward || playback == Playback.LoopPingPong)
                {
                    // No caso de PingPong, o "toValue" da ida se torna o "startValue" da volta.
                    yield return SelectAnimationCoroutine(target, field, prop, toValue, startValue, duration, easing, propertyType);
                }
                
            } while (isLooping);

            // --- FINALIZAÇÃO ---
            // Este código só é alcançado quando o tween termina (não está em loop).
            var key = new Tuple<object, string>(target, field != null ? field.Name : prop.Name);
            _activeTweens.Remove(key);
            onComplete?.Invoke();
        }

        // Seleciona e retorna a corrotina de animação correta da classe auxiliar
        private static IEnumerator SelectAnimationCoroutine(object target, FieldInfo field, PropertyInfo prop, object from, object to, float duration, Easing easing, Type propertyType)
        {
            if (propertyType == typeof(int) || propertyType == typeof(float))
            {
                return AnimaTweenCoroutines.AnimateNumeric(target, field, prop, propertyType, Convert.ToSingle(from), Convert.ToSingle(to), duration, easing);
            }
            if (propertyType == typeof(Vector2))
            {
                return AnimaTweenCoroutines.AnimateVector2(target, field, prop, (Vector2)from, (Vector2)to, duration, easing);
            }
            if (propertyType == typeof(Vector3))
            {
                return AnimaTweenCoroutines.AnimateVector3(target, field, prop, (Vector3)from, (Vector3)to, duration, easing);
            }
            if (propertyType == typeof(Color))
            {
                return AnimaTweenCoroutines.AnimateColor(target, field, prop, (Color)from, (Color)to, duration, easing);
            }
            if (propertyType == typeof(Quaternion))
            {
                return AnimaTweenCoroutines.AnimateQuaternion(target, field, prop, (Quaternion)from, (Quaternion)to, duration, easing);
            }
            if (propertyType == typeof(string))
            {
                 // Lógica especial para strings (encadeamento para "replace")
                string startString = (string)from ?? "";
                string endString = (string)to ?? "";

                if (endString.StartsWith(startString) || startString.StartsWith(endString))
                {
                    // Grow or Shrink
                    return AnimaTweenCoroutines.AnimateString(target, field, prop, startString, endString, duration, easing);
                }
                else 
                {
                    // Replace: shrink then grow. This needs a special conductor.
                    return AnimateStringReplace(target, field, prop, startString, endString, duration, easing);
                }
            }

            Debug.LogError($"AnimaTween: Tipo de propriedade não suportado: {propertyType.Name}");
            return null;
        }
        
        // Corrotina especial para lidar com a substituição de strings (encadeamento)
        private static IEnumerator AnimateStringReplace(object target, FieldInfo field, PropertyInfo prop, string startString, string endString, float duration, Easing easing)
        {
            int prefixLength = 0;
            while (prefixLength < startString.Length && prefixLength < endString.Length && startString[prefixLength] == endString[prefixLength])
            {
                prefixLength++;
            }
            string commonPrefix = startString.Substring(0, prefixLength);
        
            float shrinkDuration = duration * 0.5f;
            float growDuration = duration * 0.5f;

            // Shrink
            yield return AnimaTweenCoroutines.AnimateString(target, field, prop, startString, commonPrefix, shrinkDuration, easing);
            // Grow
            yield return AnimaTweenCoroutines.AnimateString(target, field, prop, commonPrefix, endString, growDuration, easing);
        }
        
        // Helper para animação de caminhos (inalterado)
        private static void AnimatePath(object target, string propertyName, Queue<object> waypoints, float segmentDuration, Easing easing, Playback playback, Action onComplete)
        {
            if (waypoints.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }
            object nextWaypoint = waypoints.Dequeue();
            Action nextAction = () => {
                AnimatePath(target, propertyName, waypoints, segmentDuration, easing, playback, onComplete);
            };
            target.AnimaTween(propertyName, nextWaypoint, segmentDuration, easing, playback, nextAction);
        }
    }
}