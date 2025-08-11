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

    public enum EndState
    {
        Start,
        Middle,
        End
    }

    // --- THE MAIN EXTENSION CLASS ---
    public static class AnimaTweenExtensions
    {
        // A static reference ONLY for the global fallback runner.
        private static AnimaTweenRunner _globalRunner;
        // --- PUBLIC API METHODS ---

        public static void ATween(this object target, string propertyName, object toValue, float duration, 
            Easing easing = Easing.Linear,  Action onComplete = null, Playback playback = Playback.Forward,
            object fromValue = null)
        {
            // Stops any previous tween on the same property
            var host = GetHostForTarget(target);
            if (host == null) return;
            target.AComplete(propertyName, internalCall: true);

            FieldInfo fieldInfo = target.GetType().GetField(propertyName, BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo propertyInfo = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            
            
            
            if (fieldInfo == null && propertyInfo == null)
            {
                Debug.LogError($"AnimaTween: Propriedade ou Campo '{propertyName}' não encontrado ou não é público em '{target.GetType().Name}'.");
                return;
            }
            object startValue = fieldInfo != null ? fieldInfo.GetValue(target) : propertyInfo.GetValue(target);
            if (fromValue!= null && startValue.GetType() == fromValue.GetType())
            {
                startValue = fromValue;
                if (fieldInfo != null) fieldInfo.SetValue(target, startValue);
                else propertyInfo.SetValue(target, startValue);
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
            
            var tweenInfo = new TweenInfo
            {
                Target = target,
                OnComplete = onComplete,
                StartValue = startValue,
                ToValue = toValue,
                FieldInfo = fieldInfo,
                PropertyInfo = propertyInfo
            };
            tweenInfo.Coroutine = host.StartCoroutine(
                TweenConductorCoroutine(host, propertyName, tweenInfo, duration, easing, playback)
            );

            // Add the tween to the correct dictionary.
            if (host is AnimaTweenInstance instance)
            {
                instance.activeTweens[propertyName] = tweenInfo;
            }
            else if (host is AnimaTweenRunner runner)
            {
                var key = new Tuple<object, string>(target, propertyName);
                runner.AddTweenInfo(key, tweenInfo);
            }
        }
        
        /// <summary>
        /// Executes an action after a specified delay. Can be set to repeat, creating an interval.
        /// </summary>
        /// <returns>The unique ID of the created timer for that target, which can be used to stop it later.</returns>
        public static int ATimeout(this object target, float time, Action callback, bool repeat = false)
        {
            var host = GetHostForTarget(target);
            var tweenInfo = new TweenInfo { Target = target, OnComplete = callback };
            int nextId = 0;
            string key;

            // Lógica para gerar ID e chave do timer
            if (host is AnimaTweenInstance instance)
            {
                nextId = instance.activeTweens.Keys
                    .Where(k => k.StartsWith("@timer_"))
                    .Select(k => int.TryParse(k.Substring("@timer_".Length), out int id) ? id : -1)
                    .DefaultIfEmpty(-1).Max() + 1;
                key = $"@timer_{nextId}";
                tweenInfo.Coroutine = instance.StartCoroutine(TimerCoroutine(instance, key, tweenInfo, time, repeat));
                instance.activeTweens[key] = tweenInfo;
            }
            else if (host is AnimaTweenRunner runner)
            {
                nextId = runner.unhostedTweens.Keys
                    .Where(k => k.Item1 == target && k.Item2.StartsWith("@timer_"))
                    .Select(k => int.TryParse(k.Item2.Substring("@timer_".Length), out int id) ? id : -1)
                    .DefaultIfEmpty(-1).Max() + 1;
                key = $"@timer_{nextId}";
                var runnerKey = new Tuple<object, string>(target, key);
                tweenInfo.Coroutine = runner.StartCoroutine(TimerCoroutine(runner, runnerKey, tweenInfo, time, repeat));
                runner.AddTweenInfo(runnerKey, tweenInfo);
            }
            
            return nextId;
        }

        /// <summary>
        /// Stops a specific timer or all timers associated with a target object, if the timer do not exists do nothing.
        /// </summary>
        /// <param name="target">The object whose timers will be stopped.</param>
        /// <param name="timerId">The specific ID of the timer to stop. If omitted (-1), all timers on the object will be stopped.</param>
        
        /// <summary>
        /// Completa um timer específico ou todos os timers em um objeto.
        /// </summary>
        public static void ACompleteTimer(this object target, int timerId = -1, bool withCallback = true)
        {
            // Caso 1: Completar todos os timers do alvo.
            if (timerId == -1)
            {
                // Checa a instância local e completa seus timers.
                if (target is Component c && c.TryGetComponent<AnimaTweenInstance>(out var instance))
                {
                    instance.activeTweens.Keys
                        .Where(key => key.StartsWith("@timer_"))
                        .ToList()
                        .ForEach(timerKey => target.AComplete(timerKey, withCallback));
                }
        
                // Checa o runner global e completa seus timers para este alvo.
                if (_globalRunner != null)
                {
                    _globalRunner.unhostedTweens.Keys
                        .Where(key => key.Item1 == target && key.Item2.StartsWith("@timer_"))
                        .ToList()
                        // Usamos k.Item2 porque AComplete espera o nome da propriedade (a chave do timer).
                        .ForEach(k => target.AComplete(k.Item2, withCallback));
                }
                return;
            }
    
            // Caso 2: Completar um timer específico pelo seu ID.
            // A função AComplete principal irá procurar automaticamente nos locais corretos.
            string specificTimerKey = $"@timer_{timerId}";
            target.AComplete(specificTimerKey, withCallback);
        }
        
        /// <summary>
        /// Completes a tween, jumping to its final state and executing the OnComplete callback.
        /// </summary>
        public static void AComplete(this object target, string propertyName=null, bool withCallback = true,
             EndState endState=EndState.End,bool internalCall = false)
        {
            // --- Lógica para completar todos os tweens de um alvo ---
            if (string.IsNullOrEmpty(propertyName))
            {
                // Checa a instância local
                if (target is Component c && c.TryGetComponent<AnimaTweenInstance>(out var instance))
                {
                    instance.activeTweens.Keys.ToList().ForEach(k => target.AComplete(k, withCallback, endState));
                }
                // Checa o runner global
                if (_globalRunner != null)
                {
                    _globalRunner.unhostedTweens.Keys.Where(k => k.Item1 == target).ToList()
                        .ForEach(k => target.AComplete(k.Item2, withCallback, endState));
                }
                return;
            }
            // Tenta encontrar na instância local
            if (target is Component comp && comp.TryGetComponent<AnimaTweenInstance>(out var localInstance))
            {
                if (localInstance.activeTweens.TryGetValue(propertyName, out TweenInfo tweenInfo))
                {
                    localInstance.StopCoroutine(tweenInfo.Coroutine);
                    if (endState == EndState.End && tweenInfo.ToValue != null) tweenInfo.SetValue(tweenInfo.ToValue);
                    else if (endState == EndState.Start && tweenInfo.StartValue != null) tweenInfo.SetValue(tweenInfo.StartValue);
                    if (withCallback) tweenInfo.OnComplete?.Invoke();
                    localInstance.activeTweens.Remove(propertyName);
                    localInstance.MarkAsDirty();
                    return; // Encontrou e completou, pode sair.
                }
            }
            
            // Se não encontrou no local, tenta no runner global
            if (_globalRunner != null)
            {
                var key = new Tuple<object, string>(target, propertyName);
                if (_globalRunner.unhostedTweens.TryGetValue(key, out TweenInfo tweenInfo))
                {
                    _globalRunner.StopCoroutine(tweenInfo.Coroutine);
                    if (endState == EndState.End && tweenInfo.ToValue != null) tweenInfo.SetValue(tweenInfo.ToValue);
                    else if (endState == EndState.Start && tweenInfo.StartValue != null) tweenInfo.SetValue(tweenInfo.StartValue);
                    if (withCallback) tweenInfo.OnComplete?.Invoke();
                    _globalRunner.RemoveUnhostedTween(key);
                }
            }
        }

        /// <summary>
        /// Stops a tween immediately in its current state. No callbacks are invoked.
        /// </summary>
        public static void AStop(this object target, string propertyName = null, bool withCallback = false)
        {
            target.AComplete(propertyName, withCallback,EndState.Middle);
        }

        /// <summary>
        /// Stops a tween and reverts its target property to the value it had when the tween started.
        /// </summary>
        public static void ACancel(this object target, string propertyName = null, bool withCallback = false)
        {
            target.AComplete(propertyName, withCallback,EndState.Start);
        }
        
        public static void AFade(this Component target, float duration, Easing easing = Easing.Linear, 
            Action onComplete = null, float toAlpha=0)
        {
            if (target is Graphic graphic)
            {
                Color targetColor = graphic.color;
                targetColor.a = toAlpha;
                graphic.ATween("color", targetColor, duration, easing, onComplete);
            }
            else if (target is CanvasGroup canvasGroup)
            {
                canvasGroup.ATween("alpha", toAlpha, duration, easing, onComplete);
            }
            else if (target is SpriteRenderer sprite)
            {
                Color targetColor = sprite.color;
                targetColor.a = toAlpha;
                sprite.ATween("color", targetColor, duration, easing, onComplete);
            }
            else
            {
                Debug.LogError($"AnimaFade: Component '{target.GetType().Name}' not suportated to fade.");
            }
        }

        /// <summary>
        /// Called by the Scene Watcher when a scene is unloaded.
        /// Cleans up any tweens in the global runner whose targets were destroyed.
        /// </summary>
        public static void CleanUpGlobalRunner()
        {
            if (_globalRunner == null) return;
            _globalRunner.CheckCleanup();
        }


        // --- PRIVATE HELPER METHODS ---
        
        private static MonoBehaviour GetHostForTarget(object target)
        {
            // Case 1: The target is a standard Unity object.
            if (target is Component c)
            {
                if (c.TryGetComponent<AnimaTweenInstance>(out var instance)) return instance;
                return c.gameObject.AddComponent<AnimaTweenInstance>();
            }
            if (target is GameObject go)
            {
                if (go.TryGetComponent<AnimaTweenInstance>(out var instance)) return instance;
                return go.AddComponent<AnimaTweenInstance>();
            }

            // Case 2: The target is NOT a standard Unity object (e.g., a Material).
            // We use the global fallback runner.
            if (_globalRunner == null)
            {
                // Find it in the scene, or create it if it doesn't exist.
                _globalRunner = GameObject.FindFirstObjectByType<AnimaTweenRunner>();
                if (_globalRunner == null)
                {
                    var runnerObject = new GameObject("AnimaTweenRunner (Global)");
                    _globalRunner = runnerObject.AddComponent<AnimaTweenRunner>();
                    UnityEngine.Object.DontDestroyOnLoad(runnerObject);
                }
            }
            return _globalRunner;
        }
        
        // O "Maestro" que gerencia a lógica de playback
        private static IEnumerator TweenConductorCoroutine(MonoBehaviour host, string key, TweenInfo tweenInfo, float duration, Easing easing, Playback playback)
        {
            bool isLooping = playback == Playback.LoopForward || playback == Playback.LoopBackward || playback == Playback.LoopPingPong;
            do
            {
                // --- FORWARD Animation ---
                if (playback == Playback.Forward || playback == Playback.PingPong || playback == Playback.LoopForward || playback == Playback.LoopPingPong)
                {
                    yield return AnimaTweenCoroutines.Animate(tweenInfo, duration, easing, isFrom: false);
                }

                // --- BACKWARD Animation ---
                if (playback == Playback.Backward || playback == Playback.PingPong || playback == Playback.LoopBackward || playback == Playback.LoopPingPong)
                {
                    yield return AnimaTweenCoroutines.Animate(tweenInfo, duration, easing, isFrom: true);
                }
            } while (isLooping);

            // --- Natural Completion ---
            // This code is only reached when a non-looping tween finishes its duration.
            if (host is AnimaTweenInstance instance)
            {
                instance.activeTweens.Remove(key);
                instance.MarkAsDirty();
            }
            else if (host is AnimaTweenRunner runner)
            {
                var runnerKey = new Tuple<object, string>(tweenInfo.Target, key);
                runner.RemoveUnhostedTween(runnerKey);
            }
            tweenInfo.OnComplete?.Invoke();
        }

        /// <summary>
        /// The coroutine for handling timeouts and intervals.
        /// </summary>
        private static IEnumerator TimerCoroutine(MonoBehaviour host, object key, TweenInfo tweenInfo, float time, bool repeat)
        {
            if (repeat)
            {
                while (true)
                {
                    yield return new WaitForSeconds(time);
                    // Repeating timers do not remove themselves, they must be stopped manually.
                    tweenInfo.OnComplete?.Invoke();
                }
            }
            else
            {
                yield return new WaitForSeconds(time);
                
                // --- Natural Completion for a one-shot timer ---
                if (host is AnimaTweenInstance instance && key is string stringKey)
                {
                    instance.activeTweens.Remove(stringKey);
                    instance.MarkAsDirty();
                }
                else if (host is AnimaTweenRunner runner && key is System.Tuple<object, string> tupleKey)
                {
                    runner.RemoveUnhostedTween(tupleKey);
                }

                tweenInfo.OnComplete?.Invoke();
            }
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
            target.ATween(propertyName, nextWaypoint, segmentDuration, easing, nextAction, playback);
        }
    }
}