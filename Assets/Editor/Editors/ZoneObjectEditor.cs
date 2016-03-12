
using Forgelight.Editor.AttributeDrawers;
using Forgelight.Formats.Zone;
using UnityEditor;

namespace Forgelight.Editor.Editors
{
    [CustomEditor(typeof(ZoneObject))]
    public class ZoneObjectEditor : UnityEditor.Editor
    {
        ZoneObject instance;
        PropertyField[] fields;

        public void OnEnable()
        {
            instance = target as ZoneObject;
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
