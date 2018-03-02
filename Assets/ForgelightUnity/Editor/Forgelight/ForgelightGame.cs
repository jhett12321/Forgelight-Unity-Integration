namespace ForgelightUnity.Editor.Forgelight
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using Assets;
    using Assets.Adr;
    using Assets.Areas;
    using Assets.Dma;
    using Assets.Pack;
    using Assets.Zone;
    using Importers;
    using UnityEditor;
    using UnityEngine;
    using Utils;
    using MathUtils = Utils.MathUtils;
    using ModelImporter = Importers.ModelImporter;

    public class ForgelightGame
    {
        //Info
        public ForgelightGameInfo GameInfo;

        //Available Assets
        public List<Asset> AvailableActors { get; private set; }
        public List<Asset> AvailableZones { get; private set; }
        public List<Asset> AvailableAreaDefinitions { get; private set; }

        // Importers
        private ModelImporter modelImporter = new ModelImporter();
        private TerrainLODImporter terrainLodImporter = new TerrainLODImporter();

        //Data
        public List<Pack> Packs { get; private set; }
        public ConcurrentDictionary<AssetType, List<AssetRef>> AssetsByType { get; private set; }
        public MaterialDefinitionManager MaterialDefinitionManager { get; private set; }

        // Internal cache to check whether a pack has already been loaded
        private ConcurrentDictionary<string, Pack> packLookupCache = new ConcurrentDictionary<string, Pack>();

        //Progress
        private float lastProgress;

        #region Constructor
        public ForgelightGame(ForgelightGameInfo gameInfo)
        {
            this.GameInfo = gameInfo;
            Packs = new List<Pack>();
            AssetsByType = new ConcurrentDictionary<AssetType, List<AssetRef>>();

            foreach (Enum type in Enum.GetValues(typeof(AssetType)))
            {
                AssetsByType.TryAdd((AssetType) type, new List<AssetRef>());
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
            string[] files = Directory.GetFiles(GameInfo.PackDirectory, "*.pack");

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
            Pack pack;

            if (!packLookupCache.TryGetValue(path, out pack))
            {
                pack = Pack.LoadBinary(path);

                if (pack != null)
                {
                    packLookupCache.TryAdd(path, pack);
                    Packs.Add(pack);

                    foreach (AssetRef asset in pack.Assets)
                    {
                        AssetsByType[asset.AssetType].Add(asset);
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

            foreach (Pack pack in Packs)
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
            modelImporter.RunImport(this, progress0, progress100);
        }

        public void ImportTerrain(float progress0, float progress100)
        {
            terrainLodImporter.RunImport(this, progress0, progress100);
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

            List<AssetRef> actors = AssetsByType[AssetType.ADR];
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

            List<AssetRef> zones = AssetsByType[AssetType.ZONE];
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

            List<AssetRef> xmlFiles = AssetsByType[AssetType.XML];

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

        public void ProgressBar(float progress, string currentTask)
        {
            if (progress == lastProgress)
            {
                return;
            }

            EditorUtility.DisplayProgressBar("Forgelight - " + GameInfo.Name, currentTask, progress);
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
