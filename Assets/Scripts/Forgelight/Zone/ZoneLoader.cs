using UnityEngine;
using System.IO;
using System.Linq;
using ForgelightInteg.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace ForgelightInteg.Zone
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

            LoadZone(path);
        }

        public void LoadZone(string path)
        {
            running = true;

            using (StreamReader reader = File.OpenText(@path))
            {
                JObject zoneData = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                loadedZone = zoneData;
                loadedZonePath = path;

                //Calculate the total objects we need to process.
                foreach (JObject zoneObject in zoneData["objects"])
                {
                    totalObjects += zoneObject["instances"].Count();
                }

                ZoneObjectFactory ZoneObjectFactory = Forgelight.Instance.ZoneObjectFactory;

                //Begin processing the file
                foreach (JObject zoneObject in zoneData["objects"])
                {
                    if (running && zoneObject["instances"].Count() > 0)
                    {
                        string actorDefinition = (string)zoneObject["actorDefinition"];
                        int renderDistance = (int)zoneObject["renderDistance"];

                        for (int i = 0; i < zoneObject["instances"].Count(); i++)
                        {
                            JObject instanceData = (JObject)zoneObject["instances"][i];

                            Vector3 position = GetPositionFromInstance(instanceData);
                            Quaternion rotation = GetRotationFromInstance(instanceData);
                            Vector3 scale = GetScaleFromInstance(instanceData);

                            float unknownFloat1 = (float)instanceData["unknownFloat1"];
                            byte unknownByte1 = (byte)instanceData["unknownByte1"];
                            long id = (long)instanceData["id"];

                            ZoneObjectFactory.CreateForgelightObject(actorDefinition, position, rotation, scale, renderDistance, unknownFloat1, unknownByte1, id);
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
            }

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

        private Vector3 GetPositionFromInstance(JObject instance)
        {
            float posX = (float)instance["position"][0];
            float posY = (float)instance["position"][1];
            float posZ = (float)instance["position"][2];

            return new Vector3(posX, posY, posZ);
        }

        private Quaternion GetRotationFromInstance(JObject instance)
        {
            float x = (float)instance["rotation"][0] * Mathf.Rad2Deg;
            float y = (float)instance["rotation"][1] * Mathf.Rad2Deg;
            float z = (float)instance["rotation"][2] * Mathf.Rad2Deg;

            return Quaternion.Euler(y, x, z);
        }

        private Vector3 GetScaleFromInstance(JObject instance)
        {
            float scaleX = -(float)instance["scale"][0];
            float scaleY = (float)instance["scale"][1];
            float scaleZ = (float)instance["scale"][2];

            return new Vector3(scaleX, scaleY, scaleZ);
        }
    }
}
