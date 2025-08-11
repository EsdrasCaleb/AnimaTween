using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AnimaTween
{
    /// <summary>
    /// A persistent, global runner for tweens whose targets are not GameObjects or Components (e.g., Materials).
    /// It self-manages and cleans up dead tweens periodically.
    /// </summary>
    [AddComponentMenu("")]
    internal class AnimaTweenRunner : MonoBehaviour
    {
        // This runner only manages tweens for special, "unhosted" targets.
        internal readonly Dictionary<System.Tuple<object, string>, TweenInfo> unhostedTweens = 
            new Dictionary<System.Tuple<object, string>, TweenInfo>();
        
        public void RemoveUnhostedTween(System.Tuple<object, string> target)
        {
            unhostedTweens.Remove(target);
            CheckCleanup();
        }

        public void AddTweenInfo(System.Tuple<object, string> key, TweenInfo tweenInfo)
        {
            unhostedTweens[key] = tweenInfo;
        }
        
        public void CheckCleanup()
        {
            // Find all keys whose target has been destroyed.
            var destroyedKeys = unhostedTweens.Keys
                .Where(key => AnimaTweenCoroutines.IsTargetDestroyed(key.Item1))
                .ToList();

            // Remove them from the dictionary.
            if (destroyedKeys.Count > 0)
            {
                foreach (var key in destroyedKeys)
                {
                    unhostedTweens.Remove(key);
                }
            }
                
            /*
            if (unhostedTweens.Count == 0)
            {
                Destroy(gameObject);
                // The static reference in AnimaTweenExtensions will be cleared on the next GetRunner call.
            }
            */
        }
    }
}