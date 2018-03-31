namespace ForgelightUnity.Editor.Forgelight.Assets.Zone
{
    using System.IO;
    using UnityEditor;
    using Utils;

    public class ZoneExporter
    {
        public void ExportZoneFile()
        {
            if (ForgelightExtension.Instance.ZoneManager.LoadedZone != null)
            {
                string path = EditorUtility.SaveFilePanel(
                    "Save zone file",
                    ForgelightExtension.Instance.ForgelightGameFactory.ActiveForgelightGame.GameInfo.PackDirectory,
                    Path.GetFileNameWithoutExtension(ForgelightExtension.Instance.ZoneManager.LoadedZone.Name),
                    "zone");

                if (path == null)
                {
                    return;
                }

                SaveZone(path);
            }
            else
            {
                DialogUtils.DisplayDialog("Cannot save zone", "An existing zone file needs to be loaded first. Please import a zone file, then try again");
            }
        }

        private void SaveZone(string path)
        {
            ForgelightExtension.Instance.ZoneManager.ApplySceneChangesToZone();

            //Write zone to file.
            using (FileStream zoneFile = new FileStream(path, FileMode.Create))
            {
                Zone.SerializeZoneToStream(ForgelightExtension.Instance.ZoneManager.LoadedZone, zoneFile);
            }
        }
    }
}
