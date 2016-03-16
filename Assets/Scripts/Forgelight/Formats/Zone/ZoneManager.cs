using System.Collections.Generic;
using System.IO;
using System.Linq;
using Forgelight.Formats.Cnk;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Forgelight.Formats.Zone
{
    public class ZoneManager
    {
        private bool running = false;
        public Zone LoadedZone { get; private set; }

        public void ChangeZone(ForgelightGame forgelightGame, Zone zone)
        {
            //Destroy any objects in the current zone.
            ForgelightExtension.Instance.ZoneObjectFactory.DestroyAllObjects();

            //Destroy the terrain
            ForgelightExtension.Instance.ChunkLoader.DestroyTerrain();

            running = true;

            int totalObjects = 0;
            int objectsProcessed = 0;

            //Calculate the total objects we need to process.
            foreach (Object zoneObject in zone.Objects)
            {
                totalObjects += zoneObject.Instances.Count();
            }

            ZoneObjectFactory ZoneObjectFactory = ForgelightExtension.Instance.ZoneObjectFactory;

            //Begin processing the file
            foreach (Object zoneObject in zone.Objects)
            {
                if (running && zoneObject.Instances.Any())
                {
                    foreach (Object.Instance instance in zoneObject.Instances)
                    {
                        ZoneObjectFactory.CreateForgelightObject(forgelightGame, zoneObject.ActorDefinition, ConvertForgelightPosition(instance.Position), ConvertForgelightRotation(instance.Rotation), ConvertForgelightScale(instance.Scale), zoneObject.RenderDistance, instance.LODMultiplier, instance.DontCastShadows, instance.ID);
                        objectsProcessed++;
                    }
                }

                else if (!running)
                {
                    OnLoadComplete();
                    return;
                }

                ProgressBar(Utils.MathUtils.RemapProgress((float)objectsProcessed / (float)totalObjects, 0.0f, 0.50f), zoneObject.ActorDefinition);
            }

            ZoneObjectFactory.transform.localScale = new Vector3(-1, 1, 1);

            //Load this zone's terrain, if it exists
            ForgelightExtension.Instance.ChunkLoader.LoadTerrain(forgelightGame, Path.GetFileNameWithoutExtension(zone.Name));

            //Unload any unused assets.
            OnLoadComplete();
            LoadedZone = zone;
        }

        /// <summary>
        /// Merges the current scene into the
        /// </summary>
        public void ApplySceneChangesToZone()
        {
            Dictionary<string, List<ZoneObject>> actorInstances = new Dictionary<string, List<ZoneObject>>();

            //Check to make sure we don't have duplicate ID's
            ForgelightExtension.Instance.ZoneObjectFactory.ValidateObjectUIDs();

            foreach (ZoneObject zoneObject in UnityEngine.Object.FindObjectsOfType<ZoneObject>())
            {
                string actorDef = zoneObject.actorDefinition;
                if (!actorInstances.ContainsKey(actorDef))
                {
                    actorInstances.Add(actorDef, new List<ZoneObject>());
                }

                actorInstances[actorDef].Add(zoneObject);
            }

            //Objects
            LoadedZone.Objects.Clear();
            foreach (var actorInstanceList in actorInstances)
            {
                Object zoneObj = new Object();

                zoneObj.ActorDefinition = actorInstanceList.Key;
                zoneObj.RenderDistance = actorInstanceList.Value[0].renderDistance;

                zoneObj.Instances = new List<Object.Instance>();

                foreach (ZoneObject zoneObject in actorInstanceList.Value)
                {
                    Object.Instance instance = new Object.Instance();

                    Vector3 rawPosition = zoneObject.transform.position;
                    instance.Position = new Vector4(-rawPosition.x, rawPosition.y, rawPosition.z, 1.0f);

                    Vector3 rawRotation = zoneObject.transform.rotation.eulerAngles;
                    instance.Rotation = new Vector4(rawRotation.y * Mathf.Deg2Rad, rawRotation.x * Mathf.Deg2Rad, rawRotation.z * Mathf.Deg2Rad, 0);

                    Vector3 rawScale = zoneObject.transform.localScale;
                    instance.Scale = new Vector4(-rawScale.x, rawScale.y, rawScale.z, 1.0f);

                    instance.ID = (uint) zoneObject.ID;
                    instance.DontCastShadows = zoneObject.DontCastShadows;
                    instance.LODMultiplier = zoneObject.lodMultiplier;

                    zoneObj.Instances.Add(instance);
                }

                LoadedZone.Objects.Add(zoneObj);
            }

            //TODO ecos
            //TODO floras
            //TODO invisible walls
            //TODO lights
            //TODO unknowns
        }

        public void OnLoadComplete()
        {
            running = false;

            //Unload any unused assets.
            Resources.UnloadUnusedAssets();

            EditorUtility.ClearProgressBar();
        }

        private void ProgressBar(float progress, string currentTask)
        {
            if (EditorUtility.DisplayCancelableProgressBar("Loading Zone", currentTask, progress))
            {
                OnLoadComplete();
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
