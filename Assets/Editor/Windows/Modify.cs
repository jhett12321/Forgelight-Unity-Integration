using UnityEditor;

public class Modify : EditorWindow
{
    [MenuItem("Forgelight/Windows/Modify")]
    public static void Init()
    {
        EditorWindow.GetWindow(typeof(Modify));
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
