// AnimaTweenCoroutines.cs
using UnityEngine;
using System;
using System.Collections;



namespace AnimaTween
{
    internal static class AnimaTweenCoroutines
    {
        /// <summary>
        /// Checks if a Unity Object has been destroyed.
        /// </summary>
        public static bool IsTargetDestroyed(object target)
        {
            return target is UnityEngine.Object obj && obj == null;
        }

        /// <summary>
        /// The main dispatcher. It determines the correct interpolation logic based on the
        /// target's type and passes it to the AnimateCore engine.
        /// </summary>
        public static IEnumerator Animate(TweenInfo tweenInfo, float duration, Easing easing, bool isFrom)
        {
            object start = isFrom ? tweenInfo.ToValue : tweenInfo.StartValue;
            object end = isFrom ? tweenInfo.StartValue : tweenInfo.ToValue;
            tweenInfo.StartValue = start;
            tweenInfo.ToValue = end;
            Type targetType = tweenInfo.StartValue.GetType();

            if (targetType == typeof(string))
            {
                return AnimateString(tweenInfo, (string)start, (string)end, duration, easing);
            }

            Action<float> updater = p => tweenInfo.SetProgress(GetEasedProgress(easing, p));;

            return AnimateCore(updater, duration);
        }
        
        /// <summary>
        /// The generic animation engine. It handles the core loop of time and progress.
        /// </summary>
        private static IEnumerator AnimateCore(Action<float> updater, float duration)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime <= duration)
            {
                float progress = (duration > 0) ? elapsedTime / duration : 1f;
                updater(progress);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            // This final call ensures the animation always ends at exactly 100% progress.
            updater(1.0f);
        }
        
