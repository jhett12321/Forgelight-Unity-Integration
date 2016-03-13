using Forgelight;
using Forgelight.Formats.Zone;
using Forgelight.Pack;
using Forgelight.Utils;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ForgelightExtension))]
public class ForgelightMenu : Editor
{
    [MenuItem("Forgelight/Load Forgelight Game Data...")]
    public static void SelectGameDirectory()
    {
        ForgelightExtension.Instance.ForgelightGameFactory.OpenForgelightGameFolder();
    }

    [MenuItem("Forgelight/Create/New Object")]
    public static void CreateZoneObject()
    {
        //GameObject newObject = ForgelightExtension.Instance.ZoneObjectFactory.CreateForgelightObject("default", ForgelightExtension.Instance.LastCameraPos, Quaternion.identity);
        //Selection.activeGameObject = newObject;
    }

    [MenuItem("Forgelight/Delete/Terrain")]
    public static void DeleteTerrain()
    {
        GameObject terrain = GameObject.FindWithTag("Terrain");

        if (terrain != null)
        {
            DestroyImmediate(terrain);
        }
    }

    [MenuItem("Forgelight/Delete/All Zone Objects")]
    public static void DeleteZoneObjects()
    {
        if (DialogUtils.DisplayCancelableDialog("Delete Zone Objects", "This will destroy all objects current scene, and you will lose any unsaved changes. This cannot be undone. Are you sure you wish to continue?"))
        {
            ForgelightExtension.Instance.ZoneObjectFactory.DestroyAllObjects();
        }
    }

    [MenuItem("Forgelight/Export/Export Current Scene to Zone File...")]
    public static void SaveZoneFile()
    {
        ForgelightExtension.Instance.ZoneExporter.ExportZoneFile();
    }

    [MenuItem("Forgelight/Export/Create Pack File...")]
    public static void CreatePackFile()
    {
        PackCreator.CreatePackFromDirectory();
    }
}
