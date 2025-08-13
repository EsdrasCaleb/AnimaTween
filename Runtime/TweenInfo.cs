using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace AnimaTween
{
    internal class TweenInfo
    {
        // --- Propriedades Gerais ---
        public Coroutine Coroutine { get; set; }
        public Action OnComplete { get; set; }
        public object Target { get; set; }
        public object StartValue { get; set; }
        public object ToValue { get; set; }
        public FieldInfo FieldInfo { get; set; }
        public PropertyInfo PropertyInfo { get; set; }

        // --- Optimized Path (Direct Access Delegates) ---
        // Delegates that store the action of setting the value directly.
        private Action<float> _floatSetter;
        private Action<int> _intSetter;
        private Action<double> _doubleSetter;
        private Action<Color> _colorSetter;
        private Action<Vector2> _vector2Setter;
        private Action<Vector3> _vector3Setter;
        private Action<Vector4> _vector4Setter;
        private Action<Quaternion> _quaternionSetter;
        private Action<Rect> _rectSetter;
        private Action<Bounds> _boundsSetter;
        private Action<string> _stringSetter;



        //Constructor to Timers
        public TweenInfo(object target, Action onComplete)
        {
            Target = target;
            OnComplete = onComplete;
        }
        
        
        /// <summary>
        /// The constructor is now the brain. It tries to find an optimized path
        /// and, if it can't, falls back to reflection.
        /// </summary>
        public TweenInfo(object target, string propertyName, Action onComplete, object startValue, object toValue, 
            PropertyInfo propertyInfo, FieldInfo fieldInfo)
        {
            Target = target;
            OnComplete = onComplete;
            StartValue = startValue;
            ToValue = toValue;
            // --- Attempt to find an optimized path for common types ---

            // Transform
            if (target is Transform t)
            {
                switch (propertyName)
                {
                    case "position": _vector3Setter = (v) => t.position = v; return;
                    case "localPosition": _vector3Setter = (v) => t.localPosition = v; return;
                    case "eulerAngles": _vector3Setter = (v) => t.eulerAngles = v; return;
                    case "localEulerAngles": _vector3Setter = (v) => t.localEulerAngles = v; return;
                    case "localScale": _vector3Setter = (v) => t.localScale = v; return;
                    case "rotation": _quaternionSetter = (q) => t.rotation = q; return;
                    case "localRotation": _quaternionSetter = (q) => t.localRotation = q; return;
                }
            }
            else if (target is Text text) // Exemplo de otimização para Text
            {
                if (propertyName == "text") { _stringSetter = (s) => text.text = s; return; }
                if (propertyName == "color") { _colorSetter = (c) => text.color = c; return; }
            }
            // RectTransform (for UI)
            else if (target is RectTransform rt)
            {
                switch (propertyName)
                {
                    case "anchoredPosition": _vector2Setter = (v) => rt.anchoredPosition = v; return;
                    case "sizeDelta": _vector2Setter = (v) => rt.sizeDelta = v; return;
                    // Inherits Transform properties like localScale
                    case "localScale": _vector3Setter = (v) => rt.localScale = v; return;
                }
            }
            // CanvasGroup (for UI fading)
            else if (target is CanvasGroup cg)
            {
                if (propertyName == "alpha") { _floatSetter = (f) => cg.alpha = f; return; }
            }
            // UI Graphic (Image, RawImage, Text)
            else if (target is Graphic g)
            {
                if (propertyName == "color") { _colorSetter = (c) => g.color = c; return; }
            }
            // SpriteRenderer
            else if (target is SpriteRenderer sr)
            {
                if (propertyName == "color") { _colorSetter = (c) => sr.color = c; return; }
            }
            // Material
            else if (target is Material m)
            {
                // Note: This modifies the shared material asset. A better practice for individual
                // objects is to use renderer.material to create an instance first.
                if (propertyName == "color") { _colorSetter = (c) => m.color = c; return; }
            }
            // Light
            else if (target is Light l)
            {
                switch (propertyName)
                {
                    case "color": _colorSetter = (c) => l.color = c; return;
                    case "intensity": _floatSetter = (f) => l.intensity = f; return;
                    case "range": _floatSetter = (f) => l.range = f; return;
                }
            }
            // Camera
            else if (target is Camera cam)
            {
                switch (propertyName)
                {
                    case "fieldOfView": _floatSetter = (f) => cam.fieldOfView = f; return;
                    case "orthographicSize": _floatSetter = (f) => cam.orthographicSize = f; return;
                    case "backgroundColor": _colorSetter = (c) => cam.backgroundColor = c; return;
                }
            }
            // AudioSource
            else if (target is AudioSource audio)
            {
                switch (propertyName)
                {
                    case "volume": _floatSetter = (f) => audio.volume = f; return;
                    case "pitch": _floatSetter = (f) => audio.pitch = f; return;
                }
            }
            else if (propertyInfo != null)
            {
                CreateTypedSetter(propertyInfo.PropertyType, (val) => propertyInfo.SetValue(target, val));
            }
            else if (fieldInfo != null)
            {
                CreateTypedSetter(fieldInfo.FieldType, (val) => fieldInfo.SetValue(target, val));
            }
            else
            {
                Debug.LogWarning($"AnimaTween: Property or Field '{propertyName}' not found or is not writable on target '{target.GetType().Name}'. The tween may fail.");
            }
        }
        
        /// <summary>
        /// Helper function that takes a member's type and a generic setter action,
        /// and assigns it to the correct typed Action delegate.
        /// </summary>
        private void CreateTypedSetter(Type memberType, Action<object> setter)
        {
            if (memberType == typeof(float)) _floatSetter = (v) => setter(v);
            else if (memberType == typeof(double)) _doubleSetter = (v) => setter(v);
            else if (memberType == typeof(int)) _intSetter = (v) => setter(v);
            else if (memberType == typeof(string)) _stringSetter = (v) => setter(v);
            else if (memberType == typeof(Color)) _colorSetter = (v) => setter(v);
            else if (memberType == typeof(Vector2)) _vector2Setter = (v) => setter(v);
            else if (memberType == typeof(Vector3)) _vector3Setter = (v) => setter(v);
            else if (memberType == typeof(Vector4)) _vector4Setter = (v) => setter(v);
            else if (memberType == typeof(Quaternion)) _quaternionSetter = (v) => setter(v);
            else if (memberType == typeof(Rect)) _rectSetter = (v) => setter(v);
            else if (memberType == typeof(Bounds)) _boundsSetter = (v) => setter(v);
        }

        // --- SetValue methods are now extremely simple and fast ---

        public void SetValue(float value)   { if (!AnimaTweenCoroutines.IsTargetDestroyed(Target)) _floatSetter?.Invoke(value); }
        public void SetValue(double value)  { if (!AnimaTweenCoroutines.IsTargetDestroyed(Target)) _doubleSetter?.Invoke(value); }
        public void SetValue(int value)     { if (!AnimaTweenCoroutines.IsTargetDestroyed(Target)) _intSetter?.Invoke(value); }
        public void SetValue(string value)     { if (!AnimaTweenCoroutines.IsTargetDestroyed(Target)) _stringSetter?.Invoke(value); }
        public void SetValue(Color value)   { if (!AnimaTweenCoroutines.IsTargetDestroyed(Target)) _colorSetter?.Invoke(value); }
        public void SetValue(Vector2 value) { if (!AnimaTweenCoroutines.IsTargetDestroyed(Target)) _vector2Setter?.Invoke(value); }
        public void SetValue(Vector3 value) { if (!AnimaTweenCoroutines.IsTargetDestroyed(Target)) _vector3Setter?.Invoke(value); }
        public void SetValue(Vector4 value) { if (!AnimaTweenCoroutines.IsTargetDestroyed(Target)) _vector4Setter?.Invoke(value); }
        public void SetValue(Quaternion value) { if (!AnimaTweenCoroutines.IsTargetDestroyed(Target)) _quaternionSetter?.Invoke(value); }
        public void SetValue(Rect value)    { if (!AnimaTweenCoroutines.IsTargetDestroyed(Target)) _rectSetter?.Invoke(value); }
        public void SetValue(Bounds value)  { if (!AnimaTweenCoroutines.IsTargetDestroyed(Target)) _boundsSetter?.Invoke(value); }
        /// <summary>
        /// The new SetValue prioritizes the optimized path.
        /// </summary>
        public void SetValue(object value)
        {
            if (AnimaTweenCoroutines.IsTargetDestroyed(Target)) return;

            // --- Attempt to use the optimized setters first ---
            if (_floatSetter != null && value is float f) { _floatSetter(f); return; }
            if (_intSetter != null && value is int i) { _intSetter(i); return; }
            if (_stringSetter != null && value is string s) { _stringSetter(s); return; }
            if (_colorSetter != null && value is Color c) { _colorSetter(c); return; }
            if (_vector2Setter != null && value is Vector2 v2) { _vector2Setter(v2); return; }
            if (_vector3Setter != null && value is Vector3 v3) { _vector3Setter(v3); return; }
            if (_vector4Setter != null && value is Vector4 v4) { _vector4Setter(v4); return; }
            if (_quaternionSetter != null && value is Quaternion q) { _quaternionSetter(q); return; }
            if (_rectSetter != null && value is Rect r) { _rectSetter(r); return; }
            if (_boundsSetter != null && value is Bounds b) { _boundsSetter(b); return; }
            // --- If no specific setter exists, use the reflection fallback ---
            if (FieldInfo != null) FieldInfo.SetValue(Target, value);
            else PropertyInfo?.SetValue(Target, value);
        }
    }
}
