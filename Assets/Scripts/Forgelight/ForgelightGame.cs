using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Forgelight.Formats.Dma;
using Forgelight.Formats.Dme;
using Forgelight.Pack;
using UnityEditor;
using MathUtils = Forgelight.Utils.MathUtils;

namespace Forgelight
{
    public class ForgelightGame
    {
        //Info
        public string Alias { get; private set; }
        public string PackDirectory { get; private set; }
        public string ResourceDirectory { get; private set; }

        //Available Assets
        public List<string> AvailableActors { get; private set; }
        public List<string> AvailableZones { get; private set; }

        //Data
        public List<Pack.Pack> Packs { get; private set; }
        public Dictionary<Asset.Types, List<Asset>> AssetsByType { get; private set; }
        public MaterialDefinitionManager MaterialDefinitionManager { get; private set; }

        // Internal cache to check whether a pack has already been loaded
        private Dictionary<Int32, Pack.Pack> packLookupCache = new Dictionary<Int32, Pack.Pack>();

        public ForgelightGame(string alias, string packDirectory, string resourceDirectory)
        {
            Alias = alias;
            PackDirectory = packDirectory;
            ResourceDirectory = resourceDirectory;

            AvailableActors = new List<string>();
            AvailableZones = new List<string>();

            Packs = new List<Pack.Pack>();
            AssetsByType = new Dictionary<Asset.Types, List<Asset>>();
        }

        public void LoadPack(string path)
        {
            Pack.Pack pack = null;

            if (packLookupCache.TryGetValue(path.GetHashCode(), out pack) == false)
            {
                pack = Pack.Pack.LoadBinary(path);

                if (pack != null)
                {
                    packLookupCache.Add(path.GetHashCode(), pack);
                    Packs.Add(pack);

                    foreach (Asset asset in pack.Assets)
                    {
                        if (false == AssetsByType.ContainsKey(asset.Type))
                        {
                            AssetsByType.Add(asset.Type, new List<Asset>());
                        }

                        AssetsByType[asset.Type].Add(asset);
                    }
                }
            }
        }

        public MemoryStream CreateAssetMemoryStreamByName(String name)
        {
            MemoryStream memoryStream = null;

            foreach (Pack.Pack pack in Packs)
            {
                memoryStream = pack.CreateAssetMemoryStreamByName(name);

                if (memoryStream != null)
                {
                    break;
                }
            }

            return memoryStream;
        }

        private void ProgressBar(float progress, string currentTask)
        {
            EditorUtility.DisplayProgressBar("Forgelight Game - " + Alias, currentTask, progress);
        }

        public void OnLoadComplete()
        {
            EditorUtility.ClearProgressBar();
        }

        public void LoadPackFiles(float progress0, float progress100)
        {
            String[] files = Directory.GetFiles(PackDirectory, "*.pack");

            //Load Pack files into AssetManager.
            ProgressBar(progress0, "Loading Pack Data...");

            for (int i = 0; i < files.Length; ++i)
            {
                ProgressBar(MathUtils.Remap((float)i / (float)files.Length, 0.0f, 1.0f, progress0, progress100), "Loading Pack File: " + Path.GetFileName(files[i]));
                LoadPack(files[i]);
            }
        }

        public void InitializeMaterialDefinitionManager()
        {
            MaterialDefinitionManager = new MaterialDefinitionManager(this);
        }

        public void UpdateActors(float progress0, float progress100)
        {
            AvailableActors.Clear();
            ProgressBar(progress0, "Updating Actors List...");

            List<Asset> actors = AssetsByType[Asset.Types.ADR];

            for (int i = 0; i < actors.Count; ++i)
            {
                ProgressBar(MathUtils.Remap((float)i / (float)actors.Count, 0.0f, 1.0f, progress0, progress100), "Updating Actors List...");
                AvailableActors.Add(actors[i].Name);
            }

            AvailableActors.Sort();
        }

        //TODO Zone Listing
        public void UpdateZones(float progress0, float progress100)
        {

        }

        public void ExportModels(float progress0, float progress100)
        {
            ProgressBar(progress0, "Exporting Models...");

            List<Asset> modelAssets = AssetsByType[Asset.Types.DME];

            for (int i = 0; i < modelAssets.Count; ++i)
            {
                Asset asset = modelAssets[i];

                //Ignore auto-generated LOD's
                if (asset.Name.EndsWith("Auto.dme"))
                {
                    continue;
                }

                //Don't export if the file already exists.
                if (File.Exists(ResourceDirectory + "/Models/" + Path.GetFileNameWithoutExtension(asset.Name) + ".obj"))
                {
                    continue;
                }

                ProgressBar(MathUtils.Remap((float)i / (float)modelAssets.Count, 0.0f, 1.0f, progress0, progress100), "Exporting Model: " + Path.GetFileName(asset.Name));

                using (MemoryStream modelMemoryStream = asset.Pack.CreateAssetMemoryStreamByName(asset.Name))
                {
                    Model model = Model.LoadFromStream(asset.Name, modelMemoryStream);

                    if (model != null)
                    {
                        ModelExporter.ExportModel(this, model, ResourceDirectory + "/Models");
                    }
                }
            }
        }

        //TODO Terrain Exporting
        public void ExportTerrain(float progress0, float progress100)
        {
            List<Asset> terrainAssets = AssetsByType[Asset.Types.CNK0];
        }
    }
}
