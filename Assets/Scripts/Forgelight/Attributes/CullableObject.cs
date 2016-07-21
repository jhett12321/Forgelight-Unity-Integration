using UnityEngine;

namespace Forgelight.Attributes
{
    /// <summary>
    /// A component added to terrain, and zone objects to indicate they are cullable.
    /// </summary>
    [SelectionBase]
    [ExecuteInEditMode]
    public abstract class CullableObject : MonoBehaviour
    {
        public abstract void Hide();
        public abstract void Show();
    }
}
