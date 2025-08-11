using System;
using System.Reflection;
using UnityEngine;

namespace AnimaTween
{
    // --- HELPER CLASS TO STORE TWEEN DATA ---
    internal class TweenInfo
    {
        public Coroutine Coroutine { get; set; }
        public Action OnComplete { get; set; }
        public object Target { get; set; }
        
        // For property tweens
        public FieldInfo FieldInfo { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
        public object StartValue { get; set; }
        public object ToValue { get; set; }

        public void SetValue(object value)
        {
            if (AnimaTweenCoroutines.IsTargetDestroyed(Target)) return;
            if (FieldInfo != null) FieldInfo.SetValue(Target, value);
            else PropertyInfo?.SetValue(Target, value);
        }
    }
}