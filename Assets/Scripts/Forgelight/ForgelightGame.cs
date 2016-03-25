using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Forgelight.Formats.Cnk;
using Forgelight.Formats.Dma;
using Forgelight.Formats.Dme;
using Forgelight.Formats.Zone;
using Forgelight.Pack;
using Forgelight.Utils;
using UnityEditor;
using Debug = UnityEngine.Debug;
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
        public Dictionary<string, Zone> AvailableZones { get; private set; }

        //Data
        public List<Pack.Pack> Packs { get; private set; }
        public ConcurrentDictionary<Asset.Types, List<Asset>> AssetsByType { get; private set; }
        public MaterialDefinitionManager MaterialDefinitionManager { get; private set; }

        // Internal cache to check whether a pack has already been loaded
        private ConcurrentDictionary<int, Pack.Pack> packLookupCache = new ConcurrentDictionary<int, Pack.Pack>();

        //Progress
        private float lastProgress = 0.0f;

        public ForgelightGame(string name, string packDirectory, string resourceDirectory)
        {
            Name = name;
            PackDirectory = packDirectory;
            ResourceDirectory = resourceDirectory;

            Packs = new List<Pack.Pack>();
            AssetsByType = new ConcurrentDictionary<Asset.Types, List<Asset>>();
        }

        public void LoadPack(string path)
        {
            Pack.Pack pack;

            if (!packLookupCache.TryGetValue(path.GetHashCode(), out pack))
            {
                pack = Pack.Pack.LoadBinary(path);

                if (pack != null)
                {
                    packLookupCache.TryAdd(path.GetHashCode(), pack);
                    Packs.Add(pack);

                    foreach (Asset asset in pack.Assets)
                    {
                        if (!AssetsByType.ContainsKey(asset.Type))
                        {
                            AssetsByType.TryAdd(asset.Type, new List<Asset>());
                        }

                        AssetsByType[asset.Type].Add(asset);
                    }
                }
            }
        }

        public bool LoadZoneFromFile(string path)
        {
            try
            {
                using (FileStream fileStream = File.OpenRead(path))
                {
                    string zoneName = Path.GetFileNameWithoutExtension(path);
                    Zone zone = Zone.LoadFromStream(Path.GetFileName(path), fileStream);

                    zoneName = zoneName + " (" + path + ")";
                    AvailableZones[zoneName] = zone;

                    if (DialogUtils.DisplayCancelableDialog("Change Zones", "Would you like to load this zone now?"))
                    {
                        ForgelightExtension.Instance.ZoneManager.ChangeZone(ForgelightExtension.Instance.ForgelightGameFactory.ActiveForgelightGame, zone);
                    }

                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("An error occurred while importing zone at: " + path + ". " + e.Message);
            }

            return false;
        }

        public MemoryStream CreateAssetMemoryStreamByName(string name)
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
            if (progress != lastProgress)
            {
                EditorUtility.DisplayProgressBar("Forgelight - " + Name, currentTask, progress);
                lastProgress = progress;
            }
        }

        public void OnLoadComplete()
        {
            EditorUtility.ClearProgressBar();
        }

        public void LoadPackFiles(float progress0, float progress100)
        {
            string[] files = Directory.GetFiles(PackDirectory, "*.pack");

            //Load Pack files into AssetManager.
            ProgressBar(progress0, "Loading Pack Data...");

            int packsProcessed = 0;
            string assetProcessing = "";

            BackgroundWorker backgroundWorker = Parallel.AsyncForEach(false, files, file =>
            {
                assetProcessing = Path.GetFileName(file);

                LoadPack(file);
                Interlocked.Increment(ref packsProcessed);
            });

            while (backgroundWorker.IsBusy)
            {
                ProgressBar(MathUtils.RemapProgress(packsProcessed / (float)files.Length, progress0, progress100), "Loading Pack File: " + assetProcessing);
            }

            backgroundWorker.Dispose();
        }

        public void InitializeMaterialDefinitionManager()
        {
            MaterialDefinitionManager = new MaterialDefinitionManager(this);
        }

        public void UpdateActors(float progress0, float progress100)
        {
            ProgressBar(progress0, "Updating Actors List...");

            List<Asset> actors = AssetsByType[Asset.Types.ADR];
            AvailableActors = new List<string>(actors.Count);

            int actorsProcessed = 0;
            string assetProcessing = "";
            object listLock = new object();

            BackgroundWorker backgroundWorker = Parallel.AsyncForEach(false, actors, asset =>
            {
                assetProcessing = asset.Name;

                lock (listLock)
                {
                    AvailableActors.Add(asset.Name);
                }

                Interlocked.Increment(ref actorsProcessed);
            });

            while (backgroundWorker.IsBusy)
            {
                ProgressBar(MathUtils.RemapProgress(actorsProcessed / (float)actors.Count, progress0, progress100), "Updating Actors List: " + assetProcessing);
            }

            backgroundWorker.Dispose();
            AvailableActors.Sort();
        }

        public void UpdateZones(float progress0, float progress100)
        {
            ProgressBar(progress0, "Updating Zones...");

            List<Asset> zones = AssetsByType[Asset.Types.ZONE];
            AvailableZones = new Dictionary<string, Zone>(zones.Count);

            int assetsProcessed = 0;
            string lastAssetProcessed = "";
            object listLock = new object();

            BackgroundWorker backgroundWorker = Parallel.AsyncForEach(false, zones, asset =>
            {
                string zoneName = Path.GetFileNameWithoutExtension(asset.Name);

                lastAssetProcessed = zoneName;

                if (zoneName == null)
                {
                    return;
                }

                MemoryStream memoryStream = asset.Pack.CreateAssetMemoryStreamByName(asset.Name);
                Zone zone = Zone.LoadFromStream(asset.Name, memoryStream);

                lock (listLock)
                {
                    zoneName = zoneName + " (" + asset.Pack.Name + ")";
                    AvailableZones[zoneName] = zone;
                }
            });

            while (backgroundWorker.IsBusy)
            {
                ProgressBar(MathUtils.RemapProgress(assetsProcessed / (float)zones.Count, progress0, progress100), "Updating Zone: " + lastAssetProcessed);
            }
        }

        public void ExportModels(float progress0, float progress100)
        {
            ProgressBar(progress0, "Exporting Models...");

            List<Asset> modelAssets = AssetsByType[Asset.Types.DME];
            int assetsProcessed = 0;
            string lastAssetProcessed = "";

            BackgroundWorker backgroundWorker = Parallel.AsyncForEach(false, modelAssets, asset =>
            {
                //Ignore auto-generated LOD's and Don't export if the file already exists.
                if (!asset.Name.EndsWith("Auto.dme") && !File.Exists(ResourceDirectory + "/Models/" + Path.GetFileNameWithoutExtension(asset.Name) + ".obj"))
                {
                    lastAssetProcessed = asset.Name;

                    using (MemoryStream modelMemoryStream = asset.Pack.CreateAssetMemoryStreamByName(asset.Name))
                    {
                        Model model = Model.LoadFromStream(asset.Name, modelMemoryStream);

                        if (model != null)
                        {
                            ModelExporter.ExportModel(this, model, ResourceDirectory + "/Models");
                        }
                    }
                }

                Interlocked.Increment(ref assetsProcessed);
            });

            while (backgroundWorker.IsBusy)
            {
                ProgressBar(MathUtils.RemapProgress(assetsProcessed / (float)modelAssets.Count, progress0, progress100), "Exporting Model: " + lastAssetProcessed);
            }

            backgroundWorker.Dispose();
        }

        //TODO Less Code Duplication.
        //TODO Update CNK0 Parsing. The current format seems to be incorrect.
        public void ExportTerrain(float progress0, float progress100)
        {
            int chunksProcessed = 0;
            int texturesProcessed = 0;
            string assetProcessing = "";

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
            //    //ProgressBar(MathUtils.RemapProgress((float)terrainAssetsCnk0Processed / (float)terrainAssetsCnk0.Count, progress0, progress100), "Exporting Chunk (LOD0): " + Path.GetFileName(asset.Name));
            //});

            //CNK1 (Geo)
            List<Asset> terrainAssetsCnk1 = AssetsByType[Asset.Types.CNK1];

            BackgroundWorker backgroundWorker = Parallel.AsyncForEach(false, terrainAssetsCnk1, asset =>
            {
                using (MemoryStream terrainMemoryStream = asset.Pack.CreateAssetMemoryStreamByName(asset.Name))
                {
                    assetProcessing = asset.Name;

                    CnkLOD chunk = CnkLOD.LoadFromStream(asset.Name, terrainMemoryStream);

                    ChunkExporter.ExportChunk(this, chunk, ResourceDirectory + "/Terrain");

                    Interlocked.Increment(ref chunksProcessed);
                }
            });

            //CNK1 (Textures)
            foreach (Asset asset in terrainAssetsCnk1)
            {
                using (MemoryStream terrainMemoryStream = asset.Pack.CreateAssetMemoryStreamByName(asset.Name))
                {
                    CnkLOD chunk = CnkLOD.LoadFromStream(asset.Name, terrainMemoryStream);

                    ChunkExporter.ExportTextures(this, chunk, ResourceDirectory + "/Terrain");

                    texturesProcessed++;

                    assetProcessing = chunk.Name;
                    ProgressBar(MathUtils.RemapProgress(texturesProcessed + chunksProcessed / (float)terrainAssetsCnk1.Count * 2, progress0, progress100), "Exporting Chunk: " + assetProcessing);
                }
            }

            while (backgroundWorker.IsBusy)
            {
                ProgressBar(MathUtils.RemapProgress(texturesProcessed + chunksProcessed / (float)terrainAssetsCnk1.Count * 2, progress0, progress100), "Exporting Chunk: " + assetProcessing);
            }

            backgroundWorker.Dispose();

            ////CNK2
            //ProgressBar(progress0 + MathUtils.RemapProgress(0.50f, progress0, progress100), "Exporting Terrain Data (LOD 2)...");
            //List<Asset> terrainAssetsCnk2 = AssetsByType[Asset.Types.CNK2];
            //int terrainAssetsCnk2Processed = 0;

            //Parallel.ForEach(terrainAssetsCnk2, asset =>
            //{
            //    using (MemoryStream terrainMemoryStream = asset.Pack.CreateAssetMemoryStreamByName(asset.Name))
            //    {
            //        CnkLOD chunk = CnkLOD.LoadFromStream(asset.Name, terrainMemoryStream);
            //    }

            //    Interlocked.Increment(ref terrainAssetsCnk2Processed);
            //    //ProgressBar(MathUtils.RemapProgress((float)terrainAssetsCnk2Processed / (float)terrainAssetsCnk2.Count, progress0, progress100), "Exporting Chunk (LOD2): " + Path.GetFileName(asset.Name));
            //});

            ////CNK3
            //ProgressBar(progress0 + MathUtils.RemapProgress(0.75f, progress0, progress100), "Exporting Terrain Data (LOD 3)...");
            //List<Asset> terrainAssetsCnk3 = AssetsByType[Asset.Types.CNK3];
            //int terrainAssetsCnk3Processed = 0;

            //Parallel.ForEach(terrainAssetsCnk3, asset =>
            //{
            //    using (MemoryStream terrainMemoryStream = asset.Pack.CreateAssetMemoryStreamByName(asset.Name))
            //    {
            //        CnkLOD chunk = CnkLOD.LoadFromStream(asset.Name, terrainMemoryStream);
            //    }

            //    Interlocked.Increment(ref terrainAssetsCnk3Processed);
            //    //ProgressBar(MathUtils.RemapProgress((float)terrainAssetsCnk3Processed / (float)terrainAssetsCnk3.Count, progress0, progress100), "Exporting Chunk (LOD3): " + Path.GetFileName(asset.Name));
            //});
        }
    }
}
