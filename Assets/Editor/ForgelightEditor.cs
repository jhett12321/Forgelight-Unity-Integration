using Forgelight;
using Forgelight.Pack;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ForgelightExtension))]
public class ForgelightEditor : Editor
{
    [MenuItem("Forgelight/Load Forgelight Game Data...")]
    public static void SelectGameDirectory()
    {
        ForgelightExtension.Instance.AssetLoader.OpenAssetFolder();
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
        //TODO Implement alias'
        //GameObject newObject = ForgelightExtension.Instance.ZoneObjectFactory.CreateForgelightObject("default", ForgelightExtension.Instance.lastCameraPos, Quaternion.identity);
        //Selection.activeGameObject = newObject;
    }

    [MenuItem("Forgelight/Load Zone File...")]
    public static void LoadZoneFile()
    {
        ForgelightExtension.Instance.ZoneLoader.LoadZoneFile();
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

    #region Forgelight/Load Terrain Data
    [MenuItem("Forgelight/Load Terrain Data/Indar")]
    public static void LoadIndarTerrainData()
    {
        ForgelightExtension.Instance.TerrainLoader.LoadTerrain("Indar");
    }

    [MenuItem("Forgelight/Load Terrain Data/Hossin")]
    public static void LoadHossinTerrainData()
    {
        ForgelightExtension.Instance.TerrainLoader.LoadTerrain("Hossin");
    }

    [MenuItem("Forgelight/Load Terrain Data/Amerish")]
    public static void LoadAmerishTerrainData()
    {
        ForgelightExtension.Instance.TerrainLoader.LoadTerrain("Amerish");
    }

    [MenuItem("Forgelight/Load Terrain Data/Esamir")]
    public static void LoadEsamirTerrainData()
    {
        ForgelightExtension.Instance.TerrainLoader.LoadTerrain("Esamir");
    }

    [MenuItem("Forgelight/Load Terrain Data/Tutorial")]
    public static void LoadTutorialTerrainData()
    {
        ForgelightExtension.Instance.TerrainLoader.LoadTerrain("Tutorial");
    }

    [MenuItem("Forgelight/Load Terrain Data/VR")]
    public static void LoadVRTerrainData()
    {
        ForgelightExtension.Instance.TerrainLoader.LoadTerrain("VR");
    }

    [MenuItem("Forgelight/Load Terrain Data/Koltyr (quickload)")]
    public static void LoadQuickLoadTerrainData()
    {
        ForgelightExtension.Instance.TerrainLoader.LoadTerrain("quickload");
    }

    [MenuItem("Forgelight/Load Terrain Data/Nexus")]
    public static void LoadNexusTerrainData()
    {
        ForgelightExtension.Instance.TerrainLoader.LoadTerrain("nexus");
    }
    #endregion
}
