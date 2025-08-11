using UnityEngine;
using System;
using System.Collections.Generic;

namespace AnimaTween
{
    /// <summary>
    /// A self-managed component that runs all tweens for a specific GameObject.
    /// It is added automatically and destroys itself when it has no more tweens to run.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("")] // Hides this from the "Add Component" menu
    internal class AnimaTweenInstance : MonoBehaviour
    {
        // This runner only manages tweens for special, "unhosted" targets.
        internal readonly Dictionary<System.Tuple<object, string>, TweenInfo> unhostedTweens = 
            new Dictionary<System.Tuple<object, string>, TweenInfo>();
        
        // Each instance now manages its own dictionary of tweens.
        internal readonly Dictionary<string, TweenInfo> activeTweens = new Dictionary<string, TweenInfo>();

        // A flag to check if we should destroy this component at the end of the frame.
        private bool _isDirty = false;

        public void MarkAsDirty()
        {
            _isDirty = true;
        }

        private void LateUpdate()
        {
            // If the component was marked for cleanup and has no more active tweens, destroy it.
            if (_isDirty && activeTweens.Count == 0)
            {
                Destroy(this);
            }
            _isDirty = false;
        }
        
        private void OnDestroy()
        {
            // The scene is being destroyed, so we must clean up any tweens
            // that were running on objects from this scene.
            AnimaTweenExtensions.CleanUpGlobalRunner();
        }
    }
}