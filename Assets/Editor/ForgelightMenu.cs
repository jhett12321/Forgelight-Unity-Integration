using Forgelight;
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
