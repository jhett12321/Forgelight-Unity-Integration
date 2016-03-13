using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Forgelight.Utils;

namespace Forgelight.Pack
{
    public class Pack
    {
        [Description("The path on disk to this pack file.")]
        [ReadOnlyAttribute(true)]
        public string Path { get; private set; }

        [BrowsableAttribute(false)]
        public List<Asset> Assets { get; private set; }

        [BrowsableAttribute(false)]
        public String Name
        {
            get { return System.IO.Path.GetFileName(Path); }
        }

        public Dictionary<string, Asset> assetLookupCache = new Dictionary<string, Asset>();

        private Pack(String path)
        {
            Path = path;
            Assets = new List<Asset>();
        }

        public static Pack LoadBinary(string path)
        {
            Pack pack = new Pack(path);

            using (FileStream fileStream = File.OpenRead(path))
            {
                BinaryReaderBigEndian binaryReader = new BinaryReaderBigEndian(fileStream);

                UInt32 nextChunkAbsoluteOffset = 0;

                do
                {
                    fileStream.Seek(nextChunkAbsoluteOffset, SeekOrigin.Begin);

                    nextChunkAbsoluteOffset = binaryReader.ReadUInt32();
                    uint fileCount = binaryReader.ReadUInt32();

                    for (UInt32 i = 0; i < fileCount; ++i)
                    {
                        Asset file = Asset.LoadBinary(pack, binaryReader.BaseStream);

                        pack.assetLookupCache[file.Name] = file;

                        pack.Assets.Add(file);
                    }
                } while (nextChunkAbsoluteOffset != 0);

                return pack;
            }
        }

        public MemoryStream CreateAssetMemoryStreamByName(string name)
        {
            Asset asset = null;

            if (!assetLookupCache.TryGetValue(name, out asset))
            {
                return null;
            }

            FileStream file = File.Open(asset.Pack.Path, FileMode.Open, FileAccess.Read, FileShare.Read);

            byte[] buffer = new byte[asset.Size];

            file.Seek(asset.AbsoluteOffset, SeekOrigin.Begin);
            file.Read(buffer, 0, (Int32) asset.Size);

            MemoryStream memoryStream = new MemoryStream(buffer);

            return memoryStream;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}