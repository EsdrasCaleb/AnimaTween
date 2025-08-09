using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection; // Essential for finding properties by name

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
        Forward // Default "one-way" playback
        // Future: Loop, Yoyo
    }

    // --- THE MAIN EXTENSION CLASS ---

    public static class AnimaTween
    {
        // The key is a Tuple: the target object AND the name of the property being animated.
        private static readonly Dictionary<Tuple<object, string>, Coroutine> _activeTweens
            = new Dictionary<Tuple<object, string>, Coroutine>();

        private static AnimaTweenRunner _runner;

        // Helper method to ensure our coroutine runner exists in the scene.
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
        /// Animates a property or field of an object over time.
        /// </summary>
        /// <param name="target">The object containing the property to be animated.</param>
        /// <param name="propertyName">The name of the public property or field.</param>
        /// <param name="toValue">The final value of the animation.</param>
        /// <param name="duration">The duration of the animation in seconds.</param>
        /// <param name="easing">The type of animation curve.</param>
        /// <param name="playback">The playback mode.</param>
        /// <param name="onComplete">A function to call when the animation is complete.</param>
        public static void AnimaTween(this object target, string propertyName, object toValue, float duration,
            Easing easing = Easing.Linear, Playback playback = Playback.Forward, Action onComplete = null)
        {
            // Try to find the Field or Property on the target object
            FieldInfo fieldInfo = target.GetType().GetField(propertyName, BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo propertyInfo = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

            // Validation
            if (fieldInfo == null && propertyInfo == null)
            {
                Debug.LogError($"AnimaTween: Property or Field '{propertyName}' not found or is not public on object '{target.GetType().Name}'.");
                return;
            }

            // --- Type Check and Coroutine Start ---
            // For now, only for integers as requested.
            if ((fieldInfo != null && fieldInfo.FieldType == typeof(int)) || (propertyInfo != null && propertyInfo.PropertyType == typeof(int)))
            {
                if (toValue is int endValue)
                {
                    var key = new Tuple<object, string>(target, propertyName);
                    
                    // Stop any previous tween on the same target/property
                    if (_activeTweens.TryGetValue(key, out Coroutine existing)) GetRunner().StopCoroutine(existing);
                    
                    // Start the new coroutine
                    Coroutine newCoroutine = GetRunner().StartCoroutine(AnimateIntCoroutine(target, fieldInfo, propertyInfo, endValue, duration, easing, playback, onComplete));
                    _activeTweens[key] = newCoroutine;
                }
                else
                {
                    Debug.LogError($"AnimaTween: The provided 'toValue' is not an integer (int) for property '{propertyName}'.");
                }
            }
            else
            {
                Debug.LogError($"AnimaTween: Property '{propertyName}' is not of type integer (int). Other types are not yet supported.");
            }
        }

        // --- PRIVATE COROUTINE FOR ANIMATING INTEGERS ---
        private static IEnumerator AnimateIntCoroutine(object target, FieldInfo field, PropertyInfo prop, int toValue, float duration, Easing easing, Playback playback, Action onComplete)
        {
            // Get the starting value using reflection
            int startValue = (field != null) ? (int)field.GetValue(target) : (int)prop.GetValue(target);
            
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = EasingFunctions.GetEasedProgress(easing, Mathf.Clamp01(elapsedTime / duration));
                
                // Calculate the interpolated value and round to the nearest integer
                int currentValue = (int)Mathf.Lerp((float)startValue, (float)toValue, progress);

                // Set the new value using reflection
                if (field != null) field.SetValue(target, currentValue);
                else prop.SetValue(target, currentValue);
                
                yield return null;
            }

            // Ensure the final value is set correctly
            if (field != null) field.SetValue(target, toValue);
            else prop.SetValue(target, toValue);
            
            // Clean up the dictionary and invoke the callback
            _activeTweens.Remove(new Tuple<object, string>(target, (field != null ? field.Name : prop.Name)));
            onComplete?.Invoke();
        }
    }

    // --- EASING HELPER CLASS ---
    public static class EasingFunctions
    {
        public static float GetEasedProgress(Easing ease, float progress)
        {
            switch (ease)
            {
                case Easing.Linear:
                default:
                    return progress;
            }
        }
    }
}