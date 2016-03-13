using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Forgelight.Formats.Cnk;
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
        public string Name { get; private set; }
        public string PackDirectory { get; private set; }
        public string ResourceDirectory { get; private set; }

        //Available Assets
        public List<string> AvailableActors { get; private set; }
        public Dictionary<string, Formats.Zone.Zone> AvailableZones { get; private set; }

        //Data
        public List<Pack.Pack> Packs { get; private set; }
        public Dictionary<Asset.Types, List<Asset>> AssetsByType { get; private set; }
        public MaterialDefinitionManager MaterialDefinitionManager { get; private set; }

        // Internal cache to check whether a pack has already been loaded
        private Dictionary<Int32, Pack.Pack> packLookupCache = new Dictionary<Int32, Pack.Pack>();

        public ForgelightGame(string name, string packDirectory, string resourceDirectory)
        {
            Name = name;
            PackDirectory = packDirectory;
            ResourceDirectory = resourceDirectory;

            AvailableActors = new List<string>();
            AvailableZones = new Dictionary<string, Formats.Zone.Zone>();

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
                        if (!AssetsByType.ContainsKey(asset.Type))
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
            EditorUtility.DisplayProgressBar("Forgelight - " + Name, currentTask, progress);
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

        public void UpdateZones(float progress0, float progress100)
        {
            AvailableZones.Clear();
            ProgressBar(progress0, "Updating Zones...");

            List<Asset> zones = AssetsByType[Asset.Types.ZONE];

            for (int i = 0; i < zones.Count; ++i)
            {
                ProgressBar(MathUtils.Remap((float)i / (float)zones.Count, 0.0f, 1.0f, progress0, progress100), "Updating Zone: " + zones[i].Name);

                MemoryStream memoryStream = zones[i].Pack.CreateAssetMemoryStreamByName(zones[i].Name);
                Formats.Zone.Zone zone = Formats.Zone.Zone.LoadFromStream(zones[i].Name, memoryStream);

                string rawZoneName = Path.GetFileNameWithoutExtension(zones[i].Name);
                string zoneName = rawZoneName;

                if (AvailableZones.ContainsKey(zoneName))
                {
                    zoneName = rawZoneName +  " (" + zones[i].Pack.Name + ")";
                }

                //int j = 0;
                //while (AvailableZones.ContainsKey(zoneName))
                //{
                //    j++;
                //    zoneName = string.Format("{0} ({1})", rawZoneName, j);
                //}

                AvailableZones[zoneName] = zone;
            }
        }

        public void ExportModels(float progress0, float progress100)
        {
            ProgressBar(progress0, "Exporting Models...");

            List<Asset> modelAssets = AssetsByType[Asset.Types.DME];
            int modelsProcessed = 0;

            Parallel.ForEach(modelAssets, asset =>
            {
                //Ignore auto-generated LOD's
                if (asset.Name.EndsWith("Auto.dme"))
                {
                    return;
                }

                //Don't export if the file already exists.
                if (File.Exists(ResourceDirectory + "/Models/" + Path.GetFileNameWithoutExtension(asset.Name) + ".obj"))
                {
                    return;
                }

                using (MemoryStream modelMemoryStream = asset.Pack.CreateAssetMemoryStreamByName(asset.Name))
                {
                    Model model = Model.LoadFromStream(asset.Name, modelMemoryStream);

                    if (model != null)
                    {
                        ModelExporter.ExportModel(this, model, ResourceDirectory + "/Models");
                    }
                }

                Interlocked.Increment(ref modelsProcessed);
                //ProgressBar(MathUtils.Remap((float)modelsProcessed / (float)modelAssets.Count, 0.0f, 1.0f, progress0, progress100), "Exporting Model: " + Path.GetFileName(asset.Name));
            });
        }

        //TODO Less Code Duplication.
        //TODO Update CNK0 Parsing. The current format seems to be incorrect.
        //TODO Make Progress Bars more verbose.
        public void ExportTerrain(float progress0, float progress100)
        {
            //CNK0
            //ProgressBar(progress0, "Exporting Terrain Data (LOD 0)...");
            //List<Asset> terrainAssetsCnk0 = AssetsByType[Asset.Types.CNK0];
            //int terrainAssetsCnk0Processed = 0;

            //foreach (Asset asset in terrainAssetsCnk0)
            //{
            //    using (MemoryStream terrainMemoryStream = asset.Pack.CreateAssetMemoryStreamByName(asset.Name))
            //    {
            //        Cnk0 chunk = Cnk0.LoadFromStream(asset.Name, terrainMemoryStream);
            //    }
            //}

            //Parallel.ForEach(terrainAssetsCnk0, asset =>
            //{
            //    using (MemoryStream terrainMemoryStream = asset.Pack.CreateAssetMemoryStreamByName(asset.Name))
            //    {
            //        Cnk0 chunk = Cnk0.LoadFromStream(asset.Name, terrainMemoryStream);
            //    }

            //    Interlocked.Increment(ref terrainAssetsCnk0Processed);
            //    //ProgressBar(MathUtils.Remap((float)terrainAssetsCnk0Processed / (float)terrainAssetsCnk0.Count, 0.0f, 1.0f, progress0, progress100), "Exporting Chunk (LOD0): " + Path.GetFileName(asset.Name));
            //});

            //CNK1
            ProgressBar(progress0 + MathUtils.Remap(0.25f, 0.0f, 1.0f, progress0, progress100), "Exporting Terrain Data (LOD 1)...");
            List<Asset> terrainAssetsCnk1 = AssetsByType[Asset.Types.CNK1];
            int terrainAssetsCnk1Processed = 0;

            Parallel.ForEach(terrainAssetsCnk1, asset =>
            {
                using (MemoryStream terrainMemoryStream = asset.Pack.CreateAssetMemoryStreamByName(asset.Name))
                {
                    CnkLOD chunk = CnkLOD.LoadFromStream(asset.Name, terrainMemoryStream);
                }

                Interlocked.Increment(ref terrainAssetsCnk1Processed);
                //ProgressBar(MathUtils.Remap((float)terrainAssetsCnk1Processed / (float)terrainAssetsCnk1.Count, 0.0f, 1.0f, progress0, progress100), "Exporting Chunk (LOD1): " + Path.GetFileName(asset.Name));
            });

            //CNK2
            ProgressBar(progress0 + MathUtils.Remap(0.50f, 0.0f, 1.0f, progress0, progress100), "Exporting Terrain Data (LOD 2)...");
            List<Asset> terrainAssetsCnk2 = AssetsByType[Asset.Types.CNK2];
            int terrainAssetsCnk2Processed = 0;

            Parallel.ForEach(terrainAssetsCnk2, asset =>
            {
                using (MemoryStream terrainMemoryStream = asset.Pack.CreateAssetMemoryStreamByName(asset.Name))
                {
                    CnkLOD chunk = CnkLOD.LoadFromStream(asset.Name, terrainMemoryStream);
                }

                Interlocked.Increment(ref terrainAssetsCnk2Processed);
                //ProgressBar(MathUtils.Remap((float)terrainAssetsCnk2Processed / (float)terrainAssetsCnk2.Count, 0.0f, 1.0f, progress0, progress100), "Exporting Chunk (LOD2): " + Path.GetFileName(asset.Name));
            });

            //CNK3
            ProgressBar(progress0 + MathUtils.Remap(0.75f, 0.0f, 1.0f, progress0, progress100), "Exporting Terrain Data (LOD 3)...");
            List<Asset> terrainAssetsCnk3 = AssetsByType[Asset.Types.CNK3];
            int terrainAssetsCnk3Processed = 0;

            Parallel.ForEach(terrainAssetsCnk3, asset =>
            {
                using (MemoryStream terrainMemoryStream = asset.Pack.CreateAssetMemoryStreamByName(asset.Name))
                {
                    CnkLOD chunk = CnkLOD.LoadFromStream(asset.Name, terrainMemoryStream);
                }

                Interlocked.Increment(ref terrainAssetsCnk3Processed);
                //ProgressBar(MathUtils.Remap((float)terrainAssetsCnk3Processed / (float)terrainAssetsCnk3.Count, 0.0f, 1.0f, progress0, progress100), "Exporting Chunk (LOD3): " + Path.GetFileName(asset.Name));
            });
        }
    }
}
