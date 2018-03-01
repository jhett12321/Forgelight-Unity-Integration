namespace ForgelightUnity.Forgelight.Integration
{
    using System.Collections.Generic;
    using System.IO;
    using Assets.Adr;
    using Assets.Zone;
    using UnityEditor;
    using UnityEngine;
    using Utils;
    using MathUtils = Utils.MathUtils;
    using Object = Assets.Zone.Object;

    public class ZoneObjectFactory
    {
        private const float cullPower = 5.0f;

        private HashSet<long> usedIDs = new HashSet<long>();

        private Transform parent;
        private Transform Parent
        {
            get
            {
                if (parent == null)
                {
                    GameObject parentGo = GameObject.FindGameObjectWithTag("ForgelightZoneObjects");

                    if (parentGo == null)
                    {
                        parentGo = new GameObject("Forgelight Zone Objects");
                        parentGo.layer = LayerMask.NameToLayer("ForgelightZoneObject");
                        parentGo.tag = "ForgelightZoneObjects";
                    }

                    parent = parentGo.transform;
                }

                return parent;
            }
        }

        private Dictionary<string, Transform> actorParents = new Dictionary<string, Transform>();

        public void DestroyAllObjects()
        {
            if (parent != null)
            {
                UnityEngine.Object.DestroyImmediate(parent.gameObject);
                parent = null;
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(GameObject.FindGameObjectWithTag("ForgelightZoneObjects"));
            }

            //Cleanup any null references we may hold to old actor parents.
            actorParents.Clear();
        }

        public void LoadZoneObjects(ForgelightGame forgelightGame, string zoneName, List<Object> objects, float progressMin, float progressMax)
        {
            Parent.name += " - " + zoneName;

            //Begin processing the file
            for (int i = 0; i < objects.Count; i++)
            {
                Object zoneObject = objects[i];

                if (zoneObject.Instances.Count > 0)
                {
                    Adr actorDef = forgelightGame.GetActorDefinition(zoneObject.ActorDefinition);

                    if (actorDef == null)
                    {
                        Debug.LogWarning("Could not find Actor Definition: " + zoneObject.ActorDefinition + "!");
                        continue;
                    }

                    foreach (Object.Instance instance in zoneObject.Instances)
                    {
                        TransformData correctedTransform = MathUtils.ConvertTransform(instance.Position, instance.Rotation, instance.Scale, true, TransformMode.Standard);

                        CreateForgelightObject(forgelightGame, actorDef, correctedTransform.Position, Quaternion.Euler(correctedTransform.Rotation), correctedTransform.Scale, zoneObject.RenderDistance, instance.LODMultiplier, instance.DontCastShadows, instance.ID);
                    }
                }

                EditorUtility.DisplayProgressBar("Loading Zone: " + zoneName, "Loading Objects: " + zoneObject.ActorDefinition, MathUtils.Remap01((float)i/objects.Count, progressMin, progressMax));
            }
        }

        public GameObject CreateForgelightObject(ForgelightGame forgelightGame, Adr actorDefinition, Vector3 position, Quaternion rotation)
        {
            uint randID = GenerateUID();

            return CreateForgelightObject(forgelightGame, actorDefinition, position, rotation, Vector3.one, 1000, 1.0f, false, randID);
        }

        public GameObject CreateForgelightObject(ForgelightGame forgelightGame, Adr actorDefinition, Vector3 position, Quaternion rotation, Vector3 scale, float renderDistance, float lodMultiplier, bool dontCastShadows, uint id)
        {
            GameObject instance = InitializeActor(forgelightGame, actorDefinition);

            if (instance != null)
            {
                InitializeInstance(instance, position, rotation, scale, actorDefinition, renderDistance, lodMultiplier, dontCastShadows, id);
            }

            return instance;
        }

        public GameObject GetForgelightObject(ForgelightGame forgelightGame, string model)
        {
            //By default, models are appended with a .dme extension.
            string modelName = Path.GetFileNameWithoutExtension(model) + ".obj";
            string baseModelDir = "Assets/Resources/" + forgelightGame.Name + "/Models";
            string modelPath = baseModelDir + "/" + modelName;

            return AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        }

        private GameObject InstantiateModel(GameObject prefab)
        {
            GameObject model = null;
            Renderer[] modelRenderers;

            if (prefab != null)
            {
                model = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

                if (model != null)
                {
                    modelRenderers = model.GetComponentsInChildren<Renderer>();

                    if (modelRenderers == null)
                    {
                        UnityEngine.Object.DestroyImmediate(model);

                        Debug.LogWarning("Warning: Forgelight Object Failed to instantiate. Ignoring object.");

                        model = null;
                    }
                }
            }

            if (model != null)
            {
                return model;
            }

            //This model does not exist.
            model = GameObject.CreatePrimitive(PrimitiveType.Cube);

            modelRenderers = model.GetComponents<Renderer>();
            modelRenderers[0].sharedMaterial.color = Color.magenta;

            return model;
        }

        private GameObject InitializeActor(ForgelightGame forgelightGame, Adr actorDef)
        {
            GameObject basePrefab = GetForgelightObject(forgelightGame, actorDef.Base);
            GameObject baseModel = InstantiateModel(basePrefab);

            if (baseModel == null)
            {
                return null;
            }

            //LODs
            //TODO Unity has a hard limit for LODs. Re-enable when fixed.
            //LODGroup lodGroup = baseModel.AddComponent<LODGroup>();

            ////This adds the base model, and the "culled" LOD.
            //int levelsOfDetail = actorDef.Lods.Count + 2;
            //float max = Mathf.Pow(levelsOfDetail, cullPower);

            //List<LOD> unityLods = new List<LOD>(levelsOfDetail);

            //LOD lod0 = new LOD();
            //lod0.renderers = baseModel.GetComponentsInChildren<Renderer>();
            //lod0.screenRelativeTransitionHeight = Mathf.Pow(levelsOfDetail - 1, cullPower).Remap(0, max, 0, 1.0f);

            //unityLods.Add(lod0);

            //for (int i = 0; i < actorDef.Lods.Count; i++)
            //{
            //    Lod lod = actorDef.Lods[i];
            //    GameObject lodPrefab = GetForgelightObject(forgelightGame, lod.FileName);
            //    GameObject lodModel = InstantiateModel(lodPrefab);

            //    if (lodModel == null)
            //    {
            //        continue;
            //    }

            //    lodModel.transform.SetParent(baseModel.transform);

            //    float cullDist = Mathf.Pow(levelsOfDetail - i - 2, cullPower).Remap(0, max, 0, 1.0f);

            //    unityLods.Add(new LOD(cullDist, lodModel.GetComponentsInChildren<Renderer>()));
            //}

            //lodGroup.SetLODs(unityLods.ToArray());

            int layer = LayerMask.NameToLayer("ForgelightZoneObject");

            baseModel.layer = layer;

            foreach (Transform child in baseModel.transform)
            {
                child.gameObject.layer = layer;
            }

            return baseModel;
        }

        private void InitializeInstance(GameObject instance, Vector3 position, Quaternion rotation, Vector3 scale, Adr actorDef, float renderDistance, float lodBias, bool dontCastShadows, long id)
        {
            //Set our position, scale and rotation values to the ones defined in the zone file.
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.transform.localScale = scale;

            //We attach ourselves to a common parent to improve undo performance.
            if (!actorParents.ContainsKey(actorDef.Name))
            {
                GameObject actorParent = new GameObject(actorDef.Name);
                actorParent.transform.SetParent(Parent, false);

                actorParents[actorDef.Name] = actorParent.transform;
            }

            //Attach ourselves to the master object parent.
            instance.transform.parent = actorParents[actorDef.Name];

            //Attach our ZoneObject script, and update its variables
            ZoneObject zoneObject = instance.GetComponent<ZoneObject>();
            if (zoneObject == null)
            {
                zoneObject = instance.AddComponent<ZoneObject>();
            }

            zoneObject.actorDefinition = actorDef.Name;
            zoneObject.renderDistance = renderDistance;
            zoneObject.lodMultiplier = lodBias;
            zoneObject.DontCastShadows = dontCastShadows;
            zoneObject.ID = id;

            //Apply any changes we may have made.
            zoneObject.OnValidate();

            //instance.isStatic = true;

            //Add the ID to our used list.
            usedIDs.Add(id);
        }

        public void ValidateObjectUIDs()
        {
            //This list may not be updated. We create a new one.
            usedIDs.Clear();

            foreach (ZoneObject zoneObject in Resources.FindObjectsOfTypeAll<ZoneObject>())
            {
                if (zoneObject.hideFlags == HideFlags.NotEditable || zoneObject.hideFlags == HideFlags.HideAndDontSave || EditorUtility.IsPersistent(zoneObject))
                {
                    continue;
                }

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
                if (zoneObject.hideFlags == HideFlags.NotEditable || zoneObject.hideFlags == HideFlags.HideAndDontSave || EditorUtility.IsPersistent(zoneObject))
                {
                    continue;
                }

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
                    //Make sure we don't have a parent.
                    Transform objectParent = zoneObject.transform.parent;
                    if (objectParent != null)
                    {
                        zoneObject.transform.SetParent(null);
                    }

                    Object.Instance instance = new Object.Instance();

                    TransformData correctedTransform = MathUtils.ConvertTransform(zoneObject.transform.position, zoneObject.transform.rotation.eulerAngles, zoneObject.transform.localScale, false, TransformMode.Standard);

                    instance.Position = correctedTransform.Position;
                    instance.Rotation = correctedTransform.Rotation.ToRadians();
                    instance.Scale = correctedTransform.Scale;

                    instance.ID = (uint)zoneObject.ID;
                    instance.DontCastShadows = zoneObject.DontCastShadows;
                    instance.LODMultiplier = zoneObject.lodMultiplier;

                    zoneObj.Instances.Add(instance);

                    //If we had a parent, reset our parent to the original.

                    if (objectParent != null)
                    {
                        zoneObject.transform.SetParent(objectParent);
                    }
                }

                zone.Objects.Add(zoneObj);
            }
        }
    }

}