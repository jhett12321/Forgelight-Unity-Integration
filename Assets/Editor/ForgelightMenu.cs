using System.Reflection;
using Forgelight;
using Forgelight.Attributes;
using Forgelight.Editor;
using Forgelight.Editor.Helper;
using Forgelight.Editor.Windows;
using Forgelight.Pack;
using Forgelight.Utils;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ForgelightExtension))]
public class ForgelightMenu : Editor
{
    #region Windows
    [MenuItem("Forgelight/Windows/Create %&c", false, 10000)]
    public static void CreateWindow()
    {
        Create.Init();
    }

    [MenuItem("Forgelight/Windows/Games %&g", false, 10001)]
    public static void GamesWindow()
    {
        ForgelightGameSelect.Init();
    }

    [MenuItem("Forgelight/Windows/Zones %&z", false, 10002)]
    public static void ZonesWindow()
    {
        ZoneLoader.Init();
    }

    [MenuItem("Forgelight/Windows/Areas %&a", false, 10003)]
    public static void AreasWindow()
    {
        AreaLoader.Init();
    }
    #endregion

    #region Helpers

    [MenuItem("Forgelight/Helpers/Parent Selected Entities")]
    public static void ParentEntities()
    {
        EntityParenter.ParentSelection();
    }
    #endregion

    #region Draw
    [MenuItem("Forgelight/Draw/Cull World from Current Position %g", false, 10004)]
    public static void CullWorld()
    {
        ResetCulling(); //Make sure we have not already culled the world.

        Vector3 cameraPos = ForgelightExtension.Instance.LastCameraPos;
        cameraPos.y = 0; //We ignore vertical position.

        foreach (CullableObject cullableObject in Resources.FindObjectsOfTypeAll<CullableObject>())
        {
            if (cullableObject.hideFlags == HideFlags.NotEditable || cullableObject.hideFlags == HideFlags.HideAndDontSave || EditorUtility.IsPersistent(cullableObject))
            {
                continue;
            }

            Vector3 objPos = cullableObject.transform.position;
            objPos.y = 0;

            if (Vector3.Distance(objPos, cameraPos) > ForgelightPreferences.CullingDistance)
            {
                cullableObject.Hide();
            }
        }
    }

    [MenuItem("Forgelight/Draw/Draw All %#g", false, 10050)]
    public static void ResetCulling()
    {
        foreach (CullableObject cullableObject in Resources.FindObjectsOfTypeAll<CullableObject>())
        {
            if (cullableObject.hideFlags == HideFlags.NotEditable || cullableObject.hideFlags == HideFlags.HideAndDontSave || EditorUtility.IsPersistent(cullableObject))
            {
                continue;
            }

            cullableObject.Show();
        }
    }

    //[MenuItem("Forgelight/Draw/Hide Terrain", false, 10051)]
    //public static void HideTerrain()
    //{

    //}

    //[MenuItem("Forgelight/Draw/Hide Objects", false, 10052)]
    //public static void HideObjects()
    //{

    //}

    [MenuItem("Forgelight/Draw/Cull Settings", false, 10100)]
    public static void CullSettings()
    {
        ForgelightSettings();
    }
    #endregion

    #region Export
    [MenuItem("Forgelight/Export/Export Current Scene to Zone File...", false, 10200)]
    public static void SaveZoneFile()
    {
        ForgelightExtension.Instance.ZoneExporter.ExportZoneFile();
    }

    [MenuItem("Forgelight/Export/Create Pack File...", false, 10201)]
    public static void CreatePackFile()
    {
        PackCreator.CreatePackFromDirectory();
    }
    #endregion

    #region Load
    [MenuItem("Forgelight/Load/Load Forgelight Game Data...", false, 10202)]
    public static void SelectGameDirectory()
    {
        ForgelightExtension.Instance.ForgelightGameFactory.OpenForgelightGameFolder();
    }

    [MenuItem("Forgelight/Load/Load Zone File...", false, 10203)]
    public static void LoadZoneFile()
    {
        if (ForgelightExtension.Instance.ForgelightGameFactory.ActiveForgelightGame == null)
        {
            DialogUtils.DisplayDialog("No Active Game", "There is currently no active forgelight game. Please load the correct forgelight game and try again.");
            return;
        }

        string path = DialogUtils.OpenFile("Load Zone File", "", "zone");

        if (path != null)
        {
            if (!ForgelightExtension.Instance.ForgelightGameFactory.ActiveForgelightGame.LoadZoneFromFile(path))
            {
                DialogUtils.DisplayDialog("Zone Import Failed", "An error occurred while loading the zone file. Please check the console window for more info.");
            }
        }
    }
    #endregion

    [MenuItem("Forgelight/Destroy Active Zone", false, 10400)]
    public static void DeleteZoneObjects()
    {
        if (DialogUtils.DisplayCancelableDialog("Destroy Zone", "This will destroy all objects and terrain in the current scene, and you will lose any unsaved changes. This cannot be undone. Are you sure you wish to continue?"))
        {
            ForgelightExtension.Instance.ZoneManager.DestroyActiveZone();
        }
    }

    [MenuItem("Forgelight/Settings", false, 10500)]
    public static void ForgelightSettings()
    {
        var asm = Assembly.GetAssembly(typeof(EditorWindow));
        var T = asm.GetType("UnityEditor.PreferencesWindow");
        var M = T.GetMethod("ShowPreferencesWindow", BindingFlags.NonPublic | BindingFlags.Static);
        M.Invoke(null, null);
    }
}