        /// <summary>
        /// A special coroutine to handle string animations.
        /// It performs a numeric tween if start/end values are numbers, otherwise does a typewriter effect.
        /// </summary>
        private static IEnumerator AnimateString(TweenInfo tweenInfo, string startValue, string toValue, float duration, Easing easing)
        {
            // Tenta converter as strings para números (double para abranger int e float).
            bool isStartNumeric = double.TryParse(startValue, out double startNum);
            bool isEndNumeric = double.TryParse(toValue, out double endNum);

            // Se AMBAS as strings forem numéricas, faz o tween numérico.
            if (isStartNumeric && isEndNumeric)
            {
                // Verifica se os números originais eram inteiros para manter o formato.
                bool isIntegerTween = startValue.IndexOf('.') == -1 && toValue.IndexOf('.') == -1;

                Action<float> updater = p =>
                {
                    float easedProgress = GetEasedProgress(easing, p);
                    double currentValue = startNum + (endNum - startNum) * easedProgress; // Lerp para double

                    // Converte o número de volta para string, formatando como inteiro se necessário.
                    string displayValue = isIntegerTween 
                        ? Mathf.RoundToInt((float)currentValue).ToString() 
                        : currentValue.ToString("F2"); // "F2" para formatar com 2 casas decimais, ajuste se necessário.
                    
                    tweenInfo.SetValue(displayValue);
                };

                return AnimateCore(updater, duration);
            }
            else // Caso contrário, mantém o efeito de máquina de escrever.
            {
                // Caso 1 e 2: Simples crescer ou encolher (uma string começa com a outra).
                if (toValue.StartsWith(startValue) || startValue.StartsWith(toValue))
                {
                    int startLength = startValue.Length;
                    int endLength = toValue.Length;
                    string baseString = endLength > startLength ? toValue : startValue;

                    Action<float> updater = p => {
                        float easedProgress = GetEasedProgress(easing, p);
                        int currentLength = Mathf.RoundToInt(Mathf.Lerp(startLength, endLength, easedProgress));
                        string currentValue = baseString.Substring(0, Mathf.Clamp(currentLength, 0, baseString.Length));
                        tweenInfo.SetValue(currentValue);
                    };

                    return AnimateCore(updater, duration);
                }
                // Caso 3: Substituir (as strings são diferentes).
                else
                {
                    // Isso requer uma sequência de dois tweens, então criamos uma corrotina especial para isso.
                    return AnimateStringReplace(tweenInfo, startValue, toValue, duration, easing);
                }
            }
        }
        
        
        /// <summary>
        /// A special coroutine to handle replacing one string with another by shrinking to a
        /// common prefix and then growing to the new string.
        /// </summary>
        private static IEnumerator AnimateStringReplace(TweenInfo tweenInfo, string startValue, string toValue, float duration, Easing easing)
        {
            // 1. Encontra o prefixo comum mais longo.
            int prefixLength = 0;
            while (prefixLength < startValue.Length && prefixLength < toValue.Length && startValue[prefixLength] == toValue[prefixLength])
            {
                prefixLength++;
            }
            string commonPrefix = startValue.Substring(0, prefixLength);

            // 2. Calcula a proporção da duração com base no número de caracteres a serem alterados.
            float shrinkChars = startValue.Length - prefixLength;
            float growChars = toValue.Length - prefixLength;
            float totalCharsChanged = shrinkChars + growChars;

            // Evita divisão por zero se as strings forem idênticas (embora este caso não deva ser alcançado).
            if (totalCharsChanged <= 0)
            {
                yield return new WaitForSeconds(duration);
                yield break;
            }

            float shrinkProportion = shrinkChars / totalCharsChanged;
            float growProportion = growChars / totalCharsChanged;

            float shrinkDuration = duration * shrinkProportion;
            float growDuration = duration * growProportion;

            // 3. Fase de Encolher: Anima de startValue até o prefixo comum.
            int shrinkStartLength = startValue.Length;
            int shrinkEndLength = commonPrefix.Length;
            Action<float> shrinkUpdater = p => {
                float easedProgress = GetEasedProgress(easing, p);
                int currentLength = Mathf.RoundToInt(Mathf.Lerp(shrinkStartLength, shrinkEndLength, easedProgress));
                string currentValue = startValue.Substring(0, Mathf.Clamp(currentLength, 0, startValue.Length));
                tweenInfo.SetValue(currentValue);
            };
            yield return AnimateCore(shrinkUpdater, shrinkDuration);

            // 4. Fase de Crescer: Anima do prefixo comum até o toValue.
            int growStartLength = commonPrefix.Length;
            int growEndLength = toValue.Length;
            Action<float> growUpdater = p => {
                float easedProgress = GetEasedProgress(easing, p);
                int currentLength = Mathf.RoundToInt(Mathf.Lerp(growStartLength, growEndLength, easedProgress));
                string currentValue = toValue.Substring(0, Mathf.Clamp(currentLength, 0, toValue.Length));
                tweenInfo.SetValue(currentValue);
            };
            yield return AnimateCore(growUpdater, growDuration);
        }


