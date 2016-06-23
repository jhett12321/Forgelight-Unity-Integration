using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Forgelight.Attributes;
using UnityEditor;

namespace Forgelight.Formats.Zone
{
    public class ZoneObjectFactory : MonoBehaviour
    {
        private List<long> usedIDs = new List<long>();

        public GameObject CreateForgelightObject(ForgelightGame forgelightGame, string actorDefinition, Vector3 position, Quaternion rotation)
        {
            uint randID = GenerateUID();

            return CreateForgelightObject(forgelightGame, actorDefinition, position, rotation, Vector3.one, 1000, 1.0f, false, randID);
        }

        public GameObject CreateForgelightObject(ForgelightGame forgelightGame, string actorDefinition, Vector3 position, Quaternion rotation, Vector3 scale, float renderDistance, float lodMultiplier, bool dontCastShadows, uint id)
        {
            GameObject instance = InitializeActor(forgelightGame, actorDefinition);

            if (instance != null)
            {
                InitializeInstance(instance, position, rotation, scale, actorDefinition, renderDistance, lodMultiplier, dontCastShadows, id);
            }

            return instance;
        }

        public GameObject GetForgelightObject(ForgelightGame forgelightGame, string actorDefinition)
        {
            //By default, the actor definitions are appended with the .adr extension.
            string modelName = Path.GetFileNameWithoutExtension(actorDefinition) + "_LOD0.obj";
            string baseModelDir = "Assets/Resources/" + forgelightGame.Name + "/Models";
            string modelPath = baseModelDir + "/" + modelName;

            return AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        }

        public void DestroyAllObjects()
        {
            DestroyImmediate(gameObject);
        }

        private GameObject InitializeActor(ForgelightGame forgelightGame, string actorDef)
        {
            GameObject resourceObj = GetForgelightObject(forgelightGame, actorDef);

            GameObject actor = null;
            Renderer[] baseActorRenderers;

            if (resourceObj != null)
            {
                actor = (GameObject) PrefabUtility.InstantiatePrefab(resourceObj);

                if (actor != null)
                {
                    baseActorRenderers = actor.GetComponentsInChildren<Renderer>();

                    if (baseActorRenderers == null)
                    {
                        DestroyImmediate(actor);

                        Debug.LogWarning("Warning: Forgelight Object " + actorDef + " Failed to instantiate. Ignoring object.");

                        actor = null;
                    }
                }
            }

            if (actor == null)
            {
                //This model does not exist.
                actor = GameObject.CreatePrimitive(PrimitiveType.Cube);

                baseActorRenderers = actor.GetComponents<Renderer>();
                baseActorRenderers[0].sharedMaterial.color = Color.magenta;
            }

            int layer = LayerMask.NameToLayer("ForgelightZoneObject");

            actor.layer = layer;

            foreach (Transform child in actor.transform)
            {
                child.gameObject.layer = layer;
            }

            return actor;
        }

        private void InitializeInstance(GameObject instance, Vector3 position, Quaternion rotation, Vector3 scale, string actorDef, float renderDistance, float lodBias, bool dontCastShadows, long id)
        {
            //Set our position, scale and rotation values to the ones defined in the zone file.
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.transform.localScale = scale;

            //Attach ourselves to the master object parent.
            instance.transform.parent = transform;

            //Attach our ZoneObject script, and update its variables
            ZoneObject zoneObject = instance.GetComponent<ZoneObject>();
            if (zoneObject == null)
            {
                zoneObject = instance.AddComponent<ZoneObject>();
            }

            CullableObject cObject = instance.GetComponent<CullableObject>();

            if (cObject == null)
            {
                instance.AddComponent<CullableObject>();
            }

            zoneObject.actorDefinition = actorDef;
            zoneObject.renderDistance = renderDistance;
            zoneObject.lodMultiplier = lodBias;
            zoneObject.DontCastShadows = dontCastShadows;
            zoneObject.ID = id;

            //instance.isStatic = true;

            //Add the ID to our used list.
            usedIDs.Add(id);
        }

        public void ValidateObjectUIDs()
        {
            //This list may not be updated. We create a new one.
            usedIDs.Clear();

            foreach (ZoneObject zoneObject in GetComponentsInChildren<ZoneObject>())
            {
                if (usedIDs.Contains(zoneObject.ID))
                {
                    zoneObject.ID = GenerateUID();
                }

                usedIDs.Add(zoneObject.ID);
            }
        }

        private uint GenerateUID()
        {
            uint randID;

            do
            {
                randID = (uint) Random.Range(0, uint.MaxValue);
            }
            while (usedIDs.Contains(randID));

            return randID;
        }
    }

}