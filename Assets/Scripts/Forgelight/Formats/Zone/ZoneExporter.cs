using System.IO;
using Forgelight.Utils;

namespace Forgelight.Formats.Zone
{
    public class ZoneExporter
    {
        public void ExportZoneFile()
        {
            if (ForgelightExtension.Instance.ZoneManager.LoadedZone != null)
            {
                var path = DialogUtils.SaveFile(
                    "Save zone file",
                    ForgelightExtension.Instance.ForgelightGameFactory.ActiveForgelightGame.PackDirectory,
                    Path.GetFileNameWithoutExtension(ForgelightExtension.Instance.ZoneManager.LoadedZone.Name),
                    "zone");

                SaveZone(path);
            }
            else
            {
                DialogUtils.DisplayDialog("Cannot save zone",
                    "An existing zone file needs to be loaded first. Please import a zone file, then try again");
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
