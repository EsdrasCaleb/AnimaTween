using UnityEngine;

namespace AnimaTween
{
    /// <summary>
    /// This is an internal component that AnimaTween creates automatically.
    /// Its only purpose is to act as a MonoBehaviour host to run coroutines.
    /// Users should not interact with this component directly.
    /// </summary>
    [AddComponentMenu("")] // This hides the component from the "Add Component" menu in the Editor.
    public class AnimaTweenRunner : MonoBehaviour
    {
        // This component is intentionally left empty. Its existence is all we need.
    }
}
