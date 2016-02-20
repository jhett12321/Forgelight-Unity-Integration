using System;
using System.Collections.Generic;
using System.IO;
using Forgelight.Pack;

namespace Forgelight
{
    public class AssetManager
    {
        #region Singleton

        private static AssetManager instance = null;

        public static void CreateInstance()
        {
            instance = new AssetManager();
        }

        public static void DeleteInstance()
        {
            instance = null;
        }

        public static AssetManager Instance
        {
            get { return instance; }
        }

        #endregion

        public List<Pack.Pack> Packs { get; private set; }
        public Dictionary<Asset.Types, List<Asset>> AssetsByType { get; private set; }

        // Internal cache to check whether a pack has already been loaded
        private Dictionary<Int32, Pack.Pack> packLookupCache = new Dictionary<Int32, Pack.Pack>();

        private AssetManager()
        {
            Packs = new List<Pack.Pack>();
            AssetsByType = new Dictionary<Asset.Types, List<Asset>>();
        }

        public void LoadPackFile(string path)
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
    }
}