using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Forgelight))]
public class ForgelightEditor : Editor
{
    public static string forgeLightFilePath = null;

    [MenuItem("Forgelight/Select Asset Directory")]
    public static void SelectAssetDirectory()
    {
        string path = AssetLoader.OpenAssetFolder();

        if (path != null)
        {
            forgeLightFilePath = path;
        }
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
        GameObject newObject = Forgelight.Instance.ZoneObjectFactory.CreateForgelightObject("default", Forgelight.Instance.lastCameraPos, Quaternion.identity);
        Selection.activeGameObject = newObject;
    }

    [MenuItem("Forgelight/Load Zone File...")]
    public static void LoadZoneFile()
    {
        Forgelight.Instance.ZoneLoader.LoadZoneFile();
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
        Forgelight.Instance.ZoneExporter.ExportZoneFile();
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
        Forgelight.Instance.TerrainLoader.LoadTerrain("Indar");
    }

    [MenuItem("Forgelight/Load Terrain Data/Hossin")]
    public static void LoadHossinTerrainData()
    {
        Forgelight.Instance.TerrainLoader.LoadTerrain("Hossin");
    }

    [MenuItem("Forgelight/Load Terrain Data/Amerish")]
    public static void LoadAmerishTerrainData()
    {
        Forgelight.Instance.TerrainLoader.LoadTerrain("Amerish");
    }

    [MenuItem("Forgelight/Load Terrain Data/Esamir")]
    public static void LoadEsamirTerrainData()
    {
        Forgelight.Instance.TerrainLoader.LoadTerrain("Esamir");
    }

    [MenuItem("Forgelight/Load Terrain Data/Tutorial")]
    public static void LoadTutorialTerrainData()
    {
        Forgelight.Instance.TerrainLoader.LoadTerrain("Tutorial");
    }

    [MenuItem("Forgelight/Load Terrain Data/VR")]
    public static void LoadVRTerrainData()
    {
        Forgelight.Instance.TerrainLoader.LoadTerrain("VR");
    }

    [MenuItem("Forgelight/Load Terrain Data/Koltyr (quickload)")]
    public static void LoadQuickLoadTerrainData()
    {
        Forgelight.Instance.TerrainLoader.LoadTerrain("quickload");
    }

    [MenuItem("Forgelight/Load Terrain Data/Nexus")]
    public static void LoadNexusTerrainData()
    {
        Forgelight.Instance.TerrainLoader.LoadTerrain("nexus");
    }
    #endregion
}
