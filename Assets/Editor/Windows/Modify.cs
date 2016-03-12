using UnityEditor;

namespace Forgelight.Editor.Windows
{
    public class Modify : EditorWindow
    {
        [MenuItem("Forgelight/Windows/Modify")]
        public static void Init()
        {
            GetWindow(typeof (Modify), false, "Modify");
        }

        private void OnFocus()
        {
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
        }

        private void OnDestroy()
        {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
        }

        private void OnGUI()
        {

        }

        public void OnSceneGUI(SceneView sceneView)
        {

        }
    }
}