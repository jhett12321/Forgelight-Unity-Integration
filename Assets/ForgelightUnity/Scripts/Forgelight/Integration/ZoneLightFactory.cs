namespace ForgelightUnity.Forgelight.Integration
{
    using System.Collections.Generic;
    using Assets.Zone;
    using UnityEditor;
    using UnityEngine;
    using Utils;
    using Light = Assets.Zone.Light;
    using MathUtils = Utils.MathUtils;
    using Object = UnityEngine.Object;

    public class ZoneLightFactory
    {
        private HashSet<long> usedIDs = new HashSet<long>();

        private Transform parent;
        private Transform Parent
        {
            get
            {
                if (parent == null)
                {
                    parent = new GameObject("Forgelight Zone Lights").transform;
                    parent.gameObject.layer = LayerMask.NameToLayer("ForgelightZoneLight");
                    parent.gameObject.tag = "ForgelightZoneLights";
                }

                return parent;
            }
        }

        public void DestroyAllLights()
        {
            if (parent != null)
            {
                Object.DestroyImmediate(parent.gameObject);
            }
            else
            {
                Object.DestroyImmediate(GameObject.FindGameObjectWithTag("ForgelightZoneLights"));
            }
        }

        public void LoadZoneLights(string zoneName, List<Light> lights, float progressMin, float progressMax)
        {
            Parent.name += " - " + zoneName;

            for (int i = 0; i < lights.Count; i++)
            {
                Light lightData = lights[i];

                ZoneLight instance = new GameObject(lightData.Name).AddComponent<ZoneLight>();

                UnityEngine.Light lightComponent = instance.gameObject.AddComponent<UnityEngine.Light>();
                lightComponent.intensity = 4.0f;
                lightComponent.bounceIntensity = 0.0f;

                instance.lightObject = lightComponent;

                //Params
                TransformData correctedTransform = MathUtils.ConvertTransform(lightData.Position, lightData.Rotation, Vector3.one, true, TransformMode.Standard);

                instance.transform.position = correctedTransform.Position;
                instance.transform.rotation = Quaternion.Euler(correctedTransform.Rotation);

                instance.Name = lightData.Name;
                instance.ColorName = lightData.ColorName;
                instance.Type = lightData.Type;
                instance.UnknownFloat1 = lightData.UnknownFloat1;
                instance.Range = lightData.Range;
                instance.InnerRange = lightData.InnerRange;
                instance.Color = lightData.Color;
                instance.UnknownByte1 = lightData.UnknownByte1;
                instance.UnknownByte2 = lightData.UnknownByte2;
                instance.UnknownByte3 = lightData.UnknownByte3;
                instance.UnknownByte4 = lightData.UnknownByte4;
                instance.UnknownByte5 = lightData.UnknownByte5;
                instance.UnknownVector1 = lightData.UnknownVector1;
                instance.UnknownString1 = lightData.UnknownString1;
                instance.ID = lightData.ID;

                instance.transform.parent = Parent;

                //Apply any changes we may have made.
                instance.OnValidate();

                int layer = LayerMask.NameToLayer("ForgelightZoneLight");
                instance.gameObject.layer = layer;

                foreach (Transform child in instance.transform)
                {
                    child.gameObject.layer = layer;
                }

                EditorUtility.DisplayProgressBar("Loading Zone: " + zoneName, "Loading Lights: " + lightData.Name, MathUtils.Remap01((float) i/lights.Count, progressMin, progressMax));
            }
        }

        public void ValidateObjectUIDs()
        {
            //This list may not be updated. We create a new one.
            usedIDs.Clear();

            foreach (ZoneLight zoneLight in Resources.FindObjectsOfTypeAll<ZoneLight>())
            {
                if (zoneLight.hideFlags == HideFlags.NotEditable || zoneLight.hideFlags == HideFlags.HideAndDontSave || EditorUtility.IsPersistent(zoneLight))
                {
                    continue;
                }

                if (usedIDs.Contains(zoneLight.ID))
                {
                    zoneLight.ID = GenerateUID();
                    zoneLight.Name = "ForgeUnityLight-" + zoneLight.ID;
                }

                usedIDs.Add(zoneLight.ID);
            }
        }

        private uint GenerateUID()
        {
            uint randID;

            do
            {
                randID = (uint)Random.Range(0, uint.MaxValue);
            }
            while (usedIDs.Contains(randID));

            return randID;
        }

        public void WriteToZone(Zone zone)
        {
            ValidateObjectUIDs();

            zone.Lights.Clear();

            foreach (ZoneLight zoneLight in Resources.FindObjectsOfTypeAll<ZoneLight>())
            {
                if (zoneLight.hideFlags == HideFlags.NotEditable || zoneLight.hideFlags == HideFlags.HideAndDontSave || EditorUtility.IsPersistent(zoneLight))
                {
                    continue;
                }

                Light light = new Light();

                TransformData correctedTransform = MathUtils.ConvertTransform(zoneLight.transform.position, zoneLight.transform.rotation.eulerAngles, Vector3.one, false, TransformMode.Standard);
                light.Position = correctedTransform.Position;
                Vector3 rotationData = correctedTransform.Rotation.ToRadians();
                light.Rotation = new Vector4(rotationData.x, rotationData.y, rotationData.z, 1);

                light.Name = zoneLight.Name;
                light.ColorName = zoneLight.ColorName;
                light.Type = zoneLight.Type;
                light.UnknownFloat1 = zoneLight.UnknownFloat1;
                light.Range = zoneLight.Range;
                light.InnerRange = zoneLight.InnerRange;
                light.Color = zoneLight.Color;
                light.UnknownByte1 = zoneLight.UnknownByte1;
                light.UnknownByte2 = zoneLight.UnknownByte2;
                light.UnknownByte3 = zoneLight.UnknownByte3;
                light.UnknownByte4 = zoneLight.UnknownByte4;
                light.UnknownByte5 = zoneLight.UnknownByte5;
                light.UnknownVector1 = zoneLight.UnknownVector1;
                light.UnknownString1 = zoneLight.UnknownString1;
                light.ID = zoneLight.ID;

                zone.Lights.Add(light);
            }
        }
    }
}
