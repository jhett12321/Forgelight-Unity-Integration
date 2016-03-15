using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace Forgelight.Formats.Zone
{
    public class ZoneObjectFactory : MonoBehaviour
    {
        private List<long> usedIDs = new List<long>();
        private Dictionary<ForgelightGame, Dictionary<string, GameObject>> cachedActors = new Dictionary<ForgelightGame, Dictionary<string, GameObject>>();

        public GameObject CreateForgelightObject(ForgelightGame forgelightGame, string actorDefinition, Vector3 position, Quaternion rotation)
        {
            uint randID = GenerateUID();

            return CreateForgelightObject(forgelightGame, actorDefinition, position, rotation, Vector3.one, 1000, 1.0f, false, randID);
        }

        public GameObject CreateForgelightObject(ForgelightGame forgelightGame, string actorDefinition, Vector3 position, Quaternion rotation, Vector3 scale, float renderDistance, float lodMultiplier, bool dontCastShadows, uint id)
        {
            GameObject baseActor;
            GameObject instance = null;

            if (!cachedActors.ContainsKey(forgelightGame))
            {
                cachedActors[forgelightGame] = new Dictionary<string, GameObject>();
            }

            if (!cachedActors[forgelightGame].ContainsKey(actorDefinition))
            {
                baseActor = InitializeBaseActor(forgelightGame, actorDefinition);

                instance = baseActor;
                cachedActors[forgelightGame][actorDefinition] = baseActor;
            }

            else
            {
                baseActor = cachedActors[forgelightGame][actorDefinition];

                if (baseActor == null)
                {
                    baseActor = InitializeBaseActor(forgelightGame, actorDefinition);

                    instance = baseActor;
                    cachedActors[forgelightGame][actorDefinition] = baseActor;
                }

                else
                {
                    instance = Instantiate(baseActor);
                }
            }

            if (instance != null)
            {
                InitializeInstance(instance, position, rotation, scale, actorDefinition, renderDistance, lodMultiplier, dontCastShadows, id);
            }

            return instance;
        }

        public void DestroyAllObjects()
        {
            DestroyImmediate(gameObject);
        }

        public void UpdateForgelightObject(ForgelightGame forgelightGame, ZoneObject forgeLightObject, string newActorDefinition)
        {
            GameObject baseActor = InitializeBaseActor(forgelightGame, newActorDefinition);

            foreach (Transform child in forgeLightObject.transform)
            {
                forgeLightObject.DestroyObject(child.gameObject);
            }

            foreach (Transform child in baseActor.transform)
            {
                GameObject mesh = (GameObject) Instantiate(child.gameObject, child.position, child.rotation);
                mesh.transform.SetParent(forgeLightObject.transform, false);
            }

            forgeLightObject.name = baseActor.name;
        }

        private GameObject InitializeBaseActor(ForgelightGame forgelightGame, string actorDef)
        {
            //By default, the actor definitions are appended with the .adr extension.
            string modelName = Path.GetFileNameWithoutExtension(actorDef) + "_LOD0.obj";
            string baseModelDir = "Assets/Resources/" + forgelightGame.Name + "/Models";
            string modelPath = baseModelDir + "/" + modelName;

            GameObject resourceObj = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);

            GameObject baseActor;
            Renderer baseActorRenderer;

            if (resourceObj != null)
            {
                baseActor = Instantiate(resourceObj);

                if (baseActor == null)
                {
                    return null;
                }

                baseActorRenderer = baseActor.GetComponentInChildren<Renderer>();

                if (baseActorRenderer == null)
                {
                    DestroyImmediate(baseActor);

                    Debug.LogWarning("Warning: Forgelight Object " + modelName + " Failed to instantiate. Ignoring object.");

                    return null;
                }
            }

            else
            {
                //This model does not exist.
                baseActor = GameObject.CreatePrimitive(PrimitiveType.Cube);

                baseActorRenderer = baseActor.GetComponent<Renderer>();
                baseActorRenderer.sharedMaterial.color = Color.magenta;
            }

            return baseActor;
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

            zoneObject.actorDefinition = actorDef;
            zoneObject.renderDistance = renderDistance;
            zoneObject.lodMultiplier = lodBias;
            zoneObject.DontCastShadows = dontCastShadows;
            zoneObject.ID = id;

            //Add the ID to our used list.
            usedIDs.Add(id);

            //Hide the gameobject. It will be made visible when we are in render range.
            zoneObject.Hide();
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