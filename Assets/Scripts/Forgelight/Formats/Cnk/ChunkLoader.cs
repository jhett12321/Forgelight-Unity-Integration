using System;
using System.IO;
using Forgelight.Attributes;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Forgelight.Formats.Cnk
{
    public class ChunkLoader
    {
        private const int chunkPosOffset = 32;

        private bool running;

        public void DestroyTerrain()
        {
            GameObject terrain = GameObject.FindWithTag("Terrain");

            if (terrain != null)
            {
                Object.DestroyImmediate(terrain);
            }
        }

        public void LoadTerrain(ForgelightGame forgelightGame, string contPrefix, float progressMin, float progressMax)
        {
            running = true;

            Transform terrainParent = new GameObject("Forgelight Terrain - " + contPrefix).transform;
            terrainParent.tag = "Terrain";
            //terrainParent.gameObject.isStatic = true;

            string resourcePath = forgelightGame.Name + "/Terrain/" + contPrefix;

            if (!Directory.Exists(Application.dataPath + "/Resources/" + resourcePath))
            {
                Debug.LogWarning("Could not find terrain for zone " + contPrefix);
                Object.DestroyImmediate(terrainParent.gameObject);
                return;
            }

            string[] resources = Directory.GetFiles(Application.dataPath + "/Resources/" + resourcePath, "*.obj");

            int totalResources;
            int resourcesProcessed = 0;
            string currentChunk = "";
            totalResources = resources.Length;

            foreach (string resource in resources)
            {
                if (running)
                {
                    string chunkName = Path.GetFileNameWithoutExtension(resource);
                    currentChunk = chunkName;

                    ProgressBar(Utils.MathUtils.RemapProgress((float) resourcesProcessed / totalResources, progressMin, progressMax), currentChunk);

                    CreateChunk(resourcePath + "/" + chunkName, terrainParent);

                    resourcesProcessed++;
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
                string[] nameElements = chunk.name.Split('_');

                //Multiply the position on each axis by the size of the chunk, as we are given only chunk coordinates.
                int chunkPosX = -(Convert.ToInt32(nameElements[2]) * chunkPosOffset);
                int chunkPosZ = (Convert.ToInt32(nameElements[1]) * chunkPosOffset);

                GameObject instance = (GameObject) PrefabUtility.InstantiatePrefab(chunk);
                instance.transform.position = new Vector3(chunkPosX, 0, chunkPosZ);

                instance.transform.SetParent(terrainParent);

                //Used for cull purposes.
                instance.AddComponent<CullableObject>();

                //instance.isStatic = true;
                //foreach (Transform child in instance.transform)
                //{
                //    child.gameObject.isStatic = true;
                //}

                int layer = LayerMask.NameToLayer("ForgelightTerrain");
                instance.layer = layer;

                foreach (Transform child in instance.transform)
                {
                    child.gameObject.layer = layer;
                }
            }
        }

        public void OnLoadComplete(bool completed)
        {
            //Unload any unused assets.
            Resources.UnloadUnusedAssets();

            EditorUtility.ClearProgressBar();
        }

        private void ProgressBar(float progress, string currentTask)
        {
            EditorUtility.DisplayProgressBar("Loading Zone: " + ForgelightExtension.Instance.ZoneManager.LoadedZone.Name, currentTask, progress);
        }
    }
}
