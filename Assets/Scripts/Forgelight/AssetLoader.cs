using System;
using System.IO;
using UnityEditor;

public class AssetLoader
{
    public static string OpenAssetFolder()
    {
        string path;

        do
        {
            path = EditorUtility.OpenFolderPanel(
            "Select folder containing Forgelight pack files.",
            "",
            "");

            if (path.Length == 0)
            {
                return null;
            }

        } while (!CheckGivenAssetDirectory(path));

        return path;
    }

    private static bool CheckGivenAssetDirectory(string path)
    {
        String[] files = Directory.GetFiles(path);

        foreach (string fileName in files)
        {
            if (fileName.EndsWith(".pack"))
            {
                return true;
            }
        }

        EditorUtility.DisplayDialog("Invalid Asset Directory", "The folder provided does not contain any valid .pack files. Please make sure you have selected the correct assets folder.", "OK");

        return false;
    }
}
