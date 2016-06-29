using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Forgelight.Attributes;
using Forgelight.Formats.Zone;
using Forgelight.Utils;
using UnityEditor;
using Object = Forgelight.Formats.Zone.Object;
using MathUtils = Forgelight.Utils.MathUtils;

namespace Forgelight.Editor
{
    public class ZoneObjectFactory
    {
        private HashSet<long> usedIDs = new HashSet<long>();

        private Transform parent;
        private Transform Parent
        {
            get
            {
                if (parent == null)
                {
                    parent = new GameObject("Forgelight Zone Objects").transform;
                    parent.gameObject.layer = LayerMask.NameToLayer("ForgelightZoneObject");
                    parent.gameObject.tag = "ForgelightZoneObjects";
                }

                return parent;
            }
        }

        public void DestroyAllObjects()
        {
            if (parent != null)
            {
                UnityEngine.Object.DestroyImmediate(parent.gameObject);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(GameObject.FindGameObjectWithTag("ForgelightZoneObjects"));
            }
        }

        public void LoadZoneObjects(ForgelightGame forgelightGame, string zoneName, List<Object> objects, float progressMin, float progressMax)
        {
            Parent.name += " - " + zoneName;

            //Begin processing the file
            for (int i = 0; i < objects.Count; i++)
            {
                Object zoneObject = objects[i];

                if (zoneObject.Instances.Any())
                {
                    foreach (Object.Instance instance in zoneObject.Instances)
                    {
                        Matrix4x4 correctedTransform = MathUtils.ConvertTransform(instance.Position, instance.Rotation, instance.Scale, true, true);

                        CreateForgelightObject(forgelightGame, zoneObject.ActorDefinition, correctedTransform.ExtractTranslationFromMatrix(), correctedTransform.ExtractRotationFromMatrix(), correctedTransform.ExtractScaleFromMatrix(), zoneObject.RenderDistance, instance.LODMultiplier, instance.DontCastShadows, instance.ID);
                    }
                }

                EditorUtility.DisplayProgressBar("Loading Zone: " + zoneName, "Loading Objects: " + zoneObject.ActorDefinition, MathUtils.RemapProgress((float)i/objects.Count, progressMin, progressMax));
            }

            //Forgelight -> Unity position fix
            Parent.transform.localScale = new Vector3(-1, 1, 1);
        }

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
                        UnityEngine.Object.DestroyImmediate(actor);

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
            instance.transform.parent = Parent;

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

            ZoneObject[] zoneObjects = Resources.FindObjectsOfTypeAll<ZoneObject>();

            foreach (ZoneObject zoneObject in zoneObjects)
            {
                if (zoneObject.hideFlags == HideFlags.NotEditable || zoneObject.hideFlags == HideFlags.HideAndDontSave)
                    continue;

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

        public void WriteToZone(Zone zone)
        {
            //Objects
            Dictionary<string, List<ZoneObject>> actorInstances = new Dictionary<string, List<ZoneObject>>();

            //Check to make sure we don't have duplicate ID's
            ValidateObjectUIDs();

            foreach (ZoneObject zoneObject in Resources.FindObjectsOfTypeAll<ZoneObject>())
            {
                if (zoneObject.hideFlags == HideFlags.NotEditable || zoneObject.hideFlags == HideFlags.HideAndDontSave)
                    continue;

                string actorDef = zoneObject.actorDefinition;
                if (!actorInstances.ContainsKey(actorDef))
                {
                    actorInstances.Add(actorDef, new List<ZoneObject>());
                }

                actorInstances[actorDef].Add(zoneObject);
            }

            zone.Objects.Clear();
            foreach (var actorInstanceList in actorInstances)
            {
                Object zoneObj = new Object();

                zoneObj.ActorDefinition = actorInstanceList.Key;
                zoneObj.RenderDistance = actorInstanceList.Value[0].renderDistance;

                zoneObj.Instances = new List<Object.Instance>();

                foreach (ZoneObject zoneObject in actorInstanceList.Value)
                {
                    Object.Instance instance = new Object.Instance();

                    Matrix4x4 correctedTransform = MathUtils.ConvertTransform(zoneObject.transform.position, zoneObject.transform.rotation.eulerAngles, zoneObject.transform.localScale, false, true);

                    instance.Position = correctedTransform.ExtractTranslationFromMatrix();
                    instance.Rotation = correctedTransform.ExtractRotationFromMatrix().eulerAngles.ToRadians();
                    instance.Scale = correctedTransform.ExtractScaleFromMatrix();

                    instance.ID = (uint)zoneObject.ID;
                    instance.DontCastShadows = zoneObject.DontCastShadows;
                    instance.LODMultiplier = zoneObject.lodMultiplier;

                    zoneObj.Instances.Add(instance);
                }

                zone.Objects.Add(zoneObj);
            }
        }
    }

}