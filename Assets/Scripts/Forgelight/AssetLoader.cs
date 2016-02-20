using System;
using System.Collections.Generic;
using System.IO;
using Forgelight.Formats.Dma;
using Forgelight.Formats.Dme;
using Forgelight.Pack;
using Forgelight.Utils;
using UnityEditor;
using UnityEngine;
using MathUtils = Forgelight.Utils.MathUtils;

namespace Forgelight
{
    public class AssetLoader
    {
        private bool running = false;

        public Dictionary<string, List<string>> availableActors = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> availableZones = new Dictionary<string, List<string>>();

        public void OpenAssetFolder()
        {
            string path = DialogUtils.OpenDirectory(
            "Select folder containing Forgelight game files.",
            "",
            "", CheckGivenAssetDirectory);

            LoadAssets(path);
        }

        private void LoadAssets(string path)
        {
            running = true;
            ProgressBar(0.0f, "Preparing...");

            string alias = Directory.GetParent(path).Parent.Name;
            string baseDir = Application.dataPath + "/Resources/" + alias;
            availableActors[alias] = new List<string>();
            availableZones[alias] = new List<string>();

            AssetManager.CreateInstance();

            String[] files = Directory.GetFiles(path, "*.pack");

            //Load Pack files into AssetManager.
            ProgressBar(0.0f, "Loading Pack Data...");

            for (int i = 0; i < files.Length; ++i)
            {
                ProgressBar(MathUtils.Remap((float)i / (float)files.Length, 0.0f, 1.0f, 0.0f, 0.25f), "Loading Pack File: " + Path.GetFileName(files[i]));
                AssetManager.Instance.LoadPackFile(files[i]);
            }

            //Locate materials XML and set up materialDefinition manager.
            ProgressBar(0.25f, "Initializing Materials...");
            MaterialDefinitionManager.CreateInstance();

            //Export Models
            ExportModels(baseDir, alias);

            //TODO Export Terrain
            ExportTerrain(baseDir, alias);

            //TODO Update State File
            OnLoadComplete();
            running = false;
        }

        private void ExportModels(string basePath, string alias)
        {
            ProgressBar(0.3f, "Exporting Models...");

            List<Asset> modelAssets = AssetManager.Instance.AssetsByType[Asset.Types.DME];
            List<string> aliasAvailableActors = availableActors[alias];

            for (int i = 0; i < modelAssets.Count; ++i)
            {
                Asset asset = modelAssets[i];

                //Ignore auto-generated LOD's
                if (asset.Name.EndsWith("Auto.dme"))
                {
                    continue;
                }

                //We add the model to our available actors list even if we are unable to export. We can still display them as pink cubes.
                aliasAvailableActors.Add(Path.GetFileNameWithoutExtension(asset.Name));

                ProgressBar(MathUtils.Remap((float)i / (float)modelAssets.Count, 0.0f, 1.0f, 0.3f, 0.5f), "Exporting Model: " + Path.GetFileName(asset.Name));

                using (MemoryStream modelMemoryStream = asset.Pack.CreateAssetMemoryStreamByName(asset.Name))
                {
                    Model model = Model.LoadFromStream(asset.Name, modelMemoryStream);

                    if (model != null)
                    {
                        ModelExporter.ExportModel(model, basePath + "/Models");
                    }
                }
            }

            //TODO Save available actors to state file.
        }

        private void ExportTerrain(string basePath, string alias)
        {
            List<Asset> terrainAssets = AssetManager.Instance.AssetsByType[Asset.Types.CNK0];


        }

        public void OnLoadComplete()
        {
            EditorUtility.ClearProgressBar();
        }

        private void ProgressBar(float progress, string currentTask)
        {
            if (running)
            {
                EditorUtility.DisplayProgressBar("Exporting Pack Data", currentTask, progress);
            }
        }

        private static ValidationResult CheckGivenAssetDirectory(string path)
        {
            ValidationResult validationResult = new ValidationResult();

            path += "/Resources/Assets";

            string[] files = Directory.GetFiles(path);

            foreach (string fileName in files)
            {
                if (fileName.EndsWith(".pack"))
                {
                    validationResult.result = true;
                    validationResult.path = path;
                    return validationResult;
                }
            }

            validationResult.result = false;
            validationResult.errorTitle = "Invalid Asset Directory";
            validationResult.errorDesc = "The directory provided is not a valid Forgelight game. Please make sure to select the root game directory (not the asset folder) and try again.";

            return validationResult;
        }
    }
}