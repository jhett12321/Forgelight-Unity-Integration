using System.IO;
using UnityEditor;
using UnityEngine;

public class ObjectImportSettingsOverride : AssetPostprocessor
{
    public void OnPostprocessModel(GameObject gameObject)
    {
        foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>())
        {
            Material material = renderer.sharedMaterial;
            renderer.material.shader = Shader.Find("Standard (Specular setup)");

            if (material.GetFloat("_Mode") != 1.0f)
            {
                material.SetFloat("_Mode", 1.0f);
            }

            if (material.GetFloat("_Glossiness") != 0.3f)
            {
                material.SetFloat("_Glossiness", 0.3f);
            }
        }
    }
}
