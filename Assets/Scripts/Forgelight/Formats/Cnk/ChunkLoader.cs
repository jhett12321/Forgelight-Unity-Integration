using System;
using System.IO;
using Forgelight.Formats.Zone;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Forgelight.Formats.Cnk
{
    public class ChunkLoader
    {
        private const int chunkPosOffset = 32;

        private bool running;
        private int totalResources;
        private int resourcesProcessed;
        private string currentChunk = "";

        public void DestroyTerrain()
        {
            GameObject terrain = GameObject.FindWithTag("Terrain");

            if (terrain != null)
            {
                Object.DestroyImmediate(terrain);
            }
        }

        public void LoadTerrain(ForgelightGame forgelightGame, string contPrefix)
        {
            running = true;

            Transform terrainParent = new GameObject("Forgelight Terrain - " + contPrefix).transform;
            terrainParent.tag = "Terrain";

            string resourcePath = forgelightGame.Name + "/Terrain/" + contPrefix;

            if (!Directory.Exists(Application.dataPath + "/Resources/" + resourcePath))
            {
                Debug.LogWarning("Could not find terrain for zone " + contPrefix);
                Object.DestroyImmediate(terrainParent.gameObject);
                return;
            }

            string[] resources = Directory.GetFiles(Application.dataPath + "/Resources/" + resourcePath, "*.obj");

            totalResources = resources.Length;

            foreach (string resource in resources)
            {
                if (running)
                {
                    string chunkName = Path.GetFileNameWithoutExtension(resource);
                    currentChunk = chunkName;

                    ProgressBar();

                    CreateChunk(resourcePath + "/" + chunkName, terrainParent);
                }

                else
                {
                    OnLoadComplete(false);
                }
            }

            //The terrain uses a different scale and coordinate system. We need to flip the x axis and multiply by 2.
            terrainParent.localScale = new Vector3(2, 2, 2);

            //Destroy the parent if we did not create any children.
            if (resourcesProcessed == 0)
            {
                Object.DestroyImmediate(terrainParent.gameObject);
            }

            running = false;
            OnLoadComplete(true);
        }

        private void CreateChunk(string chunkPath, Transform terrainParent)
        {
            object resource = Resources.Load(chunkPath);

            GameObject chunk = resource as GameObject;

            if (chunk != null)
            {
                currentChunk = chunk.name;
            }

            if (chunk != null)
            {
                string[] nameElements = chunk.name.Split('_');

                //Multiply the position on each axis by the size of the chunk, as we are given only chunk coordinates.
                int chunkPosX = -(Convert.ToInt32(nameElements[2]) * chunkPosOffset);
                int chunkPosZ = (Convert.ToInt32(nameElements[1]) * chunkPosOffset);

                GameObject instance = (GameObject) Object.Instantiate(chunk, new Vector3(chunkPosX, 0, chunkPosZ), Quaternion.identity);

                instance.transform.parent = terrainParent;

                //TODO HACK - We do not have the RAM to hold an entire continent in memory.
                ZoneObject zoneObject = instance.GetComponent<ZoneObject>();
                if (zoneObject == null)
                {
                    zoneObject = instance.AddComponent<ZoneObject>();
                }

                zoneObject.renderDistance = 3000;
                zoneObject.Hide();
            }

            resourcesProcessed++;
        }

        public void OnLoadComplete(bool completed)
        {
            //Unload any unused assets.
            Resources.UnloadUnusedAssets();

            EditorUtility.ClearProgressBar();

            totalResources = 0;
            resourcesProcessed = 0;
        }

        private void ProgressBar()
        {
            if (running)
            {
                float progress = resourcesProcessed / (float)totalResources;

                if (EditorUtility.DisplayCancelableProgressBar("Importing Forgelight Terrain Data",
                    currentChunk != "" ? currentChunk : "Importing Forgelight Terrain Data.", progress))
                {
                    running = false;
                }
            }
        }
    }
}
