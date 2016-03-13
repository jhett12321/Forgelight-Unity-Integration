using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LzhamWrapper;
using UnityEngine;

namespace Forgelight.Formats.Cnk
{
    public class Cnk0
    {
        public class Tile
        {
            public class Eco
            {
                public class Flora
                {
                    public class Layer
                    {
                        public UInt32 Unknown1 { get; set; }
                        public UInt32 Unknown2 { get; set; }
                    }

                    public List<Layer> Layers { get; set; }
                }

                public UInt32 ID { get; set; }
                public List<Flora> Floras { get; set; }
            }

            public Int32 X { get; set; }
            public Int32 Y { get; set; }
            public Int32 UnknownInt1 { get; set; }
            public Int32 UnknownInt2 { get; set; }
            public List<Eco> Ecos { get; set; }
            public UInt32 Index { get; set; } //TODO Verify if this is an int or uint
            public UInt32 UnknownInt3 { get; set; } //TODO Verify if this is an int or uint
            public List<byte> ImageData { get; set; }
            public List<byte> LayerTextures { get; set; }

        }

        public class Unknown1
        {
            public Int16 Height { get; set; }
            public byte UnknownByte1 { get; set; }
            public byte UnknownByte2 { get; set; }
        }

        public class Vertex
        {
            public Int16 X { get; set; }
            public Int16 Y { get; set; }
            public Int16 HeightFar { get; set; }
            public Int16 HeightNear { get; set; }
            public UInt32 Color1 { get; set; }
            public UInt32 Color2 { get; set; }
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

        //Tiles
        public List<Tile> Tiles { get; private set; }

        //Unknown Data
        public Int32 UnknownInt1 { get; private set; }
        public List<Unknown1> UnknownArray1 { get; private set; }

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

