using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LzhamWrapper;
using UnityEngine;

namespace Forgelight.Formats.Cnk
{
    public class CnkLOD
    {
        public class Texture
        {
            public List<byte> ColorNXMap { get; set; }
            public List<byte> SpecNyMap { get; set; }
            public List<byte> ExtraData1 { get; set; }
            public List<byte> ExtraData2 { get; set; }
            public List<byte> ExtraData3 { get; set; }
            public List<byte> ExtraData4 { get; set; }
        }

        public class HeightMap
        {
            public Int16 Val1 { get; set; }
            public byte Val2 { get; set; }
            public byte Val3 { get; set; }
        }

        public class Vertex
        {
            public Int16 X { get; set; }
            public Int16 Y { get; set; }
            public Int16 HeightFar { get; set; }
            public Int16 HeightNear { get; set; }
            public UInt32 Color { get; set; }
        }

        public class RenderBatch
        {
            public UInt32 IndexOffset { get; set; }
            public UInt32 IndexCount { get; set; }
            public UInt32 VertexOffset { get; set; }
            public UInt32 VertexCount { get; set; }
        }

        public class OptimizedDraw
        {
            public List<byte> Data { get; set; }
        }

        public class TileOccluderInfo
        {
            public List<byte> Data { get; set; }
        }

        //Header
        public string Name { get; private set; }
        public UInt32 Version { get; private set; }
        public UInt32 UncompressedSize { get; private set; }
        public UInt32 CompressedSize { get; private set; }

        //Textures
        public List<Texture> Textures { get; private set; }

        //Verts per side
        public UInt32 VertsPerSide { get; private set; }

        public Dictionary<int, Dictionary<int, HeightMap>> HeightMaps = new Dictionary<int, Dictionary<int, HeightMap>>();

        //Indices
        public List<UInt16> Indices { get; private set; }

        //Verts
        public List<Vertex> Vertices { get; private set; }

        //Render Batches
        public List<RenderBatch> RenderBatches { get; private set; }

        //Optimized Draw
        public List<OptimizedDraw> OptimizedDraws { get; private set; }

        //Unknown Data
        public List<UInt16> UnknownShorts1 { get; private set; }

        //Unknown Data
        public List<Vector3> UnknownVectors1 { get; private set; }

        //Tile Occluder Info
        public List<TileOccluderInfo> TileOccluderInfos { get; private set; }

        public static CnkLOD LoadFromStream(string name, MemoryStream stream)
        {
            CnkLOD chunk = new CnkLOD();
            BinaryReader binaryReader = new BinaryReader(stream);

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

            UInt32 decompressedSize = binaryReader.ReadUInt32();
            UInt32 compressedSize = binaryReader.ReadUInt32();

            //Decompression
            byte[] compressedBuffer = binaryReader.ReadBytes((int) compressedSize);
            byte[] decompressedBuffer = new byte[decompressedSize];

            InflateReturnCode result = LzhamInterop.DecompressForgelightData(compressedBuffer, compressedSize, decompressedBuffer, decompressedSize);

            if (result != InflateReturnCode.LZHAM_Z_STREAM_END && result != InflateReturnCode.LZHAM_Z_OK)
            {
                //This chunk is invalid.
                return null;
            }

            using (MemoryStream decompressedStream = new MemoryStream(decompressedBuffer))
            {
                binaryReader = new BinaryReader(decompressedStream);

                //Textures
                UInt32 textureCount = binaryReader.ReadUInt32();
                chunk.Textures = new List<Texture>((int)textureCount);

                for (int i = 0; i < textureCount; i++)
                {
                    Texture texture = new Texture();

                    UInt32 colorNxMapSize = binaryReader.ReadUInt32();
                    if (colorNxMapSize > 0)
                    {
                        texture.ColorNXMap = binaryReader.ReadBytes((int)colorNxMapSize).ToList();
                    }

                    UInt32 specNyMapSize = binaryReader.ReadUInt32();
                    if (specNyMapSize > 0)
                    {
                        texture.SpecNyMap = binaryReader.ReadBytes((int)specNyMapSize).ToList();
                    }

                    UInt32 extraData1Size = binaryReader.ReadUInt32();
                    if (extraData1Size > 0)
                    {
                        texture.ExtraData1 = binaryReader.ReadBytes((int)extraData1Size).ToList();
                    }

                    UInt32 extraData2Size = binaryReader.ReadUInt32();
                    if (extraData2Size > 0)
                    {
                        texture.ExtraData2 = binaryReader.ReadBytes((int)extraData2Size).ToList();
                    }

                    UInt32 extraData3Size = binaryReader.ReadUInt32();
                    if (extraData3Size > 0)
                    {
                        texture.ExtraData3 = binaryReader.ReadBytes((int)extraData3Size).ToList();
                    }

                    UInt32 extraData4Size = binaryReader.ReadUInt32();
                    if (extraData4Size > 0)
                    {
                        texture.ExtraData4 = binaryReader.ReadBytes((int)extraData4Size).ToList();
                    }
                }

                //Verts Per Side
                chunk.VertsPerSide = binaryReader.ReadUInt32();

                //Height Maps
                UInt32 heightMapCount = binaryReader.ReadUInt32();

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
                UInt32 indexCount = binaryReader.ReadUInt32();
                chunk.Indices = new List<ushort>((int)indexCount);

                for (int i = 0; i < indexCount; i++)
                {
                    chunk.Indices.Add(binaryReader.ReadUInt16());
                }

                //Verts
                UInt32 vertCount = binaryReader.ReadUInt32();
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

                //Render Batches
                UInt32 renderBatchCount = binaryReader.ReadUInt32();
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
                UInt32 optimizedDrawCount = binaryReader.ReadUInt32();
                chunk.OptimizedDraws = new List<OptimizedDraw>((int)optimizedDrawCount);

                for (int i = 0; i < optimizedDrawCount; i++)
                {
                    OptimizedDraw optimizedDraw = new OptimizedDraw();
                    optimizedDraw.Data = binaryReader.ReadBytes(320).ToList();
                }

                //Unknown Data
                UInt32 unknownShort1Count = binaryReader.ReadUInt32();
                chunk.UnknownShorts1 = new List<ushort>((int)unknownShort1Count);

                for (int i = 0; i < unknownShort1Count; i++)
                {
                    chunk.UnknownShorts1.Add(binaryReader.ReadUInt16());
                }

                //Unknown Data
                UInt32 unknownVectors1Count = binaryReader.ReadUInt32();
                chunk.UnknownVectors1 = new List<Vector3>((int)unknownVectors1Count);

                for (int i = 0; i < unknownVectors1Count; i++)
                {
                    chunk.UnknownVectors1.Add(new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle()));
                }

                //Tile Occluder Info
                UInt32 tileOccluderCount = binaryReader.ReadUInt32();
                chunk.TileOccluderInfos = new List<TileOccluderInfo>((int)tileOccluderCount);

                for (int i = 0; i < tileOccluderCount; i++)
                {
                    TileOccluderInfo tileOccluderInfo = new TileOccluderInfo();
                    tileOccluderInfo.Data = binaryReader.ReadBytes(64).ToList();
                }
            }

            return chunk;
        }
    }
}
