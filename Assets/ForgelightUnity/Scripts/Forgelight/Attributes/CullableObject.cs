namespace ForgelightUnity.Forgelight.Attributes
{
    using UnityEngine;

    /// <summary>
    /// A component added to terrain, and zone objects to indicate they are cullable.
    /// </summary>
    [SelectionBase]
    [ExecuteInEditMode]
    public abstract class CullableObject : MonoBehaviour
    {
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}
