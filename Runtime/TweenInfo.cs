using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnimaTween
{
    internal class TweenInfo
    {
        // --- Propriedades Gerais ---
        public Coroutine Coroutine { get; set; }
        public Action OnComplete { get; set; }
        // --- Sistema de Caminhos (Waypoints) ---
        private object[] _waypoints;
        private object _currentStartValue;
        private object _currentEndValue;
        public object Target { get; set; }
        public object StartValue { get; set; }
        public object ToValue { get; set; }
        public FieldInfo FieldInfo { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        // --- Delegate de Estratégia ---
        public readonly Action<float> _progressHandler;

        // --- Optimized Path (Direct Access Delegates) ---
        // Delegates that store the action of setting the value directly.
        private Action<float> _floatSetter;
        private Action<int> _intSetter;
        private Action<double> _doubleSetter;
        private Action<Color> _colorSetter;
        private Action<Vector2> _vector2Setter;
        private Action<Vector3> _vector3Setter;
        private Action<Vector4> _vector4Setter;
        private Action<Vector2Int> _vector2IntSetter;
        private Action<Vector3Int> _vector3IntSetter;
        private Action<Quaternion> _quaternionSetter;
        private Action<Rect> _rectSetter;
        private Action<Bounds> _boundsSetter;
        private Action<string> _stringSetter;
        private Action<Gradient> _gradientSetter;



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
            PropertyInfo propertyInfo, FieldInfo fieldInfo, bool materialProp=false, object[] midValues = null)
        {
            Target = target;
            OnComplete = onComplete;
            StartValue = startValue;
            ToValue = toValue;
            if (midValues != null && midValues.Length > 0)
            {
                _waypoints = new object[midValues.Length + 2];
                _waypoints[0] = startValue;
                Array.Copy(midValues, 0, _waypoints, 1, midValues.Length);
                _waypoints[_waypoints.Length - 1] = toValue;
                _progressHandler = HandleWaypointProgress;
            }
            else
            {
                _currentStartValue = StartValue;
                _currentEndValue = toValue;
                _progressHandler = HandleSimpleProgress;
            }
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
            else if (target is TMP_Text tmpText) 
            {
                switch (propertyName)
                {
                    case "text": _stringSetter = (s) => tmpText.text = s; return;
                    case "color": _colorSetter = (c) => tmpText.color = c; return;
                    case "fontSize": _floatSetter = (f) => tmpText.fontSize = f; return;
                    case "characterSpacing": _floatSetter = (f) => tmpText.characterSpacing = f; return;
                    case "wordSpacing": _floatSetter = (f) => tmpText.wordSpacing = f; return;
                    case "lineSpacing": _floatSetter = (f) => tmpText.lineSpacing = f; return;
                    case "margin": _vector4Setter = (v) => tmpText.margin = v; return;
                    case "maxVisibleCharacters": _intSetter = (i) => tmpText.maxVisibleCharacters = i; return;
                }
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
            // **NOVO:** Suporte para Rigidbody 3D
            else if (target is Rigidbody rb3d)
            {
                switch (propertyName)
                {
                    case "position": _vector3Setter = (v) => rb3d.position = v; return;
                    case "rotation": _quaternionSetter = (q) => rb3d.rotation = q; return;
                    case "velocity": _vector3Setter = (v) => rb3d.linearVelocity = v; return;
                    case "angularVelocity": _vector3Setter = (v) => rb3d.angularVelocity = v; return;
                }
            }
            // **NOVO:** Suporte para Rigidbody 2D
            else if (target is Rigidbody2D rb2d)
            {
                switch (propertyName)
                {
                    case "position": _vector2Setter = (v) => rb2d.position = v; return;
                    case "rotation": _floatSetter = (f) => rb2d.rotation = f; return;
                    case "velocity": _vector2Setter = (v) => rb2d.linearVelocity = v; return;
                    case "angularVelocity": _floatSetter = (f) => rb2d.angularVelocity = f; return;
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
                // Se a propriedade for "color", usamos o atalho otimizado.
                if (propertyName == "color") 
                {
                    _colorSetter = (c) => m.color = c;
                    return;
                }
                if (materialProp)
                {
                    switch (ToValue)
                    {
                        case float f:
                            _floatSetter = (val) => m.SetFloat(propertyName, val);
                            return;
                        case int i:
                            _intSetter = (val) => m.SetInt(propertyName, val);
                            return;
                        case Color c:
                            _colorSetter = (val) => m.SetColor(propertyName, val);
                            return;
                        case Vector4 v4:
                            _vector4Setter = (val) => m.SetVector(propertyName, val);
                            return;
                        case Vector3 v3:
                            // Shaders usam Vector4, então promovemos o Vector3.
                            _vector3Setter = (val) => m.SetVector(propertyName, val);
                            return;
                        case Vector2 v2:
                            // Shaders usam Vector4, então promovemos o Vector2.
                            _vector2Setter = (val) => m.SetVector(propertyName, val);
                            return;
                        case Vector3Int v3i:
                            // Cast explícito de Vector3Int para Vector3
                            _vector3IntSetter = (val) => m.SetVector(propertyName, (Vector3)val);
                            return;
                        case Vector2Int v2i:
                            // Cast explícito de Vector2Int para Vector2
                            _vector2IntSetter = (val) => m.SetVector(propertyName, (Vector2)val);
                            return;
                        default:
                            Debug.Log($"Cant find type {ToValue}");
                            break;
                    }
                }
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
            // LineRenderer
            else if (target is LineRenderer lr)
            {
                if (propertyName == "colorGradient") { _gradientSetter = (g) => lr.colorGradient = g; return; }
            }
            // TrailRenderer
            else if (target is TrailRenderer tr)
            {
                if (propertyName == "colorGradient") { _gradientSetter = (g) => tr.colorGradient = g; return; }
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
            switch (memberType.Name)
            {
                case "Single": // Nome do tipo para float
                    _floatSetter = (v) => setter(v);
                    break;
                case "Double":
                    _doubleSetter = (v) => setter(v);
                    break;
                case "Int32": // Nome do tipo para int
                    _intSetter = (v) => setter(v);
                    break;
                case "String":
                    _stringSetter = (v) => setter(v);
                    break;
                case "Color":
                    _colorSetter = (v) => setter(v);
                    break;
                case "Vector2":
                    _vector2Setter = (v) => setter(v);
                    break;
                case "Vector3":
                    _vector3Setter = (v) => setter(v);
                    break;
                case "Vector4":
                    _vector4Setter = (v) => setter(v);
                    break;
                case "Quaternion":
                    _quaternionSetter = (v) => setter(v);
                    break;
                case "Rect":
                    _rectSetter = (v) => setter(v);
                    break;
                case "Bounds":
                    _boundsSetter = (v) => setter(v);
                    break;
                case "Gradient":
                    _gradientSetter = (v) => setter(v);
                    break;
                // Tipos extras para maior compatibilidade
                case "Vector2Int":
                    _vector2IntSetter = (v) => setter(v);
                    break;
                case "Vector3Int":
                    _vector3IntSetter = (v) => setter(v);
                    break;
            }
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
        public void SetValue(Vector2Int value) { if (!AnimaTweenCoroutines.IsTargetDestroyed(Target)) _vector2IntSetter?.Invoke(value); }
        public void SetValue(Vector3Int value) { if (!AnimaTweenCoroutines.IsTargetDestroyed(Target)) _vector3IntSetter?.Invoke(value); }
        public void SetValue(Quaternion value) { if (!AnimaTweenCoroutines.IsTargetDestroyed(Target)) _quaternionSetter?.Invoke(value); }
        public void SetValue(Rect value)    { if (!AnimaTweenCoroutines.IsTargetDestroyed(Target)) _rectSetter?.Invoke(value); }
        public void SetValue(Bounds value)  { if (!AnimaTweenCoroutines.IsTargetDestroyed(Target)) _boundsSetter?.Invoke(value); }
        public void SetValue(Gradient value) { if (!AnimaTweenCoroutines.IsTargetDestroyed(Target)) _gradientSetter?.Invoke(value); }
        /// <summary>
        /// The new SetValue prioritizes the optimized path.
        /// </summary>
        public void SetValue(object value)
        {
            if (AnimaTweenCoroutines.IsTargetDestroyed(Target)) return;

            // --- Attempt to use the optimized setters first ---
            if (_floatSetter != null && value is float f) { _floatSetter(f); return; }
            if (_intSetter != null && value is int i) { _intSetter(i); return; }
            if (_intSetter != null && value is double d) { _doubleSetter(d); return; }
            if (_stringSetter != null && value is string s) { _stringSetter(s); return; }
            if (_colorSetter != null && value is Color c) { _colorSetter(c); return; }
            if (_gradientSetter != null && value is Gradient g) { _gradientSetter(g); return; }
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
        
        public void SetProgress(float easedProgress)
        {
            _progressHandler(easedProgress);
        }
        
        private void HandleWaypointProgress(float easedProgress)
        {
            int segmentCount = _waypoints.Length - 1;
            if (segmentCount <= 0) return; // Segurança
            
            float progressPerSegment = 1.0f / segmentCount;

            int currentSegmentIndex = Mathf.FloorToInt(easedProgress / progressPerSegment);
            currentSegmentIndex = Mathf.Clamp(currentSegmentIndex, 0, segmentCount - 1);

            _currentStartValue = _waypoints[currentSegmentIndex];
            _currentEndValue = _waypoints[currentSegmentIndex + 1];

            // Calcula o progresso DENTRO do segmento atual.
            float progressInSegment = (easedProgress - (currentSegmentIndex * progressPerSegment)) / progressPerSegment;

            // Reutiliza a lógica do tween 
            HandleSimpleProgress(progressInSegment);
        }

        private void HandleSimpleProgress(float getEasedProgress)
        {
            Type targetType = StartValue.GetType();
            if (targetType == typeof(float) || targetType == typeof(int) || targetType == typeof(double))
            {
                // Usa double para todos os cálculos para manter a máxima precisão.
                double startNum = Convert.ToDouble(_currentStartValue);
                double endNum = Convert.ToDouble(_currentEndValue);
    
                
                // Lerp manual para double
                double val = startNum + (endNum - startNum) * getEasedProgress;

                // Converte de volta para o tipo original antes de definir o valor.
                if (targetType == typeof(float))
                {
                    _floatSetter((float)val);
                }
                else if (targetType == typeof(int))
                {
                    _intSetter((int)Math.Round(val));
                }
                else // double
                {
                    _doubleSetter(val);
                }
            }
            else if (targetType == typeof(Rect))
            {
                _rectSetter(LerpRect((Rect)_currentStartValue, (Rect)_currentEndValue, getEasedProgress));
            }
            else if (targetType == typeof(Bounds))
            {
                _boundsSetter(LerpBounds((Bounds)_currentStartValue, (Bounds)_currentEndValue, getEasedProgress));
            }
            else if (targetType == typeof(Vector3))
            {
                _vector3Setter(Vector3.Lerp((Vector3)_currentStartValue, (Vector3)_currentEndValue, getEasedProgress));
            }
            else if (targetType == typeof(Vector2))
            {
                _vector3Setter(Vector2.Lerp((Vector2)_currentStartValue, (Vector2)_currentEndValue, getEasedProgress));
            }
            else if (targetType == typeof(Vector2Int))
            {
                _vector2IntSetter(Vector2Int.RoundToInt(Vector2.Lerp((Vector2Int)_currentStartValue, (Vector2Int)_currentEndValue, getEasedProgress)));
            }
            else if (targetType == typeof(Vector3Int))
            {
                _vector3IntSetter(Vector3Int.RoundToInt(Vector3.Lerp((Vector3Int)_currentStartValue, (Vector3Int)_currentEndValue, getEasedProgress)));
            }
            else if (targetType == typeof(Color))
            {
                Color s = (Color)_currentStartValue;
                Color e = (Color)_currentEndValue;
                _colorSetter(Color.Lerp((Color)_currentStartValue, (Color)_currentEndValue, getEasedProgress));
            }
            else if (targetType == typeof(Quaternion))
            {
                _quaternionSetter(Quaternion.Slerp((Quaternion)_currentStartValue, (Quaternion)_currentEndValue, getEasedProgress));
            }
            else if (targetType == typeof(Gradient))
            {
                Gradient s = (Gradient)_currentStartValue;
                Gradient e = (Gradient)_currentEndValue;
                _gradientSetter(LerpGradient((Gradient)_currentStartValue, (Gradient)_currentEndValue, getEasedProgress));
            }
            else
            {
                Debug.LogError($"AnimaTween: Unsupported property type for tweening: {targetType.Name}");
            }
        }
        
        /// <summary>
        /// Interpola linearmente entre dois Rects. Uma função auxiliar para compatibilidade com versões mais antigas do Unity.
        /// </summary>
        /// <summary>
        /// Interpola linearmente entre dois Rects usando Vector2.Lerp para posição e tamanho.
        /// </summary>
        private Rect LerpRect(Rect a, Rect b, float t)
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
        private Bounds LerpBounds(Bounds a, Bounds b, float t)
        {
            Vector3 center = Vector3.Lerp(a.center, b.center, t);
            Vector3 size = Vector3.Lerp(a.size, b.size, t);
            return new Bounds(center, size);
        }
        
        /// <summary>
        /// Interpola entre dois gradientes amostrando-os em vários pontos.
        /// </summary>
        /// <param name="a">O gradiente inicial.</param>
        /// <param name="b">O gradiente final.</param>
        /// <param name="t">O progresso da interpolação (0 a 1).</param>
        /// <param name="resolution">O número de amostras a retirar. Mais alto é mais preciso, mas mais lento.</param>
        /// <returns>Um novo gradiente que é a mistura dos dois.</returns>
        private Gradient LerpGradient(Gradient a, Gradient b, float t, int resolution = 16)
        {
            var newGradient = new Gradient();

            // Cria os arrays para as novas chaves de cor e alfa.
            var colorKeys = new GradientColorKey[resolution];
            var alphaKeys = new GradientAlphaKey[resolution];

            for (int i = 0; i < resolution; i++)
            {
                // Calcula a posição da amostra atual (de 0 a 1).
                float samplePos = (float)i / (resolution - 1);

                // Obtém a cor de cada gradiente nesta posição.
                Color colorA = a.Evaluate(samplePos);
                Color colorB = b.Evaluate(samplePos);

                // Interpola entre as duas cores amostradas.
                Color finalColor = Color.Lerp(colorA, colorB, t);

                // Cria as novas chaves de cor e alfa.
                colorKeys[i] = new GradientColorKey(finalColor, samplePos);
                alphaKeys[i] = new GradientAlphaKey(finalColor.a, samplePos);
            }

            newGradient.SetKeys(colorKeys, alphaKeys);
            return newGradient;
        }
    }
}
