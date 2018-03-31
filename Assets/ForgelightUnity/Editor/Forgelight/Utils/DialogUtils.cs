namespace ForgelightUnity.Editor.Forgelight.Utils
{
    using System.IO;
    using UnityEditor;

    public class DialogUtils
    {
        public static bool DirectoryIsEmpty(string path) => Directory.GetFiles(path).Length > 0;

        public static bool DisplayCancelableDialog(string title, string message)
        {
            return EditorUtility.DisplayDialog(title, message, "OK", "Cancel");
        }

        public static bool DisplayDialog(string title, string message)
        {
            return EditorUtility.DisplayDialog(title, message, "OK");
        }
    }
}
