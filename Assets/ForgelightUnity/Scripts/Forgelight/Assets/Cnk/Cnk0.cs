namespace ForgelightUnity.Forgelight.Assets.Cnk
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using LzhamWrapper;
    using UnityEngine;

    public class Cnk0 : Asset
    {
        public override string Name { get; protected set; }
        public override string DisplayName { get; protected set; }
        public ChunkType ChunkType { get; private set; }

        #region Structure
        //Header
        public uint Version { get; private set; }
        public uint DecompressedSize { get; private set; }
        public uint CompressedSize { get; private set; }

        //Tiles
        public List<Tile> Tiles { get; private set; }
        public class Tile
        {
            public class Eco
            {
                public class Flora
                {
                    public class Layer
                    {
                        public uint UnknownUint1 { get; set; }
                        public uint UnknownUint2 { get; set; }
                    }

                    public List<Layer> Layers { get; set; }
                }

                public uint ID { get; set; }
                public List<Flora> Floras { get; set; }
            }

            public int X { get; set; }
            public int Y { get; set; }
            public int UnknownInt1 { get; set; }
            public int UnknownInt2 { get; set; }
            public List<Eco> Ecos { get; set; }
            public uint Index { get; set; } //TODO Verify if this is an int or uint
            public uint UnknownInt3 { get; set; } //TODO Verify if this is an int or uint
            public List<byte> ImageData { get; set; }
            public List<byte> LayerTextures { get; set; }
        }

        //Unknown Data
        public int UnknownInt1 { get; private set; }
        public List<Unknown1> UnknownArray1 { get; private set; }
        public class Unknown1
        {
            public short Height { get; set; }
            public byte UnknownByte1 { get; set; }
            public byte UnknownByte2 { get; set; }
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
            public uint Color1 { get; set; }
            public uint Color2 { get; set; }
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

        public static Cnk0 LoadFromStream(string name, string displayName, MemoryStream stream)
        {
            Cnk0 chunk = new Cnk0();
            BinaryReader binaryReader = new BinaryReader(stream);

            chunk.Name = name;
            chunk.DisplayName = displayName;
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

                //Tiles
                uint tileCount = binaryReader.ReadUInt32();
                chunk.Tiles = new List<Tile>((int) tileCount);

                for (int i = 0; i < tileCount; i++)
                {
                    Tile tile = new Tile();

                    tile.X = binaryReader.ReadInt32();
                    tile.Y = binaryReader.ReadInt32();
                    tile.UnknownInt1 = binaryReader.ReadInt32();
                    tile.UnknownInt2 = binaryReader.ReadInt32();

                    uint ecosCount = binaryReader.ReadUInt32();

                    if (ecosCount > 0)
                    {
                        tile.Ecos = new List<Tile.Eco>((int)ecosCount);

                        for (int j = 0; j < ecosCount; j++)
                        {
                            Tile.Eco eco = new Tile.Eco();

                            eco.ID = binaryReader.ReadUInt32();

                            uint florasCount = binaryReader.ReadUInt32();
                            eco.Floras = new List<Tile.Eco.Flora>((int)florasCount);

                            for (int k = 0; k < florasCount; k++)
                            {
                                Tile.Eco.Flora flora = new Tile.Eco.Flora();

                                uint layersCount = binaryReader.ReadUInt32();
                                flora.Layers = new List<Tile.Eco.Flora.Layer>((int)layersCount);

                                for (int l = 0; l < layersCount; l++)
                                {
                                    Tile.Eco.Flora.Layer layer = new Tile.Eco.Flora.Layer();

                                    layer.UnknownUint1 = binaryReader.ReadUInt32();
                                    layer.UnknownUint2 = binaryReader.ReadUInt32();

                                    flora.Layers.Add(layer);
                                }

                                eco.Floras.Add(flora);
                            }

                            tile.Ecos.Add(eco);
                        }
                    }

                    tile.Index = binaryReader.ReadUInt32();
                    tile.UnknownInt3 = binaryReader.ReadUInt32();

                    uint imageSize = binaryReader.ReadUInt32();
                    if (imageSize > 0)
                    {
                        tile.ImageData = binaryReader.ReadBytes((int)imageSize).ToList();
                    }

                    uint layerTexturesCount = binaryReader.ReadUInt32();
                    if (layerTexturesCount > 0)
                    {
                        tile.LayerTextures = binaryReader.ReadBytes((int)layerTexturesCount).ToList();
                    }

                    chunk.Tiles.Add(tile);
                }

                //Unknown Data
                chunk.UnknownInt1 = binaryReader.ReadInt32();

                uint unknownCount = binaryReader.ReadUInt32();
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
                uint indexCount = binaryReader.ReadUInt32();
                chunk.Indices = new List<ushort>((int) indexCount);

                for (int i = 0; i < indexCount; i++)
                {
                    chunk.Indices.Add(binaryReader.ReadUInt16());
                }

                //Verts
                uint vertCount = binaryReader.ReadUInt32();
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
                catch (EndOfStreamException)
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