        public static Cnk0 LoadFromStream(string name, MemoryStream stream)
        {
            Cnk0 chunk = new Cnk0();
            BinaryReader binaryReader = new BinaryReader(stream);

            chunk.Name = name;
            //Header
            byte[] magic = binaryReader.ReadBytes(4);

            if (magic[0] != 'C' ||
                magic[1] != 'N' ||
                magic[2] != 'K' ||
                magic[3] != '0')
            {
                return null;
            }

            chunk.Version = binaryReader.ReadUInt32();

            UInt32 decompressedSize = binaryReader.ReadUInt32();
            UInt32 compressedSize = binaryReader.ReadUInt32();

            //Decompression
            byte[] compressedBuffer = binaryReader.ReadBytes((int)compressedSize);
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

                //Tiles
                UInt32 tileCount = binaryReader.ReadUInt32();
                chunk.Tiles = new List<Tile>((int) tileCount);

                for (int i = 0; i < tileCount; i++)
                {
                    Tile tile = new Tile();

                    tile.X = binaryReader.ReadInt32();
                    tile.Y = binaryReader.ReadInt32();
                    tile.UnknownInt1 = binaryReader.ReadInt32();
                    tile.UnknownInt2 = binaryReader.ReadInt32();

                    UInt32 ecosCount = binaryReader.ReadUInt32();

                    if (ecosCount > 0)
                    {
                        tile.Ecos = new List<Tile.Eco>((int)ecosCount);

                        for (int j = 0; j < ecosCount; j++)
                        {
                            Tile.Eco eco = new Tile.Eco();

                            eco.ID = binaryReader.ReadUInt32();

                            UInt32 florasCount = binaryReader.ReadUInt32();
                            eco.Floras = new List<Tile.Eco.Flora>((int)florasCount);

                            for (int k = 0; k < florasCount; k++)
                            {
                                Tile.Eco.Flora flora = new Tile.Eco.Flora();

                                UInt32 layersCount = binaryReader.ReadUInt32();
                                flora.Layers = new List<Tile.Eco.Flora.Layer>((int)layersCount);

                                for (int l = 0; l < layersCount; l++)
                                {
                                    Tile.Eco.Flora.Layer layer = new Tile.Eco.Flora.Layer();

                                    layer.Unknown1 = binaryReader.ReadUInt32();
                                    layer.Unknown2 = binaryReader.ReadUInt32();

                                    flora.Layers.Add(layer);
                                }

                                eco.Floras.Add(flora);
                            }

                            tile.Ecos.Add(eco);
                        }
                    }

                    tile.Index = binaryReader.ReadUInt32();
                    tile.UnknownInt3 = binaryReader.ReadUInt32();

                    UInt32 imageSize = binaryReader.ReadUInt32();
                    if (imageSize > 0)
                    {
                        tile.ImageData = binaryReader.ReadBytes((int)imageSize).ToList();
                    }

                    UInt32 layerTexturesCount = binaryReader.ReadUInt32();
                    if (layerTexturesCount > 0)
                    {
                        tile.LayerTextures = binaryReader.ReadBytes((int)layerTexturesCount).ToList();
                    }

                    chunk.Tiles.Add(tile);
                }

                //Unknown Data
                chunk.UnknownInt1 = binaryReader.ReadInt32();

                UInt32 unknownCount = binaryReader.ReadUInt32();
                chunk.UnknownArray1 = new List<Unknown1>((int) unknownCount);

                for (int i = 0; i < unknownCount; i++)
                {
                    Unknown1 unknown1 = new Unknown1();

                    unknown1.Height = binaryReader.ReadInt16();
                    unknown1.UnknownByte1 = binaryReader.ReadByte();
                    unknown1.UnknownByte2 = binaryReader.ReadByte();

                    chunk.UnknownArray1.Add(unknown1);
                }

                //Indices
                UInt32 indexCount = binaryReader.ReadUInt32();
                chunk.Indices = new List<ushort>((int) indexCount);

                for (int i = 0; i < indexCount; i++)
                {
                    chunk.Indices.Add(binaryReader.ReadUInt16());
                }

                //Verts
                UInt32 vertCount = binaryReader.ReadUInt32();
                chunk.Vertices = new List<Vertex>((int) vertCount);

                for (int i = 0; i < vertCount; i++)
                {
                    Vertex vertex = new Vertex();

                    vertex.X = binaryReader.ReadInt16();
                    vertex.Y = binaryReader.ReadInt16();
                    vertex.HeightFar = binaryReader.ReadInt16();
                    vertex.HeightNear = binaryReader.ReadInt16();
                    vertex.Color1 = binaryReader.ReadUInt32();
                    vertex.Color2 = binaryReader.ReadUInt32();

                    chunk.Vertices.Add(vertex);
                }

                //Render Batches
                UInt32 renderBatchCount = binaryReader.ReadUInt32();
                chunk.RenderBatches = new List<RenderBatch>((int) renderBatchCount);

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
                chunk.OptimizedDraws = new List<OptimizedDraw>((int) optimizedDrawCount);

                for (int i = 0; i < optimizedDrawCount; i++)
                {
                    OptimizedDraw optimizedDraw = new OptimizedDraw();
                    optimizedDraw.Data = binaryReader.ReadBytes(320).ToList();

                    chunk.OptimizedDraws.Add(optimizedDraw);
                }

                //Unknown Data
                UInt32 unknownShort1Count = binaryReader.ReadUInt32();
                chunk.UnknownShorts1 = new List<ushort>((int) unknownShort1Count);

                for (int i = 0; i < unknownShort1Count; i++)
                {
                    chunk.UnknownShorts1.Add(binaryReader.ReadUInt16());
                }

                //Unknown Data
                UInt32 unknownVectors1Count = binaryReader.ReadUInt32();
                chunk.UnknownVectors1 = new List<Vector3>((int) unknownVectors1Count);

                for (int i = 0; i < unknownVectors1Count; i++)
                {
                    chunk.UnknownVectors1.Add(new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle()));
                }

                //Tile Occluder Info
                UInt32 tileOccluderCount = binaryReader.ReadUInt32();
                chunk.TileOccluderInfos = new List<TileOccluderInfo>((int) tileOccluderCount);

                for (int i = 0; i < tileOccluderCount; i++)
                {
                    TileOccluderInfo tileOccluderInfo = new TileOccluderInfo();
                    tileOccluderInfo.Data = binaryReader.ReadBytes(64).ToList();

                    chunk.TileOccluderInfos.Add(tileOccluderInfo);
                }
            }

            return chunk;
        }
    }
}
