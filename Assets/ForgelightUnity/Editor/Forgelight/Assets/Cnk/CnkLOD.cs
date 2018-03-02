namespace ForgelightUnity.Editor.Forgelight.Assets.Cnk
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using LzhamWrapper;
    using UnityEngine;
    using Utils;

    public class CnkLOD : Asset, IPoolable
    {
        public override string Name { get; protected set; }
        public override string DisplayName { get; protected set; }
        public ChunkType ChunkType { get; private set; }

        #region Structure
        //Header
        public uint Version { get; private set; }

        public uint DecompressedSize { get; private set; }
        public uint CompressedSize { get; private set; }

        // Buffers
        public byte[] CompressedBuffer = new byte[0];
        public byte[] DecompressedBuffer = new byte[0];

        //Textures
        public List<Texture> Textures { get; private set; }
        public class Texture
        {
            public List<byte> ColorNXMap { get; set; }
            public List<byte> SpecNyMap { get; set; }
            public List<byte> ExtraData1 { get; set; }
            public List<byte> ExtraData2 { get; set; }
            public List<byte> ExtraData3 { get; set; }
            public List<byte> ExtraData4 { get; set; }
        }

        //Verts per side
        public uint VertsPerSide { get; private set; }

        public Dictionary<int, Dictionary<int, HeightMap>> HeightMaps = new Dictionary<int, Dictionary<int, HeightMap>>();
        public class HeightMap
        {
            public short Val1 { get; set; }
            public byte Val2 { get; set; }
            public byte Val3 { get; set; }
        }

        //Indices
        public List<ushort> Indices { get; private set; }

        //Verts
        public List<Vertex> Vertices { get; private set; }
        public class Vertex
        {
            public short X { get; set; }
            public short Y { get; set; }
            public short HeightFar { get; set; }
            public short HeightNear { get; set; }
            public uint Color { get; set; }
        }

        //Render Batches
        public List<RenderBatch> RenderBatches { get; private set; }
        public class RenderBatch
        {
            public uint Unknown { get; set; }
            public uint IndexOffset { get; set; }
            public uint IndexCount { get; set; }
            public uint VertexOffset { get; set; }
            public uint VertexCount { get; set; }
        }

        //Optimized Draw
        public List<OptimizedDraw> OptimizedDraws { get; private set; }
        public class OptimizedDraw
        {
            public List<byte> Data { get; set; }
        }

        //Unknown Data
        public List<ushort> UnknownShorts1 { get; private set; }

        //Unknown Data
        public List<Vector3> UnknownVectors1 { get; private set; }

        //Tile Occluder Info
        public List<TileOccluderInfo> TileOccluderInfos { get; private set; }
        public class TileOccluderInfo
        {
            public List<byte> Data { get; set; }
        }
        #endregion

        public CnkLOD()
        {
            Textures = new List<Texture>();
            Indices = new List<ushort>();
            Vertices = new List<Vertex>();
            RenderBatches = new List<RenderBatch>();
            OptimizedDraws = new List<OptimizedDraw>();
            UnknownShorts1 = new List<ushort>();
            UnknownVectors1 = new List<Vector3>();
            TileOccluderInfos = new List<TileOccluderInfo>();
        }

        public void Reset()
        {
            Textures.Clear();
            Indices.Clear();
            Vertices.Clear();
            RenderBatches.Clear();
            OptimizedDraws.Clear();
            UnknownShorts1.Clear();
            UnknownVectors1.Clear();
            TileOccluderInfos.Clear();
        }

        public bool InitializeFromStream(string name, string displayName, MemoryStream stream)
        {
            using (BinaryReader binaryReader = new BinaryReader(stream))
            {
                Name = name;
                DisplayName = displayName;

                //Header
                byte[] magic = binaryReader.ReadBytes(4);

                if (magic[0] != 'C' ||
                    magic[1] != 'N' ||
                    magic[2] != 'K' /* ||
                    magic[3] != '1'*/)
                {
                    return false;
                }

                Version = binaryReader.ReadUInt32();

                if (!Enum.IsDefined(typeof(ChunkType), (int) Version))
                {
                    Debug.LogWarning("Could not decode chunk " + name + ". Unknown cnk version " + Version);
                    return false;
                }

                ChunkType = (ChunkType) Version;

                DecompressedSize = binaryReader.ReadUInt32();
                CompressedSize = binaryReader.ReadUInt32();

                // Decompression
                // Make sure our buffers are large enough.
                if (CompressedBuffer.Length < CompressedSize)
                {
                    Array.Resize(ref CompressedBuffer, (int) CompressedSize);
                }

                if (DecompressedBuffer.Length < DecompressedSize)
                {
                    Array.Resize(ref DecompressedBuffer, (int) DecompressedSize);
                }

                // Read the compressed buffer.
                binaryReader.Read(CompressedBuffer, 0, (int) CompressedSize);

                // Perform decompression using Lzham.
                InflateReturnCode result = LzhamInterop.DecompressForgelightData(CompressedBuffer, CompressedSize, DecompressedBuffer, DecompressedSize);

                if (result != InflateReturnCode.LZHAM_Z_STREAM_END && result != InflateReturnCode.LZHAM_Z_OK)
                {
                    //This chunk is invalid, or something went wrong.
                    return false;
                }
            }

            using (MemoryStream decompressedStream = new MemoryStream(DecompressedBuffer, 0, (int) DecompressedSize))
            {
                using (BinaryReader binaryReader = new BinaryReader(decompressedStream))
                {
                    //Textures
                    uint textureCount = binaryReader.ReadUInt32();

                    for (int i = 0; i < textureCount; i++)
                    {
                        Texture texture = new Texture();

                        uint colorNxMapSize = binaryReader.ReadUInt32();
                        if (colorNxMapSize > 0)
                        {
                            texture.ColorNXMap = binaryReader.ReadBytes((int) colorNxMapSize).ToList();
                        }

                        uint specNyMapSize = binaryReader.ReadUInt32();
                        if (specNyMapSize > 0)
                        {
                            texture.SpecNyMap = binaryReader.ReadBytes((int) specNyMapSize).ToList();
                        }

                        uint extraData1Size = binaryReader.ReadUInt32();
                        if (extraData1Size > 0)
                        {
                            texture.ExtraData1 = binaryReader.ReadBytes((int) extraData1Size).ToList();
                        }

                        uint extraData2Size = binaryReader.ReadUInt32();
                        if (extraData2Size > 0)
                        {
                            texture.ExtraData2 = binaryReader.ReadBytes((int) extraData2Size).ToList();
                        }

                        uint extraData3Size = binaryReader.ReadUInt32();
                        if (extraData3Size > 0)
                        {
                            texture.ExtraData3 = binaryReader.ReadBytes((int) extraData3Size).ToList();
                        }

                        uint extraData4Size = binaryReader.ReadUInt32();
                        if (extraData4Size > 0)
                        {
                            texture.ExtraData4 = binaryReader.ReadBytes((int) extraData4Size).ToList();
                        }

                        Textures.Add(texture);
                    }

                    //Verts Per Side
                    VertsPerSide = binaryReader.ReadUInt32();

                    //Height Maps
                    uint heightMapCount = binaryReader.ReadUInt32();

                    int n = (int) (heightMapCount / 4);

                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < n; j++)
                        {
                            Dictionary<int, HeightMap> entry;

                            if (!HeightMaps.ContainsKey(i))
                            {
                                entry = new Dictionary<int, HeightMap>();
                                HeightMaps[i] = entry;
                            }

                            else
                            {
                                entry = HeightMaps[i];
                            }

                            HeightMap heightMapData = new HeightMap();
                            heightMapData.Val1 = binaryReader.ReadInt16();
                            heightMapData.Val2 = binaryReader.ReadByte();
                            heightMapData.Val3 = binaryReader.ReadByte();

                            entry[j] = heightMapData;
                        }
                    }

                    //Indices
                    uint indexCount = binaryReader.ReadUInt32();

                    for (int i = 0; i < indexCount; i++)
                    {
                        Indices.Add(binaryReader.ReadUInt16());
                    }

                    //Verts
                    uint vertCount = binaryReader.ReadUInt32();

                    for (int i = 0; i < vertCount; i++)
                    {
                        Vertex vertex = new Vertex();

                        vertex.X = binaryReader.ReadInt16();
                        vertex.Y = binaryReader.ReadInt16();
                        vertex.HeightFar = binaryReader.ReadInt16();
                        vertex.HeightNear = binaryReader.ReadInt16();
                        vertex.Color = binaryReader.ReadUInt32();

                        Vertices.Add(vertex);
                    }

                    //TODO HACK - Daybreak, why are some chunks (that have a version 2 header) actually version 1?
                    long offset = binaryReader.BaseStream.Position;
                    try
                    {
                        //Render Batches
                        uint renderBatchCount = binaryReader.ReadUInt32();

                        for (int i = 0; i < renderBatchCount; i++)
                        {
                            RenderBatch renderBatch = new RenderBatch();

                            if (ChunkType == ChunkType.H1Z1_Planetside2V2)
                            {
                                renderBatch.Unknown = binaryReader.ReadUInt32();
                            }

                            renderBatch.IndexOffset = binaryReader.ReadUInt32();
                            renderBatch.IndexCount = binaryReader.ReadUInt32();
                            renderBatch.VertexOffset = binaryReader.ReadUInt32();
                            renderBatch.VertexCount = binaryReader.ReadUInt32();

                            RenderBatches.Add(renderBatch);
                        }

                        //Optimized Draw
                        uint optimizedDrawCount = binaryReader.ReadUInt32();

                        for (int i = 0; i < optimizedDrawCount; i++)
                        {
                            OptimizedDraw optimizedDraw = new OptimizedDraw();
                            optimizedDraw.Data = binaryReader.ReadBytes(320).ToList();

                            OptimizedDraws.Add(optimizedDraw);
                        }

                        //Unknown Data
                        uint unknownShort1Count = binaryReader.ReadUInt32();

                        for (int i = 0; i < unknownShort1Count; i++)
                        {
                            UnknownShorts1.Add(binaryReader.ReadUInt16());
                        }

                        //Unknown Data
                        uint unknownVectors1Count = binaryReader.ReadUInt32();

                        for (int i = 0; i < unknownVectors1Count; i++)
                        {
                            UnknownVectors1.Add(new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(),
                                binaryReader.ReadSingle()));
                        }

                        //Tile Occluder Info
                        uint tileOccluderCount = binaryReader.ReadUInt32();

                        if (tileOccluderCount > 16)
                        {
                            throw new ArgumentOutOfRangeException();
                        }

                        for (int i = 0; i < tileOccluderCount; i++)
                        {
                            TileOccluderInfo tileOccluderInfo = new TileOccluderInfo();
                            tileOccluderInfo.Data = binaryReader.ReadBytes(64).ToList();

                            TileOccluderInfos.Add(tileOccluderInfo);
                        }
                    }
                    catch (Exception)
                    {
                        // Some of these may have been populated from the "try".
                        RenderBatches.Clear();
                        OptimizedDraws.Clear();
                        UnknownShorts1.Clear();
                        UnknownVectors1.Clear();
                        TileOccluderInfos.Clear();

                        binaryReader.BaseStream.Position = offset;

                        //Render Batches
                        uint renderBatchCount = binaryReader.ReadUInt32();

                        for (int i = 0; i < renderBatchCount; i++)
                        {
                            RenderBatch renderBatch = new RenderBatch();

                            renderBatch.IndexOffset = binaryReader.ReadUInt32();
                            renderBatch.IndexCount = binaryReader.ReadUInt32();
                            renderBatch.VertexOffset = binaryReader.ReadUInt32();
                            renderBatch.VertexCount = binaryReader.ReadUInt32();

                            RenderBatches.Add(renderBatch);
                        }

                        //Optimized Draw
                        uint optimizedDrawCount = binaryReader.ReadUInt32();

                        for (int i = 0; i < optimizedDrawCount; i++)
                        {
                            OptimizedDraw optimizedDraw = new OptimizedDraw();
                            optimizedDraw.Data = binaryReader.ReadBytes(320).ToList();

                            OptimizedDraws.Add(optimizedDraw);
                        }

                        //Unknown Data
                        uint unknownShort1Count = binaryReader.ReadUInt32();

                        for (int i = 0; i < unknownShort1Count; i++)
                        {
                            UnknownShorts1.Add(binaryReader.ReadUInt16());
                        }

                        //Unknown Data
                        uint unknownVectors1Count = binaryReader.ReadUInt32();

                        for (int i = 0; i < unknownVectors1Count; i++)
                        {
                            UnknownVectors1.Add(new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(),
                                binaryReader.ReadSingle()));
                        }

                        //Tile Occluder Info
                        uint tileOccluderCount = binaryReader.ReadUInt32();

                        if (tileOccluderCount > 16)
                        {
                            return false;
                        }

                        for (int i = 0; i < tileOccluderCount; i++)
                        {
                            TileOccluderInfo tileOccluderInfo = new TileOccluderInfo();
                            tileOccluderInfo.Data = binaryReader.ReadBytes(64).ToList();

                            TileOccluderInfos.Add(tileOccluderInfo);
                        }
                    }
                }
            }

            return true;
        }
    }
}
