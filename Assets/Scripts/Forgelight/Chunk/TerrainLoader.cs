
using System;
using System.IO;
using ForgelightInteg.Zone;
using UnityEditor;
using UnityEngine;

namespace ForgelightInteg.Chunk
{
    public class TerrainLoader
    {
        private const int chunkPosOffset = 32;

        private bool running = false;
        private int totalResources = 0;
        private int resourcesProcessed = 0;
        private string currentChunk = "";

        /// <summary>
        /// Loads Terrain data, using the default terrain directory. (Assets/Resources/Terrain)
        /// </summary>
        /// <param name="contPrefix">The terrain object prefix (before the underscore) for each terrain chunk.</param>
        public void LoadTerrain(string contPrefix)
        {
            LoadTerrain("Terrain/" + contPrefix, contPrefix);
        }

        public void LoadTerrain(string path, string contPrefix)
        {
            running = true;

            Transform terrainParent = new GameObject("Forgelight Terrain - " + contPrefix).transform;
            terrainParent.tag = "Terrain";

            string resourcePath = Application.dataPath + "/Resources/" + path;
            string[] resources = Directory.GetFiles(resourcePath, "*.obj");

            totalResources = resources.Length;

            foreach (string resource in resources)
            {
                if (running)
                {
                    string chunkName = Path.GetFileNameWithoutExtension(resource);
                    currentChunk = chunkName;

                    ProgressBar();

                    CreateChunk(path + "/" + chunkName, contPrefix, terrainParent);
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
                GameObject.DestroyImmediate(terrainParent.gameObject);
            }

            running = false;
            OnLoadComplete(true);
        }

        private void CreateChunk(string chunkPath, string contPrefix, Transform terrainParent)
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

                if (nameElements[0] == contPrefix)
                {
                    //Multiply the position on each axis by the size of the chunk, as we are given only chunk coordinates.
                    int chunkPosX = -(Convert.ToInt32(nameElements[2]) * chunkPosOffset);
                    int chunkPosZ = (Convert.ToInt32(nameElements[1]) * chunkPosOffset);

                    GameObject instance = GameObject.Instantiate(chunk, new Vector3(chunkPosX, 0, chunkPosZ), Quaternion.identity) as GameObject;

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
                float progress = (float)resourcesProcessed / (float)totalResources;

                if (EditorUtility.DisplayCancelableProgressBar("Importing Forgelight Terrain Data",
                    currentChunk != "" ? currentChunk : "Importing Forgelight Terrain Data.", progress))
                {
                    running = false;
                }
            }
        }
    }
}
