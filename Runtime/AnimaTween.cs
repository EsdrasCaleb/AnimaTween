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

        public static void ATween(this object target, string propertyName, object toValue, float duration, Easing easing = Easing.Linear, Playback playback = Playback.Forward, Action onComplete = null)
        {
            // Stops any previous tween on the same property
            target.AComplete(propertyName, internalCall: true);

            FieldInfo fieldInfo = target.GetType().GetField(propertyName, BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo propertyInfo = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            
            if (fieldInfo == null && propertyInfo == null)
            {
                Debug.LogError($"AnimaTween: Propriedade ou Campo '{propertyName}' não encontrado ou não é público em '{target.GetType().Name}'.");
                return;
            }

            // Logic for animating paths (waypoints)
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

            // Starts the "maestro" coroutine that will manage playback and finalization.
            Coroutine newCoroutine = GetRunner().StartCoroutine(
                TweenConductorCoroutine(target, fieldInfo, propertyInfo, startValue, toValue, duration, easing, playback, onComplete, propertyType)
            );

            if (newCoroutine != null)
            {
                _activeTweens[key] = newCoroutine;
            }
        }
        
        /// <summary>
        /// Executes an action after a specified delay. Can be set to repeat, creating an interval.
        /// </summary>
        /// <returns>The unique ID of the created timer for that target, which can be used to stop it later.</returns>
        public static int ATimeout(this object target, float time, Action callback, bool repeat = false)
        {
            // Find the highest existing timer ID for this specific target and add 1.
            int nextId = GetAllTimersForTarget(target)
                .Select(pair => int.TryParse(pair.Key.Item2.Substring("@timer_".Length), out int id) ? id : -1)
                .DefaultIfEmpty(-1)
                .Max() + 1;

            string timerKey = $"@timer_{nextId}";
            var key = new Tuple<object, string>(target, timerKey);

            Coroutine newCoroutine = GetRunner().StartCoroutine(
                TimerCoroutine(key, time, callback, repeat)
            );

            _activeTweens[key] = newCoroutine;
            
            // Return the new ID so the user can store it and cancel it later.
            return nextId;
        }

        /// <summary>
        /// Stops a specific timer or all timers associated with a target object, if the timer do not exists do nothing.
        /// </summary>
        /// <param name="target">The object whose timers will be stopped.</param>
        /// <param name="timerId">The specific ID of the timer to stop. If omitted (-1), all timers on the object will be stopped.</param>
        public static void ACompleteTimer(this object target, int timerId = -1)
        {
            // Case 1: Stop all timers for the target (when timerId is the default -1).
            if (timerId == -1)
            {
                target.ACompleteTimers();
                return;
            }
            
            // Case 2: Stop a specific timer by its ID.
            // We leverage the general-purpose AComplete function, which is perfectly DRY.
            string timerKeyToComplete = $"@timer_{timerId}";
            target.AComplete(timerKeyToComplete, true); // internalCall = true
        }


        /// <summary>
        /// Stops all active timers (created with ATimeout) on a specific object.
        /// This is now syntactic sugar for calling ACompleteTimer without a specific ID.
        /// </summary>
        /// <param name="target">The object whose timers will be stopped.</param>
        public static void ACompleteTimers(this object target)
        {
            var timersToComplete = GetAllTimersForTarget(target).ToList();
            
            foreach (var pair in timersToComplete)
            {
                GetRunner().StopCoroutine(pair.Value);
                _activeTweens.Remove(pair.Key);
            }
        }
        
        /// <summary>
        /// Stops ALL active animations and timers on a specific object.
        /// </summary>
        public static void AComplete(this object target)
        {
            // Find all keys associated with the target object using LINQ.
            // We convert it to a List to avoid issues with modifying the collection while iterating.
            _activeTweens.Keys
                .Where(key => key.Item1 == target)
                // Call the specific AComplete method for each found key
                .ToList().ForEach(key=>target.AComplete(key.Item2, internalCall: true));
        }

        /// <summary>
        /// Stops a specific animation or timer on an object, identified by its property name.
        /// </summary>
        public static void AComplete(this object target, string propertyName, bool internalCall = false)
        {
            var key = new Tuple<object, string>(target, propertyName);
            if (_activeTweens.TryGetValue(key, out Coroutine existing))
            {
                if (!internalCall) 
                {
                    Debug.Log($"AnimaTween: Tween for '{propertyName}' was completed manually.");
                }
        
                // This is now the single source of truth for stopping and removing a tween.
                GetRunner().StopCoroutine(existing);
                _activeTweens.Remove(key);
            }
        }
        
        public static void AFade(this Component target, float toAlpha, float duration, Easing easing = Easing.Linear, Action onComplete = null)
        {
            if (target is Graphic graphic)
            {
                Color targetColor = graphic.color;
                targetColor.a = toAlpha;
                graphic.ATween("color", targetColor, duration, easing, Playback.Forward, onComplete);
            }
            else if (target is CanvasGroup canvasGroup)
            {
                canvasGroup.ATween("alpha", toAlpha, duration, easing, Playback.Forward, onComplete);
            }
            else if (target is SpriteRenderer sprite)
            {
                Color targetColor = sprite.color;
                targetColor.a = toAlpha;
                sprite.ATween("color", targetColor, duration, easing, Playback.Forward, onComplete);
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
        
        /// <summary>
        /// Helper function to get all active timers for a specific target object using LINQ.
        /// </summary>
        private static IEnumerable<KeyValuePair<Tuple<object, string>, Coroutine>> GetAllTimersForTarget(object target)
        {
            return _activeTweens.Where(pair => pair.Key.Item1 == target && pair.Key.Item2.StartsWith("@timer_"));
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
        
        // Coroutine for handling timeouts and intervals.
        private static IEnumerator TimerCoroutine(Tuple<object, string> key, float time, Action callback, bool repeat)
        {
            if (repeat)
            {
                // For an interval, loop forever until cancelled by AComplete or ACompleteTimers.
                while (true)
                {
                    yield return new WaitForSeconds(time);
                    callback?.Invoke();
                }
            }
            else
            {
                // For a single timeout, wait, then execute and remove itself.
                yield return new WaitForSeconds(time);
                
                // Remove self from the active tweens dictionary BEFORE invoking the callback.
                // This prevents issues if the callback starts a new tween/timer on the same object.
                _activeTweens.Remove(key);
                callback?.Invoke();
            }
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
            target.ATween(propertyName, nextWaypoint, segmentDuration, easing, playback, nextAction);
        }
    }
}