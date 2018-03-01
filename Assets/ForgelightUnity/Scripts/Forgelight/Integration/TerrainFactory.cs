namespace ForgelightUnity.Forgelight.Integration
{
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using MathUtils = Utils.MathUtils;
    using Object = UnityEngine.Object;

    public class TerrainFactory
    {
        private const int chunkPosOffset = 32;

        private Transform parent;
        private Transform Parent
        {
            get
            {
                if (parent == null)
                {
                    parent = new GameObject("Forgelight Terrain").transform;
                    parent.gameObject.layer = LayerMask.NameToLayer("ForgelightTerrain");
                    parent.tag = "ForgelightTerrain";
                }

                return parent;
            }
        }

        public void DestroyTerrain()
        {
            if (parent != null)
            {
                Object.DestroyImmediate(parent.gameObject);
            }
            else
            {
                Object.DestroyImmediate(GameObject.FindGameObjectWithTag("ForgelightTerrain"));
            }
        }

        public void LoadTerrain(ForgelightGame forgelightGame, string contPrefix, float progressMin, float progressMax)
        {
            Parent.name += " - " + contPrefix;

            string resourcePath = forgelightGame.Name + "/Terrain/" + contPrefix;

            if (!Directory.Exists(Application.dataPath + "/Resources/" + resourcePath))
            {
                Debug.LogWarning("Could not find terrain for zone " + contPrefix);

                return;
            }

            string[] resources = Directory.GetFiles(Application.dataPath + "/Resources/" + resourcePath, "*.obj");

            int totalResources;
            int resourcesProcessed = 0;
            string currentChunk = "";
            totalResources = resources.Length;

            foreach (string resource in resources)
            {
                string chunkName = Path.GetFileNameWithoutExtension(resource);
                currentChunk = chunkName;

                EditorUtility.DisplayProgressBar("Loading Zone: " + contPrefix, "Loading Terrain: " + currentChunk, MathUtils.Remap01((float)totalResources / totalResources, progressMin, progressMax));

                CreateChunk(resourcePath + "/" + chunkName, Parent);

                resourcesProcessed++;
            }

            //The terrain uses a different scale and coordinate system. We need to flip the x axis and multiply by 2.
            Parent.localScale = new Vector3(2, 2, 2);

            //Destroy the parent if we did not create any children.
            if (resourcesProcessed == 0)
            {
                Object.DestroyImmediate(Parent.gameObject);
            }

            Resources.UnloadUnusedAssets();
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
                instance.AddComponent<TerrainChunk>();

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
    }
}
