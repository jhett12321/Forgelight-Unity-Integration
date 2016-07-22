using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Forgelight.Assets;
using Forgelight.Assets.Adr;
using Forgelight.Assets.Areas;
using Forgelight.Assets.Cnk;
using Forgelight.Assets.Dma;
using Forgelight.Assets.Dme;
using Forgelight.Assets.Zone;
using Forgelight.Pack;
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
        public List<Asset> AvailableActors { get; private set; }
        public List<Asset> AvailableZones { get; private set; }
        public List<Asset> AvailableAreaDefinitions { get; private set; }

        //Data
        public List<Pack.Pack> Packs { get; private set; }
        public ConcurrentDictionary<AssetRef.Types, List<AssetRef>> AssetsByType { get; private set; }
        public MaterialDefinitionManager MaterialDefinitionManager { get; private set; }

        // Internal cache to check whether a pack has already been loaded
        private ConcurrentDictionary<string, Pack.Pack> packLookupCache = new ConcurrentDictionary<string, Pack.Pack>();

        //Progress
        private float lastProgress;

        #region Constructor
        public ForgelightGame(string name, string packDirectory, string resourceDirectory)
        {
            Name = name;
            PackDirectory = packDirectory;
            ResourceDirectory = resourceDirectory;

            Packs = new List<Pack.Pack>();
            AssetsByType = new ConcurrentDictionary<AssetRef.Types, List<AssetRef>>();

            foreach (Enum type in Enum.GetValues(typeof(AssetRef.Types)))
            {
                AssetsByType.TryAdd((AssetRef.Types) type, new List<AssetRef>());
            }
        }
        #endregion

        #region Asset Getters
        public Adr GetActorDefinition(string name)
        {
            return (Adr) AvailableActors.Find(actor => actor.Name == name);
        }

        public Zone GetZone(string name)
        {
            return (Zone) AvailableZones.Find(zone => zone.Name == name);
        }

        public Areas GetAreaDefinitions(string name)
        {
            return (Areas) AvailableAreaDefinitions.Find(areas => areas.Name == name);
        }
        #endregion

        #region Pack Operations
        public void LoadPackFiles(float progress0, float progress100)
        {
            string[] files = Directory.GetFiles(PackDirectory, "*.pack");

            //Load Pack files into AssetManager.
            ProgressBar(progress0, "Loading Pack Data...");

            int packsProcessed = 0;
            string assetProcessing = "";

            Parallel.AsyncForEach<string> parallelTask = System.Threading.Tasks.Parallel.ForEach;

            IAsyncResult result = parallelTask.BeginInvoke(files, file =>
            {
                assetProcessing = Path.GetFileName(file);

                LoadPack(file);
                Interlocked.Increment(ref packsProcessed);
            }, null, null);

            while (!result.IsCompleted)
            {
                ProgressBar(MathUtils.Remap01(packsProcessed / (float)files.Length, progress0, progress100), "Loading Pack File: " + assetProcessing);
            }

            parallelTask.EndInvoke(result);
        }

        public void LoadPack(string path)
        {
            Pack.Pack pack;

            if (!packLookupCache.TryGetValue(path, out pack))
            {
                pack = Pack.Pack.LoadBinary(path);

                if (pack != null)
                {
                    packLookupCache.TryAdd(path, pack);
                    Packs.Add(pack);

                    foreach (AssetRef asset in pack.Assets)
                    {
                        AssetsByType[asset.Type].Add(asset);
                    }
                }
            }
        }

        public MemoryStream CreateAssetMemoryStreamByName(string name)
        {
            MemoryStream memoryStream = null;

            if (name == null)
            {
                Debug.LogError("Asset Name is null");
            }

            foreach (Pack.Pack pack in Packs)
            {
                if (pack == null)
                {
                    Debug.LogError("Pack is null");
                }

                memoryStream = pack.CreateAssetMemoryStreamByName(name);

                if (memoryStream != null)
                {
                    break;
                }
            }

            return memoryStream;
        }
        #endregion

        #region Asset Import
        public void ImportModels(float progress0, float progress100)
        {
            ProgressBar(progress0, "Exporting Models...");

            List<AssetRef> modelAssets = AssetsByType[AssetRef.Types.DME];
            int assetsProcessed = 0;
            string lastAssetProcessed = "";

            Parallel.AsyncForEach<AssetRef> parallelTask = System.Threading.Tasks.Parallel.ForEach;

            IAsyncResult result = parallelTask.BeginInvoke(modelAssets, asset =>
            {
                Interlocked.Increment(ref assetsProcessed);

                if (asset == null)
                {
                    return;
                }

                //Don't export if the file already exists.
                if (File.Exists(ResourceDirectory + "/Models/" + Path.GetFileNameWithoutExtension(asset.Name) + ".obj"))
                {
                    return;
                }

                //Names
                string assetName = asset.Name;
                string assetDisplayName = BuildAssetName(assetName, asset.Pack.Name);

                //De-serialize
                using (MemoryStream modelMemoryStream = asset.Pack.CreateAssetMemoryStreamByName(assetName))
                {
                    Model model = Model.LoadFromStream(assetName, assetDisplayName, modelMemoryStream);

                    if (model == null)
                    {
                        return;
                    }

                    ModelExporter.ExportModel(this, model, ResourceDirectory + "/Models");
                    lastAssetProcessed = assetName;
                }
            }, null, null);

            while (!result.IsCompleted)
            {
                ProgressBar(MathUtils.Remap01(assetsProcessed / (float)modelAssets.Count, progress0, progress100), "Exporting Model: " + lastAssetProcessed);
            }

            parallelTask.EndInvoke(result);
        }

        //TODO Less Code Duplication.
        //TODO Update CNK0 Parsing. The current format seems to be incorrect.
        public void ImportTerrain(float progress0, float progress100)
        {
            int chunksProcessed = 0;
            int texturesProcessed = 0;
            string assetProcessing = "";

            //CNK0 (Geo)
            //List<AssetRef> terrainAssetsCnk0 = AssetsByType[AssetRef.Types.CNK0];

            //Parallel.AsyncForEach<AssetRef> parallelTask = System.Threading.Tasks.Parallel.ForEach;
            //IAsyncResult result = parallelTask.BeginInvoke(terrainAssetsCnk0, asset =>
            //{
            //    if (asset == null)
            //    {
            //        return;
            //    }

            //    //Names
            //    string assetName = asset.Name;
            //    string assetDisplayName = BuildAssetName(assetName, asset.Pack.Name);

            //    //De-serialize
            //    using (MemoryStream terrainMemoryStream = asset.Pack.CreateAssetMemoryStreamByName(asset.Name))
            //    {
            //        Cnk0 chunk = Cnk0.LoadFromStream(assetName, assetDisplayName, terrainMemoryStream);

            //        if (chunk != null)
            //        {
            //            ChunkExporter.ExportChunk(this, chunk, ResourceDirectory + "/Terrain");
            //            assetProcessing = assetName;
            //        }

            //        Interlocked.Increment(ref chunksProcessed);
            //    }
            //}, null, null);

            //while (!result.IsCompleted)
            //{
            //    ProgressBar(MathUtils.Remap01(chunksProcessed / ((float)terrainAssetsCnk0.Count), progress0, progress100), "Exporting Chunk: " + assetProcessing);
            //}

            //chunksProcessed = 0;
            //texturesProcessed = 0;

            //CNK1 (Geo)
            List<AssetRef> terrainAssetsCnk1 = AssetsByType[AssetRef.Types.CNK1];

            Parallel.AsyncForEach<AssetRef> parallelTask = System.Threading.Tasks.Parallel.ForEach;
            IAsyncResult result = parallelTask.BeginInvoke(terrainAssetsCnk1, asset =>
            {
                Interlocked.Increment(ref chunksProcessed);

                if (asset == null)
                {
                    return;
                }

                //Names
                string assetName = asset.Name;
                string assetDisplayName = BuildAssetName(assetName, asset.Pack.Name);

                //De-serialize
                using (MemoryStream terrainMemoryStream = asset.Pack.CreateAssetMemoryStreamByName(assetName))
                {
                    CnkLOD chunk = CnkLOD.LoadFromStream(assetName, assetDisplayName, terrainMemoryStream);

                    if (chunk != null)
                    {
                        ChunkExporter.ExportChunk(this, chunk, ResourceDirectory + "/Terrain");
                    }

                    assetProcessing = assetName;
                }
            }, null, null);

            //CNK1 (Textures)
            foreach (AssetRef asset in terrainAssetsCnk1)
            {
                texturesProcessed++;

                //Names
                string assetName = asset.Name;
                string assetDisplayName = BuildAssetName(assetName, asset.Pack.Name);

                //De-serialize
                using (MemoryStream terrainMemoryStream = asset.Pack.CreateAssetMemoryStreamByName(assetName))
                {
                    CnkLOD chunk = CnkLOD.LoadFromStream(assetName, assetDisplayName, terrainMemoryStream);

                    if (chunk != null)
                    {
                        ChunkExporter.ExportTextures(this, chunk, ResourceDirectory + "/Terrain");
                    }

                    assetProcessing = assetName;

                    ProgressBar(MathUtils.Remap01((texturesProcessed + chunksProcessed) / ((float)terrainAssetsCnk1.Count * 2), progress0, progress100), "Exporting Chunk: " + assetProcessing);
                }
            }

            while (!result.IsCompleted)
            {
                ProgressBar(MathUtils.Remap01((texturesProcessed + chunksProcessed) / ((float)terrainAssetsCnk1.Count * 2), progress0, progress100), "Exporting Chunk: " + assetProcessing);
            }

            parallelTask.EndInvoke(result);

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
        #endregion

        #region Runtime Import
        public void InitializeMaterialDefinitionManager()
        {
            MaterialDefinitionManager = new MaterialDefinitionManager(this);
        }

        public void UpdateActors(float progress0, float progress100)
        {
            ProgressBar(progress0, "Updating Actors List...");

            List<AssetRef> actors = AssetsByType[AssetRef.Types.ADR];
            AvailableActors = new List<Asset>(actors.Count);

            int assetsProcessed = 0;
            string lastAssetProcessed = "";
            object listLock = new object();

            Parallel.AsyncForEach<AssetRef> parallelTask = System.Threading.Tasks.Parallel.ForEach;

            IAsyncResult result = parallelTask.BeginInvoke(actors, asset =>
            {
                Interlocked.Increment(ref assetsProcessed);

                if (asset == null)
                {
                    return;
                }

                //Names
                string assetName = asset.Name;
                string assetDisplayName = BuildAssetName(assetName, asset.Pack.Name);

                //De-serialize
                using (MemoryStream memoryStream = asset.Pack.CreateAssetMemoryStreamByName(assetName))
                {
                    Adr adr = Adr.LoadFromStream(assetName, assetDisplayName, memoryStream);

                    if (adr == null)
                    {
                        return;
                    }

                    lock (listLock)
                    {
                        AvailableActors.Add(adr);
                        lastAssetProcessed = assetName;
                    }
                }

            }, null, null);

            while (!result.IsCompleted)
            {
                ProgressBar(MathUtils.Remap01(assetsProcessed / (float)actors.Count, progress0, progress100), "Updating Actors List: " + lastAssetProcessed);
            }

            parallelTask.EndInvoke(result);
            AvailableActors.Sort();
        }

        public bool LoadZoneFromFile(string path)
        {
            try
            {
                using (FileStream fileStream = File.OpenRead(path))
                {
                    //Names
                    string assetName = Path.GetFileName(path);
                    string assetDisplayName = BuildAssetName(assetName, Path.GetDirectoryName(path));

                    //De-serialize
                    Zone zone = Zone.LoadFromStream(assetName, assetDisplayName, fileStream);

                    if (zone != null)
                    {
                        AvailableZones.Add(zone);
                        AvailableZones.Sort();

                        ForgelightExtension.Instance.ZoneManager.ChangeZone(ForgelightExtension.Instance.ForgelightGameFactory.ActiveForgelightGame, zone);

                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("An error occurred while importing zone at: " + path + ". " + e.Message);
            }

            return false;
        }

        public void UpdateZones(float progress0, float progress100)
        {
            ProgressBar(progress0, "Updating Zones...");

            List<AssetRef> zones = AssetsByType[AssetRef.Types.ZONE];
            AvailableZones = new List<Asset>(zones.Count);

            int assetsProcessed = 0;
            string lastAssetProcessed = "";
            object listLock = new object();

            Parallel.AsyncForEach<AssetRef> parallelTask = System.Threading.Tasks.Parallel.ForEach;

            IAsyncResult result = parallelTask.BeginInvoke(zones, asset =>
            {
                Interlocked.Increment(ref assetsProcessed);

                if (asset == null)
                {
                    return;
                }

                //Names
                string assetName = asset.Name;
                string assetDisplayName = BuildAssetName(assetName, asset.Pack.Name);

                //De-serialize
                using (MemoryStream memoryStream = asset.Pack.CreateAssetMemoryStreamByName(asset.Name))
                {
                    lastAssetProcessed = assetName;
                    Zone zone = Zone.LoadFromStream(assetName, assetDisplayName, memoryStream);

                    if (zone == null)
                    {
                        return;
                    }

                    lock (listLock)
                    {
                        AvailableZones.Add(zone);
                    }
                }
            }, null, null);

            while (!result.IsCompleted)
            {
                ProgressBar(MathUtils.Remap01(assetsProcessed / (float)zones.Count, progress0, progress100), "Updating Zone: " + lastAssetProcessed);
            }

            parallelTask.EndInvoke(result);
            AvailableZones.Sort();
        }

        public void UpdateAreas(float progress0, float progress100)
        {
            ProgressBar(progress0, "Loading Area Definitions");

            List<AssetRef> xmlFiles = AssetsByType[AssetRef.Types.XML];

            AvailableAreaDefinitions = new List<Asset>();

            int assetsProcessed = 0;
            string lastAssetProcessed = "";
            object listLock = new object();

            Parallel.AsyncForEach<AssetRef> parallelTask = System.Threading.Tasks.Parallel.ForEach;

            IAsyncResult result = parallelTask.BeginInvoke(xmlFiles, asset =>
            {
                Interlocked.Increment(ref assetsProcessed);

                if (asset == null)
                {
                    return;
                }

                //Names
                string assetName = asset.Name;
                string assetDisplayName = BuildAssetName(assetName, asset.Pack.Name);

                if (!assetName.EndsWith("Areas.xml"))
                {
                    return;
                }

                //De-serialize
                using (MemoryStream memoryStream = asset.Pack.CreateAssetMemoryStreamByName(assetName))
                {
                    Areas areas = Areas.LoadFromStream(assetName, assetDisplayName, memoryStream);

                    if (areas == null)
                    {
                        return;
                    }

                    lock (listLock)
                    {
                        AvailableAreaDefinitions.Add(areas);
                        lastAssetProcessed = assetName;
                    }
                }
            }, null, null);

            while (!result.IsCompleted)
            {
                ProgressBar(MathUtils.Remap01(assetsProcessed / (float)xmlFiles.Count, progress0, progress100), "Loading Area Definitions: " + lastAssetProcessed);
            }

            parallelTask.EndInvoke(result);
            AvailableAreaDefinitions.Sort();
        }
        #endregion

        #region Helpers
        private void ProgressBar(float progress, string currentTask)
        {
            if (progress == lastProgress)
            {
                return;
            }

            EditorUtility.DisplayProgressBar("Forgelight - " + Name, currentTask, progress);
            lastProgress = progress;
        }

        public void OnLoadComplete()
        {
            EditorUtility.ClearProgressBar();
        }

        public string BuildAssetName(string assetName, string packName)
        {
            return assetName + " (" + packName + ')';
        }
        #endregion
    }
}
