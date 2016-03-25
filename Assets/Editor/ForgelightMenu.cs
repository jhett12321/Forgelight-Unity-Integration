using System;
using System.IO;
using Forgelight;
using Forgelight.Formats.Zone;
using Forgelight.Pack;
using Forgelight.Utils;
using UnityEditor;

[CustomEditor(typeof(ForgelightExtension))]
public class ForgelightMenu : Editor
{
    [MenuItem("Forgelight/Load Forgelight Game Data...")]
    public static void SelectGameDirectory()
    {
        ForgelightExtension.Instance.ForgelightGameFactory.OpenForgelightGameFolder();
    }

    [MenuItem("Forgelight/Load Zone File...")]
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

    [MenuItem("Forgelight/Destroy Active Zone")]
    public static void DeleteZoneObjects()
    {
        if (DialogUtils.DisplayCancelableDialog("Destroy Zone", "This will destroy all objects and terrain in the current scene, and you will lose any unsaved changes. This cannot be undone. Are you sure you wish to continue?"))
        {
            ForgelightExtension.Instance.ChunkLoader.DestroyTerrain();
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
