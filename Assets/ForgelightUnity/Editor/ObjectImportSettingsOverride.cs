namespace ForgelightUnity.Editor
{
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    public class ObjectImportSettingsOverride : AssetPostprocessor
    {
        private const string bumpMatch = "bump ";
        private const string specMatch = "map_Ns ";

        public void OnPostprocessModel(GameObject gameObject)
        {
            //Forgelight Models
            if (assetPath.Contains("Models"))
            {
                foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
                {
                    Material sharedMaterial = renderer.sharedMaterial;
                    sharedMaterial.shader = Shader.Find("Custom/ForgelightModel");

                    if (assetPath == null || sharedMaterial.mainTexture == null)
                    {
                        return;
                    }

                    string mtlFilePath = Path.GetFullPath(Directory.GetParent(assetPath).FullName + "/" + Path.GetFileNameWithoutExtension(sharedMaterial.mainTexture.name) + ".mtl");

                    if (File.Exists(mtlFilePath))
                    {
                        string[] mtlDefs = File.ReadAllLines(mtlFilePath);

                        foreach (string mtlDef in mtlDefs)
                        {
                            ProcessMaterialDef(Path.GetDirectoryName(AssetDatabase.GetAssetPath(sharedMaterial.mainTexture)) + "/", sharedMaterial, mtlDef);
                        }
                    }
                }
            }

            else if (assetPath.Contains("Terrain"))
            {
                foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
                {
                    Material sharedMaterial = renderer.sharedMaterial;
                    sharedMaterial.shader = Shader.Find("Custom/ForgelightTerrain");

                    if (assetPath == null || sharedMaterial.mainTexture == null)
                    {
                        return;
                    }

                    string mtlFilePath = Path.GetFullPath(Directory.GetParent(assetPath).FullName + "/" + Path.GetFileNameWithoutExtension(assetPath) + ".mtl");

                    if (File.Exists(mtlFilePath))
                    {
                        string[] mtlDefs = File.ReadAllLines(mtlFilePath);

                        foreach (string mtlDef in mtlDefs)
                        {
                            ProcessMaterialDef(Path.GetDirectoryName(AssetDatabase.GetAssetPath(sharedMaterial.mainTexture)) + "/", sharedMaterial, mtlDef);
                        }
                    }
                }
            }
        }

        private void ProcessMaterialDef(string basePath, Material sharedMaterial, string mtlDef)
        {
            if (mtlDef.Contains(bumpMatch))
            {
                string bumpMap = mtlDef.Replace(bumpMatch, "");
                string path = basePath + bumpMap;

                Texture texture = (Texture)AssetDatabase.LoadAssetAtPath(path, typeof(Texture));

                if (texture != null)
                {
                    sharedMaterial.SetTexture("_BumpMap", texture);
                }
            }

            else if (mtlDef.Contains(specMatch))
            {
                string packedSpec = mtlDef.Replace(specMatch, "");
                string path = basePath + packedSpec;

                Texture texture = (Texture)AssetDatabase.LoadAssetAtPath(path, typeof(Texture));

                if (texture != null)
                {
                    sharedMaterial.SetTexture("_PackedSpecular", texture);
                }
            }
        }
    }
}
