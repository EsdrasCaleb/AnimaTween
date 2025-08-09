// AnimaTweenCoroutines.cs
using UnityEngine;
using System;
using System.Collections;
using System.Reflection;

namespace AnimaTween
{
    /// <summary>
    /// Classe auxiliar interna que contém as corrotinas de animação puras.
    /// Elas apenas lidam com a interpolação de valores ao longo do tempo.
    /// </summary>
    internal static class AnimaTweenCoroutines
    {
        // Corrotina genérica para tipos numéricos (float, int)
        internal static IEnumerator AnimateNumeric(object target, FieldInfo field, PropertyInfo prop, Type originalType, float startValue, float toValue, float duration, Easing easing)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = EasingFunctions.GetEasedProgress(easing, Mathf.Clamp01(elapsedTime / duration));
                float currentValue = Mathf.Lerp(startValue, toValue, progress);

                object valueToSet = (originalType == typeof(int)) ? (object)Mathf.RoundToInt(currentValue) : currentValue;
                
                if (field != null) field.SetValue(target, valueToSet);
                else prop.SetValue(target, valueToSet);

                yield return null;
            }
            
            object finalValue = (originalType == typeof(int)) ? (object)Mathf.RoundToInt(toValue) : toValue;
            if (field != null) field.SetValue(target, finalValue);
            else prop.SetValue(target, finalValue);
        }

        // Corrotina para Vector2
        internal static IEnumerator AnimateVector2(object target, FieldInfo field, PropertyInfo prop, Vector2 startValue, Vector2 toValue, float duration, Easing easing)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = EasingFunctions.GetEasedProgress(easing, Mathf.Clamp01(elapsedTime / duration));
                Vector2 currentValue = Vector2.Lerp(startValue, toValue, progress);

                if (field != null) field.SetValue(target, currentValue);
                else prop.SetValue(target, currentValue);
                yield return null;
            }
            if (field != null) field.SetValue(target, toValue);
            else prop.SetValue(target, toValue);
        }

        // Corrotina para Vector3
        internal static IEnumerator AnimateVector3(object target, FieldInfo field, PropertyInfo prop, Vector3 startValue, Vector3 toValue, float duration, Easing easing)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = EasingFunctions.GetEasedProgress(easing, Mathf.Clamp01(elapsedTime / duration));
                Vector3 currentValue = Vector3.Lerp(startValue, toValue, progress);

                if (field != null) field.SetValue(target, currentValue);
                else prop.SetValue(target, currentValue);
                yield return null;
            }
            if (field != null) field.SetValue(target, toValue);
            else prop.SetValue(target, toValue);
        }

        // Corrotina para Color
        internal static IEnumerator AnimateColor(object target, FieldInfo field, PropertyInfo prop, Color startValue, Color toValue, float duration, Easing easing)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = EasingFunctions.GetEasedProgress(easing, Mathf.Clamp01(elapsedTime / duration));
                Color currentValue = Color.Lerp(startValue, toValue, progress);

                if (field != null) field.SetValue(target, currentValue);
                else prop.SetValue(target, currentValue);
                yield return null;
            }
            if (field != null) field.SetValue(target, toValue);
            else prop.SetValue(target, toValue);
        }

        // Corrotina para Quaternion
        internal static IEnumerator AnimateQuaternion(object target, FieldInfo field, PropertyInfo prop, Quaternion startValue, Quaternion toValue, float duration, Easing easing)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = EasingFunctions.GetEasedProgress(easing, Mathf.Clamp01(elapsedTime / duration));
                Quaternion currentValue = Quaternion.Slerp(startValue, toValue, progress);

                if (field != null) field.SetValue(target, currentValue);
                else prop.SetValue(target, currentValue);
                yield return null;
            }
            if (field != null) field.SetValue(target, toValue);
            else prop.SetValue(target, toValue);
        }

        // Corrotina para String (Typewriter)
        internal static IEnumerator AnimateString(object target, FieldInfo field, PropertyInfo prop, string startValue, string toValue, float duration, Easing easing)
        {
            int startLength = startValue.Length;
            int endLength = toValue.Length;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = EasingFunctions.GetEasedProgress(easing, Mathf.Clamp01(elapsedTime / duration));
                int currentLength = Mathf.RoundToInt(Mathf.Lerp(startLength, endLength, progress));
                string baseString = endLength > startLength ? toValue : startValue;
                string currentValue = baseString.Substring(0, Mathf.Clamp(currentLength, 0, baseString.Length));

                if (field != null) field.SetValue(target, currentValue);
                else prop.SetValue(target, currentValue);
                yield return null;
            }
            if (field != null) field.SetValue(target, toValue);
            else prop.SetValue(target, toValue);
        }
    }
}