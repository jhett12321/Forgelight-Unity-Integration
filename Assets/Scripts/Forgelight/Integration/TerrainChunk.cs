using Forgelight.Attributes;
using UnityEngine;

namespace Forgelight.Integration
{
    public class TerrainChunk : CullableObject
    {
        public override void Show()
        {
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = true;
            }
        }

        public override void Hide()
        {
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }
        }
    }
}
