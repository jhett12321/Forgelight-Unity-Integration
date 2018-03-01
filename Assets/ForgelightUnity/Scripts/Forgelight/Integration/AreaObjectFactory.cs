namespace ForgelightUnity.Forgelight.Integration
{
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using Assets.Areas;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Rendering;
    using Utils;
    using MathUtils = Utils.MathUtils;

    public class AreaObjectFactory
    {
        private Dictionary<Color, Material> cachedMaterials = new Dictionary<Color, Material>();

        private Transform parent;
        private Transform Parent
        {
            get
            {
                if (parent == null)
                {
                    parent = new GameObject("Forgelight Areas").transform;
                    parent.gameObject.layer = LayerMask.NameToLayer("ForgelightAreas");
                    parent.gameObject.tag = "ForgelightAreas";
                }

                return parent;
            }
        }

        public void DestroyAreas()
        {
            if (parent != null)
            {
                Object.DestroyImmediate(parent.gameObject);
            }
            else
            {
                Object.DestroyImmediate(GameObject.FindGameObjectWithTag("ForgelightAreas"));
            }
        }

        public void LoadAreaDefinitions(Areas areas, float progressMin, float progressMax)
        {
            string areasName = Path.GetFileNameWithoutExtension(areas.Name);

            Parent.name += " - " + areasName;

            for (int i = 0; i < areas.AreaDefinitions.Count; i++)
            {
                AreaDefinition areaDefinition = areas.AreaDefinitions[i];
                AreaObject instance = null;

                switch (areaDefinition.Shape)
                {
                    case "sphere":
                        instance = GameObject.CreatePrimitive(PrimitiveType.Sphere).AddComponent<AreaObject>();

                        TransformData correctedSphereTransform = MathUtils.ConvertTransform(areaDefinition.Pos1, Vector4.zero, new Vector4(areaDefinition.Radius, areaDefinition.Radius, areaDefinition.Radius), false, TransformMode.Area);
                        instance.transform.position = correctedSphereTransform.Position;
                        instance.transform.rotation = Quaternion.Euler(correctedSphereTransform.Rotation);
                        instance.transform.localScale = correctedSphereTransform.Scale;

                        instance.Radius = areaDefinition.Radius;
                        break;
                    case "box":
                        instance = GameObject.CreatePrimitive(PrimitiveType.Cube).AddComponent<AreaObject>();
                        Vector3 pos1 = areaDefinition.Pos1;
                        Vector3 pos2 = areaDefinition.Pos2;

                        Vector3 fScale = pos2 - pos1;
                        fScale.x = Mathf.Abs(fScale.x);
                        fScale.y = Mathf.Abs(fScale.y);
                        fScale.z = Mathf.Abs(fScale.z);

                        Vector3 fPos = (pos1 + pos2) * 0.5f;
                        Vector4 fRot = areaDefinition.Rot;

                        TransformData correctedBoxMatrix = MathUtils.ConvertTransform(fPos, fRot, fScale, true, TransformMode.Area);
                        instance.transform.position = correctedBoxMatrix.Position;
                        instance.transform.rotation = Quaternion.Euler(correctedBoxMatrix.Rotation);
                        instance.transform.localScale = correctedBoxMatrix.Scale;

                        instance.Pos2 = areaDefinition.Pos2;
                        instance.Rot = areaDefinition.Rot;
                        break;
                }

                instance.name = areaDefinition.Name;

                int layer = LayerMask.NameToLayer("ForgelightAreas");
                instance.gameObject.layer = layer;

                foreach (Transform child in instance.transform)
                {
                    child.gameObject.layer = layer;
                }

                instance.ID = areaDefinition.ID;
                instance.Name = areaDefinition.Name;
                instance.Shape = areaDefinition.Shape;
                instance.Pos1 = areaDefinition.Pos1;

                instance.transform.SetParent(Parent.transform, true);

                //We color the area definition based on its parameter type.
                Renderer renderer = instance.GetComponent<Renderer>();
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;

                Color color = new Color(1.0f, 1.0f, 1.0f, 0.5f);

                if (areaDefinition.Properties != null && areaDefinition.Properties.Count > 0)
                {
                    instance.Properties = new List<string>();

                    foreach (Property property in areaDefinition.Properties)
                    {
                        instance.Properties.Add(property.Type + "_" + property.ID + ": " + property.Parameters.Attributes());
                    }

                    MD5 md5 = MD5.Create();

                    byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(areaDefinition.Properties[0].Type));

                    color.r = hash[0]/255.0f;
                    color.g = hash[1]/255.0f;
                    color.b = hash[2]/255.0f;
                }

                if (cachedMaterials.ContainsKey(color))
                {
                    renderer.sharedMaterial = cachedMaterials[color];
                }
                else
                {
                    renderer.sharedMaterial = new Material(Shader.Find("Custom/Areas"));
                    renderer.sharedMaterial.color = color;

                    cachedMaterials[color] = renderer.sharedMaterial;
                }

                EditorUtility.DisplayProgressBar("Loading " + areasName, "Loading Area Definition: " + areaDefinition.Name, MathUtils.Remap01((float)i / areas.AreaDefinitions.Count, progressMin, progressMax));
            }
        }
    }
}
