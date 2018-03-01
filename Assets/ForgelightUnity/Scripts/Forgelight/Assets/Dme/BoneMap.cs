namespace ForgelightUnity.Forgelight.Assets.Dme
{
    using System.IO;

    public class BoneMapEntry
    {
        #region Structure
        public ushort BoneIndex;
        public ushort GlobalIndex;
        #endregion

        public static BoneMapEntry LoadFromStream(Stream stream)
        {
            if (stream == null)
            {
                return null;
            }

            BinaryReader binaryReader = new BinaryReader(stream);

            BoneMapEntry boneMapEntry = new BoneMapEntry();

            boneMapEntry.BoneIndex = binaryReader.ReadUInt16();
            boneMapEntry.GlobalIndex = binaryReader.ReadUInt16();

            return boneMapEntry;
        }
    }

    public class BoneMap
    {
        #region Structure
        public uint Unknown0 { get; private set; }
        public uint BoneStart { get; private set; }
        public uint BoneCount { get; private set; }
        public uint Delta { get; private set; }
        public uint Unknown1 { get; private set; }
        public uint BoneEnd { get; private set; }
        public uint VertexCount { get; private set; }
        public uint Unknown2 { get; private set; }
        public uint IndexCount { get; private set; }
        #endregion

        public static BoneMap LoadFromStream(Stream stream)
        {
            if (stream == null)
            {
                return null;
            }

            BinaryReader binaryReader = new BinaryReader(stream);

            BoneMap boneMap = new BoneMap();

            boneMap.Unknown0 = binaryReader.ReadUInt32();
            boneMap.BoneStart = binaryReader.ReadUInt32();
            boneMap.BoneCount = binaryReader.ReadUInt32();
            boneMap.Delta = binaryReader.ReadUInt32();
            boneMap.Unknown1 = binaryReader.ReadUInt32();
            boneMap.BoneEnd = binaryReader.ReadUInt32();
            boneMap.VertexCount = binaryReader.ReadUInt32();
            boneMap.Unknown2 = binaryReader.ReadUInt32();
            boneMap.IndexCount = binaryReader.ReadUInt32();

            return boneMap;
        }
    }
}