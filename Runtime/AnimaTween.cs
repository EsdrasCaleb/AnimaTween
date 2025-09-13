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
            MonoBehaviour host = GetHostForTarget(target);
            if (host == null) return;
            target.AComplete(propertyName, internalCall: true);

            FieldInfo fieldInfo = target.GetType().GetField(propertyName, BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo propertyInfo = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            object startValue;
            bool materialProp = false;
            if ((fieldInfo != null) || (propertyInfo != null && propertyInfo.CanWrite))
            {
                startValue = fieldInfo != null ? fieldInfo.GetValue(target) : propertyInfo.GetValue(target);
            }
            else 
            {
                if (target is Material material)
                {
                    materialProp = true;
                    switch (toValue)
                    {
                        case float f:
                            startValue = material.GetFloat(propertyName);
                            break;
                        case int i:
                            startValue = material.GetInt(propertyName);
                            break;
                        case Color c:
                            startValue = material.GetColor(propertyName);
                            break;
                        case Vector4 v4:
                            startValue = material.GetVector(propertyName);
                            break;
                        case Vector3 v3:
                            startValue = material.GetVector(propertyName);
                            break;
                        case Vector2 v2:
                            startValue = material.GetVector(propertyName);
                            break;
                        default:
                            Debug.LogError($"AnimaTween: Value type '{toValue.GetType().Name}' is not supported for animating Material shader properties named '{propertyName}'.");
                            return;
                    }
                }
                else
                {
                    // Se não for um Material e não encontrarmos um campo/propriedade gravável, então é um erro.
                    Debug.LogError($"AnimaTween: Property or Field '{propertyName}' not found, is not public, or is read-only in '{target.GetType().Name}'.");
                    return;
                }
            }

            if (fromValue!= null && startValue.GetType() == fromValue.GetType())
            {
                startValue = fromValue;
                if (fieldInfo != null) fieldInfo.SetValue(target, startValue);
                else propertyInfo.SetValue(target, startValue);
            }

            // --- NOVA LÓGICA DE CAMINHOS ---
            object finalToValue = toValue;
            object[] midValues = null;

            if (toValue is IEnumerable path && !(toValue is string))
            {
                var pathList = path.Cast<object>().ToList();
                if (pathList.Count > 0)
                {
                    finalToValue = pathList.Last();
                    if (pathList.Count > 1)
                    {
                        midValues = pathList.Take(pathList.Count - 1).ToArray();
                    }
                }
            }
            
            TweenInfo tweenInfo = new TweenInfo(
                target,
                propertyName,
                onComplete,
                startValue,
                finalToValue,
                propertyInfo,
                fieldInfo,
                materialProp,
                midValues
            );

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
            var tweenInfo = new TweenInfo(target,callback);
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
        /// Creates a shake animation on a property. Works best with position or rotation.
        /// </summary>
        /// <param name="target">The object containing the property to shake.</param>
        /// <param name="propertyName">The name of the property to shake (must be a Vector3).</param>
        /// <param name="duration">The total duration of the shake effect.</param>
        /// <param name="strength">The maximum distance the object will move from its origin. Default is 1.</param>
        /// <param name="vibrato">How many "wiggles" the shake will have. Higher is more chaotic. Default is 10.</param>
        /// <param name="onComplete">An optional callback for when the shake finishes.</param>
        public static void AShake(this object target, string propertyName, float duration, float strength = 1f, int vibrato = 10, Action onComplete = null)
        {
            // Validação para garantir que a propriedade é um Vector3
            FieldInfo fieldInfo = target.GetType().GetField(propertyName, BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo propertyInfo = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if ((fieldInfo == null && propertyInfo == null) || (fieldInfo?.FieldType ?? propertyInfo?.PropertyType) != typeof(Vector3))
            {
                Debug.LogError($"AnimaTween AShake: Property '{propertyName}' not found or is not a Vector3.");
                return;
            }

            Vector3 startValue = (Vector3)(fieldInfo?.GetValue(target) ?? propertyInfo?.GetValue(target));
            
            // Gera proceduralmente a lista de pontos para o caminho do shake
            var path = new List<Vector3>();
            for (int i = 0; i < vibrato; i++)
            {
                // A força do tremor diminui ao longo do tempo
                float strengthFade = 1.0f - ((float)i / vibrato);
                path.Add(startValue + UnityEngine.Random.insideUnitSphere * strength * strengthFade);
            }
            // Garante que o último ponto seja exatamente o ponto inicial
            path.Add(startValue);

            // Chama o ATween com o caminho gerado
            target.ATween(propertyName, path, duration, Easing.OutQuad, onComplete);
        }
        
        
        /// <summary>
        /// Creates a punch animation on a property, moving it away and back to its starting point.
        /// </summary>
        /// <param name="target">The object containing the property to punch.</param>
        /// <param name="propertyName">The name of the property to punch (must be a Vector3).</param>
        /// <param name="punch">The direction and magnitude of the punch.</param>
        /// <param name="duration">The total duration of the punch and return animation.</param>
        /// <param name="onComplete">An optional callback for when the punch finishes.</param>
        public static void APunch(this object target, string propertyName, Vector3 punch, float duration, Action onComplete = null)
        {
            // Validação para garantir que a propriedade é um Vector3
            FieldInfo fieldInfo = target.GetType().GetField(propertyName, BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo propertyInfo = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if ((fieldInfo == null && propertyInfo == null) || (fieldInfo?.FieldType ?? propertyInfo?.PropertyType) != typeof(Vector3))
            {
                Debug.LogError($"AnimaTween APunch: Property '{propertyName}' not found or is not a Vector3.");
                return;
            }

            Vector3 startValue = (Vector3)(fieldInfo?.GetValue(target) ?? propertyInfo?.GetValue(target));
    
            // O caminho do punch é simples: vai até o pico do "soco" e volta ao início.
            var path = new []
            {
                startValue + punch,
                startValue
            };

            // Chama o ATween com o caminho gerado e um easing elástico para dar o efeito de "soco".
            target.ATween(propertyName, path, duration, Easing.OutElastic, onComplete);
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
            if (string.IsNullOrEmpty(propertyName)&&!internalCall)
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
                    if (withCallback && !internalCall) tweenInfo.OnComplete?.Invoke();
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
                    if (withCallback && !internalCall) tweenInfo.OnComplete?.Invoke();
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
    }
}