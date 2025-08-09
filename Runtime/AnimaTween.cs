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
        /// <summary>
        /// Stops all active tweens on the target object.
        /// </summary>
        public static void Complete(this object target)
        {
            // Create a temporary list of keys to remove, to avoid modifying the dictionary while iterating it.
            var keysToRemove = new List<Tuple<object, string>>();

            foreach (var pair in _activeTweens)
            {
                // Check if the target object of the running tween is the one we want to stop.
                if (pair.Key.Item1 == target)
                {
                    GetRunner().StopCoroutine(pair.Value);
                    keysToRemove.Add(pair.Key);
                }
            }

            // Remove all completed tweens from the tracking dictionary.
            foreach (var key in keysToRemove)
            {
                _activeTweens.Remove(key);
            }
        }

        /// <summary>
        /// Stops a specific tween on the target object based on its property name.
        /// </summary>
        public static void Complete(this object target, string propertyName, bool internalCall = false)
        {
            var key = new Tuple<object, string>(target, propertyName);
            if (_activeTweens.TryGetValue(key, out Coroutine existing))
            {
                if (!internalCall)
                {
                    Debug.Log($"AnimaTween: Tween for '{propertyName}' was completed manually.");
                }
                GetRunner().StopCoroutine(existing);
                _activeTweens.Remove(key);
            }
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
        // This is the updated main function inside your AnimaTweenExtensions static class.

        public static void AnimaTween(this object target, string propertyName, object toValue, float duration,
            Easing easing = Easing.Linear, Playback playback = Playback.Forward, Action onComplete = null)
        {
            target.Complete(propertyName, internalCall: true);

            FieldInfo fieldInfo = target.GetType().GetField(propertyName, BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo propertyInfo = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (toValue is IEnumerable path && !(toValue is string)) // Exclude strings
            {
                // Convert the collection to a queue of objects to process them in order.
                var waypoints = new Queue<object>(path.Cast<object>());

                if (waypoints.Count > 0)
                {
                    // Calculate duration per segment
                    float segmentDuration = duration / waypoints.Count;
                    // Start the recursive path animation
                    AnimatePath(target, propertyName, waypoints, segmentDuration, easing, playback, onComplete);
                }
                return; // End execution here for paths
            }
            if (fieldInfo == null && propertyInfo == null)
            {
                Debug.LogError($"AnimaTween: Property or Field '{propertyName}' not found or is not public on object '{target.GetType().Name}'.");
                return;
            }

            Type propertyType = fieldInfo != null ? fieldInfo.FieldType : propertyInfo.PropertyType;
            var key = new Tuple<object, string>(target, propertyName);
            if (_activeTweens.TryGetValue(key, out Coroutine existing)) GetRunner().StopCoroutine(existing);

            // --- NEW: Expanded type checking ---

            Coroutine newCoroutine = null;

            if (propertyType == typeof(int) || propertyType == typeof(float))
            {
                try
                {
                    float startValue = Convert.ToSingle(fieldInfo != null ? fieldInfo.GetValue(target) : propertyInfo.GetValue(target));
                    float endValue = Convert.ToSingle(toValue);
                    newCoroutine = GetRunner().StartCoroutine(AnimateNumericCoroutine(target, fieldInfo, propertyInfo, propertyType, startValue, endValue, duration, easing, playback, onComplete));
                }
                catch (Exception e) { /* Error handling */ }
            }
            else if (propertyType == typeof(Vector2) && toValue is Vector2 endVec2)
            {
                newCoroutine = GetRunner().StartCoroutine(AnimateVector2Coroutine(target, fieldInfo, propertyInfo, endVec2, duration, easing, playback, onComplete));
            }
            else if (propertyType == typeof(Vector3) && toValue is Vector3 endVec3)
            {
                newCoroutine = GetRunner().StartCoroutine(AnimateVector3Coroutine(target, fieldInfo, propertyInfo, endVec3, duration, easing, playback, onComplete));
            }
            else if (propertyType == typeof(Color) && toValue is Color endColor)
            {
                newCoroutine = GetRunner().StartCoroutine(AnimateColorCoroutine(target, fieldInfo, propertyInfo, endColor, duration, easing, playback, onComplete));
            }
            // Add this block inside your main AnimaTween function, alongside the other type checks.
            else if (propertyType == typeof(Quaternion) && toValue is Quaternion endQuat)
            {
                newCoroutine = GetRunner().StartCoroutine(AnimateQuaternionCoroutine(target, fieldInfo, propertyInfo, endQuat, duration, easing, playback, onComplete));
            }
            else if (propertyType == typeof(string) && toValue is string endString)
            {
                string startString = (fieldInfo != null ? (string)fieldInfo.GetValue(target) : (string)propertyInfo.GetValue(target)) ?? "";
    
                // Case 1: Grow Text (e.g., "Hello" -> "Hello World")
                if (endString.StartsWith(startString))
                {
                    newCoroutine = GetRunner().StartCoroutine(AnimateStringCoroutine(target, fieldInfo, propertyInfo, startString, endString, duration, easing, playback, onComplete));
                }
                // Case 2: Shrink Text (e.g., "Hello World" -> "Hello")
                else if (startString.StartsWith(endString))
                {
                    newCoroutine = GetRunner().StartCoroutine(AnimateStringCoroutine(target, fieldInfo, propertyInfo, startString, endString, duration, easing, playback, onComplete));
                }
                // Case 3: Replace Text (e.g., "Caleb" -> "AnimaTween")
                else
                {
                    // Find the nearest common prefix
                    int prefixLength = 0;
                    while (prefixLength < startString.Length && prefixLength < endString.Length && startString[prefixLength] == endString[prefixLength])
                    {
                        prefixLength++;
                    }
                    string commonPrefix = startString.Substring(0, prefixLength);
        
                    float shrinkDuration = duration * 0.5f;
                    float growDuration = duration * 0.5f;

                    // Chain two tweens: first shrink to the common prefix, then grow to the end string.
                    newCoroutine = GetRunner().StartCoroutine(AnimateStringCoroutine(target, fieldInfo, propertyInfo, startString, commonPrefix, shrinkDuration, easing, playback, () => {
                        // This is the onComplete of the shrink animation. It starts the grow animation.
                        target.AnimaTween(propertyName, endString, growDuration, easing, playback, onComplete);
                    }));
                }
            }

            else
            {
                Debug.LogError($"AnimaTween: Property '{propertyName}' has an unsupported type ({propertyType.Name}) or the provided 'toValue' does not match.");
                return;
            }

            if (newCoroutine != null)
            {
                _activeTweens[key] = newCoroutine;
            }
        }
        
        //Helper functions all can be done with just AnimaTween
        // --- HELPER SHORTCUT FUNCTIONS ---

        /// <summary>
        /// A shortcut function to fade common component types.
        /// It automatically detects the component type and animates the correct property.
        /// </summary>
        /// <param name="target">The component to fade (e.g., Image, SpriteRenderer, CanvasGroup).</param>
        /// <param name="toAlpha">The target alpha value (0 for transparent, 1 for opaque).</param>
        /// <param name="duration">The duration of the animation in seconds.</param>
        public static void AnimaFade(this Component target, float toAlpha, float duration, Easing easing = Easing.Linear, Action onComplete = null)
        {
            // Check if the target is a UI Graphic (Image, Text, RawImage)
            if (target is Graphic graphic)
            {
                // For Graphics, we need to animate the 'color' property.
                // We create a target color with the desired alpha.
                Color targetColor = graphic.color;
                targetColor.a = toAlpha;
                graphic.AnimaTween("color", targetColor, duration, easing, playback: Playback.Forward, onComplete);
            }
            // Check if the target is a CanvasGroup
            else if (target is CanvasGroup canvasGroup)
            {
                // For CanvasGroup, we animate the 'alpha' property directly.
                canvasGroup.AnimaTween("alpha", toAlpha, duration, easing, playback: Playback.Forward, onComplete);
            }
            // Check if the target is a SpriteRenderer
            else if (target is SpriteRenderer sprite)
            {
                // For Sprites, we also animate the 'color' property.
                Color targetColor = sprite.color;
                targetColor.a = toAlpha;
                sprite.AnimaTween("color", targetColor, duration, easing, playback: Playback.Forward, onComplete);
            }
            // If the component is not a supported type, log an error.
            else
            {
                Debug.LogError($"AnimaFade: The target component '{target.GetType().Name}' is not a supported type for fading. Supported types are Graphic (Image, Text), CanvasGroup, and SpriteRenderer.");
            }
        }
        
        
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
        
        private static Coroutine StartTween(object target, string propertyName, IEnumerator coroutine)
        {
            // 1. Stop any existing tween on this exact property to prevent conflicts.
            var key = new Tuple<object, string>(target, propertyName);
            if (_activeTweens.TryGetValue(key, out Coroutine existing))
            {
                GetRunner().StopCoroutine(existing);
            }
    
            // 2. Start the new coroutine (this is the part you correctly identified).
            Coroutine newCoroutine = GetRunner().StartCoroutine(coroutine);

            // 3. Add it to our tracking dictionary.
            _activeTweens[key] = newCoroutine;
    
            return newCoroutine;
        }
      


        // --- PRIVATE COROUTINE FOR ANIMATING INTEGERS ---
        // --- UNIFIED COROUTINE FOR ALL NUMERIC TYPES ---
        private static IEnumerator AnimateNumericCoroutine(object target, FieldInfo field, PropertyInfo prop, Type originalType, float startValue, float toValue, float duration, Easing easing, Playback playback, Action onComplete)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = EasingFunctions.GetEasedProgress(easing, Mathf.Clamp01(elapsedTime / duration));
        
                // Always calculate the interpolation as a float
                float currentValue = Mathf.Lerp(startValue, toValue, progress);

                // Convert the result back to the original type before setting it
                object valueToSet;
                if (originalType == typeof(int))
                {
                    valueToSet = Mathf.RoundToInt(currentValue);
                }
                else // It's a float
                {
                    valueToSet = currentValue;
                }
        
                // Set the value using reflection
                if (field != null) field.SetValue(target, valueToSet);
                else prop.SetValue(target, valueToSet);
        
                yield return null;
            }

            // Ensure the final value is set correctly, respecting the original type
            object finalValueToSet;
            if (originalType == typeof(int))
            {
                finalValueToSet = Mathf.RoundToInt(toValue);
            }
            else
            {
                finalValueToSet = toValue;
            }

            if (field != null) field.SetValue(target, finalValueToSet);
            else prop.SetValue(target, finalValueToSet);
            
            _activeTweens.Remove(new Tuple<object, string>(target, (field != null ? field.Name : prop.Name)));
            onComplete?.Invoke();
        }
        
        // --- NEW COROUTINE FOR VECTOR2 ---
        private static IEnumerator AnimateVector2Coroutine(object target, FieldInfo field, PropertyInfo prop, Vector2 toValue, float duration, Easing easing, Playback playback, Action onComplete)
        {
            Vector2 startValue = (field != null) ? (Vector2)field.GetValue(target) : (Vector2)prop.GetValue(target);
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = EasingFunctions.GetEasedProgress(easing, Mathf.Clamp01(elapsedTime / duration));
                
                // The key difference is using Vector2.Lerp here
                Vector2 currentValue = Vector2.Lerp(startValue, toValue, progress);

                if (field != null) field.SetValue(target, currentValue);
                else prop.SetValue(target, currentValue);
                yield return null;
            }
            if (field != null) field.SetValue(target, toValue);
            else prop.SetValue(target, toValue);
            _activeTweens.Remove(new Tuple<object, string>(target, (field != null ? field.Name : prop.Name)));
            onComplete?.Invoke();
        }

        // --- NEW COROUTINE FOR VECTOR3 ---
        private static IEnumerator AnimateVector3Coroutine(object target, FieldInfo field, PropertyInfo prop, Vector3 toValue, float duration, Easing easing, Playback playback, Action onComplete)
        {
            Vector3 startValue = (field != null) ? (Vector3)field.GetValue(target) : (Vector3)prop.GetValue(target);
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = EasingFunctions.GetEasedProgress(easing, Mathf.Clamp01(elapsedTime / duration));
                
                // The key difference is using Vector3.Lerp here
                Vector3 currentValue = Vector3.Lerp(startValue, toValue, progress);

                if (field != null) field.SetValue(target, currentValue);
                else prop.SetValue(target, currentValue);
                yield return null;
            }
            if (field != null) field.SetValue(target, toValue);
            else prop.SetValue(target, toValue);
            _activeTweens.Remove(new Tuple<object, string>(target, (field != null ? field.Name : prop.Name)));
            onComplete?.Invoke();
        }
        
        private static IEnumerator AnimateColorCoroutine(object target, FieldInfo field, PropertyInfo prop, Color toValue, float duration, Easing easing, Playback playback, Action onComplete)
        {
            Color startValue = (field != null) ? (Color)field.GetValue(target) : (Color)prop.GetValue(target);
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = EasingFunctions.GetEasedProgress(easing, Mathf.Clamp01(elapsedTime / duration));
                
                // The key difference is using Color.Lerp here
                Color currentValue = Color.Lerp(startValue, toValue, progress);

                if (field != null) field.SetValue(target, currentValue);
                else prop.SetValue(target, currentValue);
                yield return null;
            }
            if (field != null) field.SetValue(target, toValue);
            else prop.SetValue(target, toValue);
            _activeTweens.Remove(new Tuple<object, string>(target, (field != null ? field.Name : prop.Name)));
            onComplete?.Invoke();
        }
        
        // --- NEW COROUTINE FOR QUATERNION (ROTATION) ---
        private static IEnumerator AnimateQuaternionCoroutine(object target, FieldInfo field, PropertyInfo prop, Quaternion toValue, float duration, Easing easing, Playback playback, Action onComplete)
        {
            // Get the starting value using reflection
            Quaternion startValue = (field != null) ? (Quaternion)field.GetValue(target) : (Quaternion)prop.GetValue(target);

            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = EasingFunctions.GetEasedProgress(easing, Mathf.Clamp01(elapsedTime / duration));

                // The key difference is using Quaternion.Slerp here for smooth rotation
                Quaternion currentValue = Quaternion.Slerp(startValue, toValue, progress);

                // Set the new value using reflection
                if (field != null) field.SetValue(target, currentValue);
                else prop.SetValue(target, currentValue);
        
                yield return null;
            }

            // Ensure the final value is set correctly
            if (field != null) field.SetValue(target, toValue);
            else prop.SetValue(target, toValue);

            _activeTweens.Remove(new Tuple<object, string>(target, (field != null ? field.Name : prop.Name)));
            onComplete?.Invoke();
        }
        
        // --- NEW COROUTINE FOR STRING (TYPEWRITER EFFECT) ---
        private static IEnumerator AnimateStringCoroutine(object target, FieldInfo field, PropertyInfo prop, string startValue, string toValue, float duration, Easing easing, Playback playback, Action onComplete)
        {
            int startLength = startValue.Length;
            int endLength = toValue.Length;

            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = EasingFunctions.GetEasedProgress(easing, Mathf.Clamp01(elapsedTime / duration));

                // Use the progress to calculate the target length of the string
                int currentLength = Mathf.RoundToInt(Mathf.Lerp(startLength, endLength, progress));
        
                // Determine which base string to use (the longer one)
                string baseString = endLength > startLength ? toValue : startValue;
                string currentValue = baseString.Substring(0, currentLength);
        
                // Set the value using reflection
                if (field != null) field.SetValue(target, currentValue);
                else prop.SetValue(target, currentValue);
        
                yield return null;
            }

            // Ensure the final value is set correctly
            if (field != null) field.SetValue(target, toValue);
            else prop.SetValue(target, toValue);

            _activeTweens.Remove(new Tuple<object, string>(target, (field != null ? field.Name : prop.Name)));
            onComplete?.Invoke();
        }
        
          
        // --- NEW: PATH CONDUCTOR HELPER ---
        private static void AnimatePath(object target, string propertyName, Queue<object> waypoints, float segmentDuration, Easing easing, Playback playback, Action onComplete)
        {
            // Stop condition: if there are no more waypoints, call the final onComplete callback and finish.
            if (waypoints.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            // Get the next waypoint from the queue.
            object nextWaypoint = waypoints.Dequeue();

            // Define what to do when this segment of the path is complete.
            Action nextAction = () => {
                // Recursively call this same function to process the rest of the queue.
                AnimatePath(target, propertyName, waypoints, segmentDuration, easing, playback, onComplete);
            };

            // Start a regular tween for just this segment of the path.
            // The onComplete callback will trigger the next segment.
            target.AnimaTween(propertyName, nextWaypoint, segmentDuration, easing, playback, nextAction);
        }
    }
}