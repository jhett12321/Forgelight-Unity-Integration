using Forgelight.Editor.AttributeDrawers;
using UnityEditor;

namespace Forgelight.Editor.Editors
{
    [CustomEditor(typeof(ZoneLight))]
    public class ZoneLightEditor : UnityEditor.Editor
    {
        ZoneLight instance;
        PropertyField[] fields;

        public void OnEnable()
        {
            instance = target as ZoneLight;
            fields = ExposeProperties.GetProperties(instance);
        }

        public override void OnInspectorGUI()
        {
            if (instance == null)
            {
                return;
            }

            DrawDefaultInspector();

            ExposeProperties.Expose(fields);
        }
    }
}
