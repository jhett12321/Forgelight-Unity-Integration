namespace ForgelightUnity.Forgelight.Assets.Pack
{
    using System.Collections.Generic;
    using System.IO;
    using Utils;

    public class Pack
    {
        public string Path { get; private set; }
        public List<AssetRef> Assets { get; private set; }

        public string Name
        {
            get { return System.IO.Path.GetFileName(Path); }
        }

        public Dictionary<string, AssetRef> assetLookupCache = new Dictionary<string, AssetRef>();

        private Pack(string path)
        {
            Path = path;
            Assets = new List<AssetRef>();
        }

        public static Pack LoadBinary(string path)
        {
            Pack pack = new Pack(path);

            using (FileStream fileStream = File.OpenRead(path))
            {
                BinaryReaderBigEndian binaryReader = new BinaryReaderBigEndian(fileStream);

                uint nextChunkAbsoluteOffset = 0;

                do
                {
                    fileStream.Seek(nextChunkAbsoluteOffset, SeekOrigin.Begin);

                    nextChunkAbsoluteOffset = binaryReader.ReadUInt32();
                    uint fileCount = binaryReader.ReadUInt32();

                    for (uint i = 0; i < fileCount; ++i)
                    {
                        AssetRef file = AssetRef.LoadBinary(pack, binaryReader.BaseStream);

                        pack.assetLookupCache[file.Name] = file;

                        pack.Assets.Add(file);
                    }
                } while (nextChunkAbsoluteOffset != 0);

                return pack;
            }
        }

        public MemoryStream CreateAssetMemoryStreamByName(string name)
        {
            AssetRef assetRef;

            if (!assetLookupCache.TryGetValue(name, out assetRef))
            {
                return null;
            }

            FileStream file = File.Open(assetRef.Pack.Path, FileMode.Open, FileAccess.Read, FileShare.Read);

            byte[] buffer = new byte[assetRef.Size];

            file.Seek(assetRef.AbsoluteOffset, SeekOrigin.Begin);
            file.Read(buffer, 0, (int) assetRef.Size);

            MemoryStream memoryStream = new MemoryStream(buffer);

            return memoryStream;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}