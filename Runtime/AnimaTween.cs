using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
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

    public enum InCaseOfDestruction
    {
        CancelCallback,
        CallCallback,
        CallCallbackInTheEnd
    }

    
    public static class AnimaTweenExtensions
    {
        private static readonly Dictionary<Tuple<object, string>, TweenInfo> activeTweens = new();
        // --- PUBLIC API METHODS ---

        public static void ATween(this object target, string propertyName, object toValue, float duration, 
            Easing easing = Easing.Linear,  Action onComplete = null, Playback playback = Playback.Forward,
            object fromValue = null)
        {
            target.ACancel(propertyName);

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
            
            var cts = new CancellationTokenSource();
            
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
            
            var tweenKey = new Tuple<object, string>(target, propertyName);
            
            activeTweens[tweenKey] = tweenInfo;
            
            TweenConductorAsync(tweenKey, tweenInfo, duration, easing, playback);
        }
        
        public static int ATimeout(this object target, float time, Action callback, bool repeat = false)
        {
            // Assumindo que TweenInfo agora pode armazenar o Cts
            var tweenInfo = new TweenInfo(target, callback); 

            // Lógica de ID corrigida para olhar apenas os timers do 'target' específico.
            int nextId = activeTweens.Keys
                .Where(key => key.Item1 == target && key.Item2.StartsWith("@timer_"))
                .Select(key => int.TryParse(key.Item2.Substring("@timer_".Length), out int id) ? id : -1)
                .DefaultIfEmpty(-1).Max() + 1;

            string timerName = $"@timer_{nextId}";
            var tweenKey = new Tuple<object, string>(target, timerName);

            activeTweens[tweenKey] = tweenInfo;

            // Chama a função async única, sem aninhar outras.
            RunTimerLogicAsync(tweenKey, tweenInfo, time, repeat);

            return nextId;
        }
        
        /// <summary>
        /// A única função async que gere toda a lógica do timer.
        /// </summary>
        private static async void RunTimerLogicAsync(Tuple<object, string> key, TweenInfo tweenInfo, float time, bool repeat)
        {
            try
            {

                do
                {
                    await Awaitable.WaitForSecondsAsync(time, tweenInfo.CTS.Token);
                    tweenInfo.OnComplete?.Invoke();
                } while (repeat);
            }
            catch (OperationCanceledException)
            {
                // A tarefa foi cancelada. A função de controlo (ex: ACompleteTimer) trata da limpeza.
            }
            finally
            {
                activeTweens.Remove(key);
            }
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
        public static void ACompleteTimer(this object target, int timerId = -1, bool withCallback = true)
        {
            // Caso 1: Completar TODOS os timers do target.
            if (timerId == -1)
            {
                foreach (var kvp in activeTweens
                             .Where(kvp => kvp.Key.Item1 == target && kvp.Key.Item2.StartsWith("@timer_"))
                             .ToList())
                {
                    target.AComplete(kvp.Key.Item2, withCallback);
                }
            }
            else
            {
                string timerName = $"@timer_{timerId}";

                target.AComplete(timerName, withCallback);
            }
        }
        
        public static void AComplete(this object target, string propertyName = null, bool withCallback = true, EndState endState = EndState.End)
        {
            if (string.IsNullOrEmpty(propertyName))
            {

                foreach (var kvp in activeTweens
                             .Where(kvp => kvp.Key.Item1 == target)
                             .ToList())
                {
                    target.AComplete(kvp.Key.Item2, withCallback, endState);
                }
            }
            else
            {
                var tweenKey = new Tuple<object, string>(target, propertyName);
                if (activeTweens.TryGetValue(tweenKey, out TweenInfo tweenInfo))
                {
                    tweenInfo.CTS?.Cancel();

                    if (endState == EndState.End && tweenInfo.ToValue != null)
                    {
                        // Assumindo que TweenInfo tem um método SetValue(object) que sabe como
                        // definir o valor final, independentemente do tipo.
                        tweenInfo.SetValue(tweenInfo.ToValue); 
                    }
                    else if (endState == EndState.Start && tweenInfo.StartValue != null)
                    {
                        tweenInfo.SetValue(tweenInfo.StartValue);
                    }
                    
                    if (withCallback)
                    {
                        tweenInfo.OnComplete?.Invoke();
                    }
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
        /// A nova função "fire-and-forget" que gere o ciclo de vida completo de um tween.
        /// Substitui a antiga TweenConductorCoroutine.
        /// </summary>
        private static async void TweenConductorAsync(
            Tuple<object, string> key, 
            TweenInfo tweenInfo, 
            float duration, 
            Easing easing, 
            Playback playback)
        {
            try
            {
                bool isLooping = playback == Playback.LoopForward || playback == Playback.LoopBackward ||
                                 playback == Playback.LoopPingPong;

                do
                {
                    // --- FORWARD Animation ---
                    if (playback == Playback.Forward || playback == Playback.PingPong ||
                        playback == Playback.LoopForward || playback == Playback.LoopPingPong)
                    {
                        // Chama e espera pela versão async do Animate, passando o token de cancelamento.
                        await AnimaTweenCoroutines.AnimateAsync(tweenInfo, duration, easing, isFrom: false);
                    }

                    // --- BACKWARD Animation ---
                    if (playback == Playback.Backward || playback == Playback.PingPong ||
                        playback == Playback.LoopBackward || playback == Playback.LoopPingPong)
                    {
                        await AnimaTweenCoroutines.AnimateAsync(tweenInfo, duration, easing, isFrom: true);
                    }

                } while (isLooping);

                // --- Conclusão Natural ---
                // Este código só é alcançado quando um tween não-looping termina.
                tweenInfo.OnComplete?.Invoke();
            }
            catch (OperationCanceledException)
            {
                // A tarefa foi cancelada por AComplete, AStop, etc.
                // A limpeza do dicionário já foi tratada pela função que cancelou.
            }
            finally
            {
                activeTweens.Remove(key);
            }
        }
    }
}