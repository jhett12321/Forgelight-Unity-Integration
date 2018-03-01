namespace ForgelightUnity.Forgelight.Assets.Cnk
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using LzhamWrapper;
    using UnityEngine;

    public class CnkLOD : Asset
    {
        public override string Name { get; protected set; }
        public override string DisplayName { get; protected set; }
        public ChunkType ChunkType { get; private set; }

        #region Structure
        //Header
        public uint Version { get; private set; }

        public uint DecompressedSize { get; private set; }
        public uint CompressedSize { get; private set; }

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

        public static CnkLOD LoadFromStream(string name, string displayName, MemoryStream stream)
        {
            CnkLOD chunk = new CnkLOD();
            BinaryReader binaryReader = new BinaryReader(stream);

            chunk.Name = name;
            chunk.DisplayName = displayName;
            //Header
            byte[] magic = binaryReader.ReadBytes(4);

            if (magic[0] != 'C' ||
                magic[1] != 'N' ||
                magic[2] != 'K'/* ||
                magic[3] != '1'*/)
            {
                return null;
            }

            chunk.Version = binaryReader.ReadUInt32();

            if (!Enum.IsDefined(typeof(ChunkType), (int)chunk.Version))
            {
                Debug.LogWarning("Could not decode chunk " + name + ". Unknown cnk version " + chunk.Version);
                return null;
            }

            chunk.ChunkType = (ChunkType)chunk.Version;

            chunk.DecompressedSize = binaryReader.ReadUInt32();
            chunk.CompressedSize = binaryReader.ReadUInt32();

            //Decompression
            byte[] compressedBuffer = binaryReader.ReadBytes((int)chunk.CompressedSize);
            byte[] decompressedBuffer = new byte[chunk.DecompressedSize];

            InflateReturnCode result = LzhamInterop.DecompressForgelightData(compressedBuffer, chunk.CompressedSize, decompressedBuffer, chunk.DecompressedSize);

            if (result != InflateReturnCode.LZHAM_Z_STREAM_END && result != InflateReturnCode.LZHAM_Z_OK)
            {
                //This chunk is invalid.
                return null;
            }

            using (MemoryStream decompressedStream = new MemoryStream(decompressedBuffer))
            {
                binaryReader = new BinaryReader(decompressedStream);

                //Textures
                uint textureCount = binaryReader.ReadUInt32();
                chunk.Textures = new List<Texture>((int)textureCount);

                for (int i = 0; i < textureCount; i++)
                {
                    Texture texture = new Texture();

                    uint colorNxMapSize = binaryReader.ReadUInt32();
                    if (colorNxMapSize > 0)
                    {
                        texture.ColorNXMap = binaryReader.ReadBytes((int)colorNxMapSize).ToList();
                    }

                    uint specNyMapSize = binaryReader.ReadUInt32();
                    if (specNyMapSize > 0)
                    {
                        texture.SpecNyMap = binaryReader.ReadBytes((int)specNyMapSize).ToList();
                    }

                    uint extraData1Size = binaryReader.ReadUInt32();
                    if (extraData1Size > 0)
                    {
                        texture.ExtraData1 = binaryReader.ReadBytes((int)extraData1Size).ToList();
                    }

                    uint extraData2Size = binaryReader.ReadUInt32();
                    if (extraData2Size > 0)
                    {
                        texture.ExtraData2 = binaryReader.ReadBytes((int)extraData2Size).ToList();
                    }

                    uint extraData3Size = binaryReader.ReadUInt32();
                    if (extraData3Size > 0)
                    {
                        texture.ExtraData3 = binaryReader.ReadBytes((int)extraData3Size).ToList();
                    }

                    uint extraData4Size = binaryReader.ReadUInt32();
                    if (extraData4Size > 0)
                    {
                        texture.ExtraData4 = binaryReader.ReadBytes((int)extraData4Size).ToList();
                    }

                    chunk.Textures.Add(texture);
                }

                //Verts Per Side
                chunk.VertsPerSide = binaryReader.ReadUInt32();

                //Height Maps
                uint heightMapCount = binaryReader.ReadUInt32();

                int n = (int)(heightMapCount / 4);

                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        Dictionary<int, HeightMap> entry;

                        if (!chunk.HeightMaps.ContainsKey(i))
                        {
                            entry = new Dictionary<int, HeightMap>();
                            chunk.HeightMaps[i] = entry;
                        }

                        else
                        {
                            entry = chunk.HeightMaps[i];
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
                chunk.Indices = new List<ushort>((int)indexCount);

                for (int i = 0; i < indexCount; i++)
                {
                    chunk.Indices.Add(binaryReader.ReadUInt16());
                }

                //Verts
                uint vertCount = binaryReader.ReadUInt32();
                chunk.Vertices = new List<Vertex>((int)vertCount);

                for (int i = 0; i < vertCount; i++)
                {
                    Vertex vertex = new Vertex();

                    vertex.X = binaryReader.ReadInt16();
                    vertex.Y = binaryReader.ReadInt16();
                    vertex.HeightFar = binaryReader.ReadInt16();
                    vertex.HeightNear = binaryReader.ReadInt16();
                    vertex.Color = binaryReader.ReadUInt32();

                    chunk.Vertices.Add(vertex);
                }

                //TODO HACK - Daybreak, why are some chunks (that have a version 2 header) actually version 1?
                long offset = binaryReader.BaseStream.Position;
                try
                {
                    //Render Batches
                    uint renderBatchCount = binaryReader.ReadUInt32();
                    chunk.RenderBatches = new List<RenderBatch>((int)renderBatchCount);

                    for (int i = 0; i < renderBatchCount; i++)
                    {
                        RenderBatch renderBatch = new RenderBatch();

                        if (chunk.ChunkType == ChunkType.H1Z1_Planetside2V2)
                        {
                            renderBatch.Unknown = binaryReader.ReadUInt32();
                        }

                        renderBatch.IndexOffset = binaryReader.ReadUInt32();
                        renderBatch.IndexCount = binaryReader.ReadUInt32();
                        renderBatch.VertexOffset = binaryReader.ReadUInt32();
                        renderBatch.VertexCount = binaryReader.ReadUInt32();

                        chunk.RenderBatches.Add(renderBatch);
                    }

                    //Optimized Draw
                    uint optimizedDrawCount = binaryReader.ReadUInt32();
                    chunk.OptimizedDraws = new List<OptimizedDraw>((int)optimizedDrawCount);

                    for (int i = 0; i < optimizedDrawCount; i++)
                    {
                        OptimizedDraw optimizedDraw = new OptimizedDraw();
                        optimizedDraw.Data = binaryReader.ReadBytes(320).ToList();

                        chunk.OptimizedDraws.Add(optimizedDraw);
                    }

                    //Unknown Data
                    uint unknownShort1Count = binaryReader.ReadUInt32();
                    chunk.UnknownShorts1 = new List<ushort>((int)unknownShort1Count);

                    for (int i = 0; i < unknownShort1Count; i++)
                    {
                        chunk.UnknownShorts1.Add(binaryReader.ReadUInt16());
                    }

                    //Unknown Data
                    uint unknownVectors1Count = binaryReader.ReadUInt32();
                    chunk.UnknownVectors1 = new List<Vector3>((int)unknownVectors1Count);

                    for (int i = 0; i < unknownVectors1Count; i++)
                    {
                        chunk.UnknownVectors1.Add(new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle()));
                    }

                    //Tile Occluder Info
                    uint tileOccluderCount = binaryReader.ReadUInt32();
                    chunk.TileOccluderInfos = new List<TileOccluderInfo>((int)tileOccluderCount);

                    for (int i = 0; i < tileOccluderCount; i++)
                    {
                        TileOccluderInfo tileOccluderInfo = new TileOccluderInfo();
                        tileOccluderInfo.Data = binaryReader.ReadBytes(64).ToList();

                        chunk.TileOccluderInfos.Add(tileOccluderInfo);
                    }
                }
                catch (Exception)
                {
                    binaryReader.BaseStream.Position = offset;

                    //Render Batches
                    uint renderBatchCount = binaryReader.ReadUInt32();
                    chunk.RenderBatches = new List<RenderBatch>((int)renderBatchCount);

                    for (int i = 0; i < renderBatchCount; i++)
                    {
                        RenderBatch renderBatch = new RenderBatch();

                        renderBatch.IndexOffset = binaryReader.ReadUInt32();
                        renderBatch.IndexCount = binaryReader.ReadUInt32();
                        renderBatch.VertexOffset = binaryReader.ReadUInt32();
                        renderBatch.VertexCount = binaryReader.ReadUInt32();

                        chunk.RenderBatches.Add(renderBatch);
                    }

                    //Optimized Draw
                    uint optimizedDrawCount = binaryReader.ReadUInt32();
                    chunk.OptimizedDraws = new List<OptimizedDraw>((int)optimizedDrawCount);

                    for (int i = 0; i < optimizedDrawCount; i++)
                    {
                        OptimizedDraw optimizedDraw = new OptimizedDraw();
                        optimizedDraw.Data = binaryReader.ReadBytes(320).ToList();

                        chunk.OptimizedDraws.Add(optimizedDraw);
                    }

                    //Unknown Data
                    uint unknownShort1Count = binaryReader.ReadUInt32();
                    chunk.UnknownShorts1 = new List<ushort>((int)unknownShort1Count);

                    for (int i = 0; i < unknownShort1Count; i++)
                    {
                        chunk.UnknownShorts1.Add(binaryReader.ReadUInt16());
                    }

                    //Unknown Data
                    uint unknownVectors1Count = binaryReader.ReadUInt32();
                    chunk.UnknownVectors1 = new List<Vector3>((int)unknownVectors1Count);

                    for (int i = 0; i < unknownVectors1Count; i++)
                    {
                        chunk.UnknownVectors1.Add(new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle()));
                    }

                    //Tile Occluder Info
                    uint tileOccluderCount = binaryReader.ReadUInt32();
                    chunk.TileOccluderInfos = new List<TileOccluderInfo>((int)tileOccluderCount);

                    for (int i = 0; i < tileOccluderCount; i++)
                    {
                        TileOccluderInfo tileOccluderInfo = new TileOccluderInfo();
                        tileOccluderInfo.Data = binaryReader.ReadBytes(64).ToList();

                        chunk.TileOccluderInfos.Add(tileOccluderInfo);
                    }
                }
            }

            return chunk;
        }
    }
}
