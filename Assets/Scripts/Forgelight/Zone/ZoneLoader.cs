using UnityEngine;
using System.Linq;
using Forgelight.Utils;
using Newtonsoft.Json.Linq;
using UnityEditor;
using Object = Forgelight.Formats.Zone.Object;

namespace Forgelight.Zone
{
    public class ZoneLoader
    {
        private bool running = false;
        private int totalObjects = 0;
        private int objectsProcessed = 0;

        public JObject loadedZone { get; private set; }
        public string loadedZonePath { get; private set; }

        public void LoadZoneFile()
        {
            var path = DialogUtils.OpenFile(
                "Select converted zone file...",
                "",
                "json");



            //LoadZone(path);
        }

        public void LoadZone(ForgelightGame forgelightGame, Formats.Zone.Zone zone)
        {
            running = true;

            //Calculate the total objects we need to process.
            foreach (Object zoneObject in zone.Objects)
            {
                totalObjects += zoneObject.Instances.Count();
            }

            ZoneObjectFactory ZoneObjectFactory = ForgelightExtension.Instance.ZoneObjectFactory;

            //Begin processing the file
            foreach (Object zoneObject in zone.Objects)
            {
                if (running && zoneObject.Instances.Count() > 0)
                {
                    string actorDefinition = zoneObject.ActorDefinition;
                    float renderDistance = zoneObject.RenderDistance;

                    foreach (Object.Instance instance in zoneObject.Instances)
                    {
                        ZoneObjectFactory.CreateForgelightObject(forgelightGame, zoneObject.ActorDefinition, ConvertForgelightPosition(instance.Position), ConvertForgelightRotation(instance.Rotation), ConvertForgelightScale(instance.Scale), renderDistance, instance.LODMultiplier, instance.DontCastShadows, instance.ID);
                        objectsProcessed++;
                    }
                }

                else
                {
                    OnLoadComplete(false);
                }

                ProgressBar();
            }

            ZoneObjectFactory.transform.localScale = new Vector3(-1, 1, 1);

            //Unload any unused assets.
            Resources.UnloadUnusedAssets();

            running = false;
            OnLoadComplete(true);
        }

        public void OnLoadComplete(bool completed)
        {
            //Unload any unused assets.
            Resources.UnloadUnusedAssets();

            EditorUtility.ClearProgressBar();

            totalObjects = 0;
            objectsProcessed = 0;
        }

        private void ProgressBar()
        {
            if (running)
            {
                float progress = (float)objectsProcessed / (float)totalObjects;

                if (EditorUtility.DisplayCancelableProgressBar("Importing Forgelight Zone Data",
                    "Importing objects from zone file. This may take a while...", progress))
                {
                    running = false;
                }
            }
        }

        private Vector3 ConvertForgelightPosition(Vector4 fPos)
        {
            return new Vector3(fPos.x, fPos.y, fPos.z);
        }

        private Quaternion ConvertForgelightRotation(Vector4 fRot)
        {
            return Quaternion.Euler(fRot.y * Mathf.Rad2Deg, fRot.x * Mathf.Rad2Deg, fRot.z * Mathf.Rad2Deg);
        }

        private Vector3 ConvertForgelightScale(Vector4 fSca)
        {
            return new Vector3(-fSca.x, fSca.y, fSca.z);
        }
    }
}