        /// <summary>
        /// Calculates the eased progress for a given Easing type.
        /// </summary>
        /// <param name="ease">The type of easing curve to use.</param>
        /// <param name="progress">The linear progress of the animation (0 to 1).</param>
        /// <returns>The eased progress (can be outside 0-1 for some types like Back or Elastic).</returns>
        private static float GetEasedProgress(Easing ease, float progress)
        {
            switch (ease)
            {
                // --- Back ---
                case Easing.InBack:
                {
                    const float c1 = 1.70158f;
                    const float c3 = c1 + 1;
                    return c3 * progress * progress * progress - c1 * progress * progress;
                }
                case Easing.OutBack:
                {
                    const float c1 = 1.70158f;
                    const float c3 = c1 + 1;
                    return 1 + c3 * Mathf.Pow(progress - 1, 3) + c1 * Mathf.Pow(progress - 1, 2);
                }
                case Easing.InOutBack:
                {
                    const float c1 = 1.70158f;
                    const float c2 = c1 * 1.525f;
                    return progress < 0.5
                        ? (Mathf.Pow(2 * progress, 2) * ((c2 + 1) * 2 * progress - c2)) / 2
                        : (Mathf.Pow(2 * progress - 2, 2) * ((c2 + 1) * (progress * 2 - 2) + c2) + 2) / 2;
                }
                case Easing.OutInBack:
                {
                    const float c1 = 1.70158f;
                    const float c3 = c1 + 1;
                    if (progress < 0.5f)
                        return (1 + c3 * Mathf.Pow(2 * progress - 1, 3) + c1 * Mathf.Pow(2 * progress - 1, 2)) / 2;
                    return (c3 * (2 * progress - 1) * (2 * progress - 1) * (2 * progress - 1) -
                        c1 * (2 * progress - 1) * (2 * progress - 1) + 1) / 2;
                }

                // --- Bounce ---
                case Easing.InBounce:
                    return 1 - OutBounce(1 - progress);
                case Easing.OutBounce:
                    return OutBounce(progress);
                case Easing.InOutBounce:
                    return progress < 0.5
                        ? (1 - OutBounce(1 - 2 * progress)) / 2
                        : (1 + OutBounce(2 * progress - 1)) / 2;
                case Easing.OutInBounce:
                    if (progress < 0.5f) return OutBounce(2 * progress) / 2;
                    return (1 - OutBounce(1 - (2 * progress - 1)) + 1) / 2;

                // --- Circ ---
                case Easing.InCirc:
                    return 1 - Mathf.Sqrt(1 - Mathf.Pow(progress, 2));
                case Easing.OutCirc:
                    return Mathf.Sqrt(1 - Mathf.Pow(progress - 1, 2));
                case Easing.InOutCirc:
                    return progress < 0.5
                        ? (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * progress, 2))) / 2
                        : (Mathf.Sqrt(1 - Mathf.Pow(-2 * progress + 2, 2)) + 1) / 2;
                case Easing.OutInCirc:
                    if (progress < 0.5f) return Mathf.Sqrt(1 - Mathf.Pow(2 * progress - 1, 2)) / 2;
                    return (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * progress - 1, 2)) + 1) / 2;

                // --- Cubic ---
                case Easing.InCubic:
                    return progress * progress * progress;
                case Easing.OutCubic:
                    return 1 - Mathf.Pow(1 - progress, 3);
                case Easing.InOutCubic:
                    return progress < 0.5
                        ? 4 * progress * progress * progress
                        : 1 - Mathf.Pow(-2 * progress + 2, 3) / 2;
                case Easing.OutInCubic:
                    if (progress < 0.5f) return (1 - Mathf.Pow(1 - 2 * progress, 3)) / 2;
                    return ((2 * progress - 1) * (2 * progress - 1) * (2 * progress - 1) + 1) / 2;

                // --- Elastic ---
                case Easing.InElastic:
                {
                    const float c4 = (2 * Mathf.PI) / 3;
                    if (progress == 0) return 0;
                    if (progress == 1) return 1;
                    return -Mathf.Pow(2, 10 * progress - 10) * Mathf.Sin((progress * 10 - 10.75f) * c4);
                }
                case Easing.OutElastic:
                {
                    const float c4 = (2 * Mathf.PI) / 3;
                    if (progress == 0) return 0;
                    if (progress == 1) return 1;
                    return Mathf.Pow(2, -10 * progress) * Mathf.Sin((progress * 10 - 0.75f) * c4) + 1;
                }
                case Easing.InOutElastic:
                {
                    const float c5 = (2 * Mathf.PI) / 4.5f;
                    if (progress == 0) return 0;
                    if (progress == 1) return 1;
                    return progress < 0.5
                        ? -(Mathf.Pow(2, 20 * progress - 10) * Mathf.Sin((20 * progress - 11.125f) * c5)) / 2
                        : (Mathf.Pow(2, -20 * progress + 10) * Mathf.Sin((20 * progress - 11.125f) * c5)) / 2 + 1;
                }
                case Easing.OutInElastic:
                {
                    const float c4 = (2 * Mathf.PI) / 3;
                    if (progress < 0.5f)
                        return (Mathf.Pow(2, -20 * progress) * Mathf.Sin((20 * progress - 0.75f) * c4) + 1) / 2;
                    return (-Mathf.Pow(2, 20 * progress - 20) * Mathf.Sin((20 * progress - 20.75f) * c4) + 1) / 2;
                }

                // --- Expo ---
                case Easing.InExpo:
                    return progress == 0 ? 0 : Mathf.Pow(2, 10 * progress - 10);
                case Easing.OutExpo:
                    return progress == 1 ? 1 : 1 - Mathf.Pow(2, -10 * progress);
                case Easing.InOutExpo:
                    if (progress == 0) return 0;
                    if (progress == 1) return 1;
                    return progress < 0.5
                        ? Mathf.Pow(2, 20 * progress - 10) / 2
                        : (2 - Mathf.Pow(2, -20 * progress + 10)) / 2;
                case Easing.OutInExpo:
                    if (progress < 0.5f) return (1 - Mathf.Pow(2, -20 * progress)) / 2;
                    return (Mathf.Pow(2, 20 * progress - 20) + 1) / 2;

                // --- Quad ---
                case Easing.InQuad:
                    return progress * progress;
                case Easing.OutQuad:
                    return 1 - (1 - progress) * (1 - progress);
                case Easing.InOutQuad:
                    return progress < 0.5 ? 2 * progress * progress : 1 - Mathf.Pow(-2 * progress + 2, 2) / 2;
                case Easing.OutInQuad:
                    if (progress < 0.5f) return (1 - (1 - 2 * progress) * (1 - 2 * progress)) / 2;
                    return ((2 * progress - 1) * (2 * progress - 1) + 1) / 2;

                // --- Quart ---
                case Easing.InQuart:
                    return progress * progress * progress * progress;
                case Easing.OutQuart:
                    return 1 - Mathf.Pow(1 - progress, 4);
                case Easing.InOutQuart:
                    return progress < 0.5
                        ? 8 * progress * progress * progress * progress
                        : 1 - Mathf.Pow(-2 * progress + 2, 4) / 2;
                case Easing.OutInQuart:
                    if (progress < 0.5f) return (1 - Mathf.Pow(1 - 2 * progress, 4)) / 2;
                    return (Mathf.Pow(2 * progress - 1, 4) + 1) / 2;

                // --- Quint ---
                case Easing.InQuint:
                    return progress * progress * progress * progress * progress;
                case Easing.OutQuint:
                    return 1 - Mathf.Pow(1 - progress, 5);
                case Easing.InOutQuint:
                    return progress < 0.5
                        ? 16 * progress * progress * progress * progress * progress
                        : 1 - Mathf.Pow(-2 * progress + 2, 5) / 2;
                case Easing.OutInQuint:
                    if (progress < 0.5f) return (1 - Mathf.Pow(1 - 2 * progress, 5)) / 2;
                    return (Mathf.Pow(2 * progress - 1, 5) + 1) / 2;

                // --- Sine ---
                case Easing.InSine:
                    return 1 - Mathf.Cos((progress * Mathf.PI) / 2);
                case Easing.OutSine:
                    return Mathf.Sin((progress * Mathf.PI) / 2);
                case Easing.InOutSine:
                    return -(Mathf.Cos(Mathf.PI * progress) - 1) / 2;
                case Easing.OutInSine:
                    if (progress < 0.5f) return Mathf.Sin((2 * progress * Mathf.PI) / 2) / 2;
                    return (1 - Mathf.Cos(((2 * progress - 1) * Mathf.PI) / 2) + 1) / 2;

                // --- Linear (Default) ---
                case Easing.Linear:
                default:
                    return progress;
            }
        }

        // Private helper for Bounce eases
        private static float OutBounce(float x)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (x < 1 / d1)
            {
                return n1 * x * x;
            }

            if (x < 2 / d1)
            {
                return n1 * (x -= 1.5f / d1) * x + 0.75f;
            }

            if (x < 2.5 / d1)
            {
                return n1 * (x -= 2.25f / d1) * x + 0.9375f;
            }

            return n1 * (x -= 2.625f / d1) * x + 0.984375f;
        }
    }
}