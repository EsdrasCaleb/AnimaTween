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
            Type targetType = tweenInfo.StartValue.GetType();

            if (targetType == typeof(string))
            {
                return AnimateString(tweenInfo, (string)start, (string)end, duration, easing);
            }

            Action<float> updater;
            
            if (targetType == typeof(float) || targetType == typeof(int) || targetType == typeof(double))
            {
                // Usa double para todos os cálculos para manter a máxima precisão.
                double s = Convert.ToDouble(start);
                double e = Convert.ToDouble(end);
    
                updater = p => {
                    // Lerp manual para double
                    double val = s + (e - s) * GetEasedProgress(easing, p);

                    // Converte de volta para o tipo original antes de definir o valor.
                    if (targetType == typeof(float))
                    {
                        tweenInfo.SetValue((float)val);
                    }
                    else if (targetType == typeof(int))
                    {
                        tweenInfo.SetValue(Convert.ToInt32(Math.Round(val)));
                    }
                    else // double
                    {
                        tweenInfo.SetValue(val);
                    }
                };
            }
            else if (targetType == typeof(Rect))
            {
                Rect s = (Rect)start;
                Rect e = (Rect)end;
                updater = p => tweenInfo.SetValue(LerpRect(s, e, GetEasedProgress(easing, p)));
            }
            else if (targetType == typeof(Bounds))
            {
                Bounds s = (Bounds)start;
                Bounds e = (Bounds)end;
                updater = p => tweenInfo.SetValue(LerpBounds(s, e, GetEasedProgress(easing, p)));
            }
            else if (targetType == typeof(Vector3))
            {
                Vector3 s = (Vector3)start;
                Vector3 e = (Vector3)end;
                updater = p => tweenInfo.SetValue(Vector3.Lerp(s, e, GetEasedProgress(easing, p)));
            }
            else if (targetType == typeof(Vector2))
            {
                Vector2 s = (Vector2)start;
                Vector2 e = (Vector2)end;
                updater = p => tweenInfo.SetValue(Vector2.Lerp(s, e, GetEasedProgress(easing, p)));
            }
            else if (targetType == typeof(Color))
            {
                Color s = (Color)start;
                Color e = (Color)end;
                updater = p => tweenInfo.SetValue(Color.Lerp(s, e, GetEasedProgress(easing, p)));
            }
            else if (targetType == typeof(Quaternion))
            {
                Quaternion s = (Quaternion)start;
                Quaternion e = (Quaternion)end;
                updater = p => tweenInfo.SetValue(Quaternion.Slerp(s, e, GetEasedProgress(easing, p)));
            }
            else
            {
                Debug.LogError($"AnimaTween: Unsupported property type for tweening: {targetType.Name}");
                return null;
            }

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
        /// A special coroutine to handle the typewriter effect for strings.
        /// </summary>
        private static IEnumerator AnimateString(TweenInfo tweenInfo, string startValue, string toValue, float duration, Easing easing)
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
        
        /// <summary>
        /// Interpola linearmente entre dois Rects. Uma função auxiliar para compatibilidade com versões mais antigas do Unity.
        /// </summary>
        /// <summary>
        /// Interpola linearmente entre dois Rects usando Vector2.Lerp para posição e tamanho.
        /// </summary>
        private static Rect LerpRect(Rect a, Rect b, float t)
        {
            // Interpola a posição (x, y) como um Vector2
            Vector2 newPosition = Vector2.Lerp(a.position, b.position, t);
    
            // Interpola o tamanho (width, height) como um Vector2
            Vector2 newSize = Vector2.Lerp(a.size, b.size, t);

            return new Rect(newPosition, newSize);
        }
        
        /// <summary>
        /// Interpola linearmente entre dois Bounds.
        /// </summary>
        private static Bounds LerpBounds(Bounds a, Bounds b, float t)
        {
            Vector3 center = Vector3.Lerp(a.center, b.center, t);
            Vector3 size = Vector3.Lerp(a.size, b.size, t);
            return new Bounds(center, size);
        }
    }
}