using Forgelight;
using Forgelight.Pack;
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

    #region Validate editor
    //[MenuItem("Forgelight/Load Zone File...", true)]
    //[MenuItem("Forgelight/Load Terrain Data", true)]
    //[MenuItem("Forgelight/Export Current Scene to Zone File...", true)]
    //[MenuItem("Forgelight/Create Pack File", true)]
    //[MenuItem("Forgelight/Load Terrain Data/Indar", true)]
    //[MenuItem("Forgelight/Load Terrain Data/Hossin", true)]
    //[MenuItem("Forgelight/Load Terrain Data/Amerish", true)]
    //[MenuItem("Forgelight/Load Terrain Data/Esamir", true)]
    //[MenuItem("Forgelight/Load Terrain Data/Tutorial", true)]
    //[MenuItem("Forgelight/Load Terrain Data/VR", true)]
    //[MenuItem("Forgelight/Load Terrain Data/Koltyr (quickload)", true)]
    //[MenuItem("Forgelight/Load Terrain Data/Nexus", true)]
    //public static bool CanUseEditor()
    //{
    //    //if (forgeLightFilePath != null)
    //    //{
    //    //    return true;
    //    //}

    //    //return false;
    //    return true;
    //}
    #endregion

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

    [MenuItem("Forgelight/Delete/Zone Objects")]
    public static void DeleteZoneObjects()
    {
        GameObject zoneObjectsParent = GameObject.FindWithTag("ZoneObjects");

        if (zoneObjectsParent != null)
        {
            DestroyImmediate(zoneObjectsParent);
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
