using System;
using System.Collections.Generic;
using System.IO;

namespace Forgelight.Formats.Zone
{
    public class Zone
    {
        //Header
        public UInt32 Version { get; private set; }
        public Dictionary<string, UInt32> Offsets { get; private set; }
        public UInt32 QuadsPerTile { get; private set; }
        public float TileSize { get; private set; }
        public float TileHeight { get; private set; }
        public UInt32 VertsPerTile { get; private set; }
        public UInt32 TilesPerChunk { get; private set; }
        public Int32 StartX { get; private set; }
        public Int32 StartY { get; private set; }
        public UInt32 ChunksX { get; private set; }
        public UInt32 ChunksY { get; private set; }

        //Data
        public List<Eco> Ecos { get; private set; }
        public List<Flora> Floras { get; private set; }
        public List<InvisibleWall> InvisibleWalls { get; private set; }
        public List<Object> Objects { get; private set; }
        public List<Light> Lights { get; private set; }

        public static Zone LoadFromStream(Stream stream)
        {
            BinaryReader binaryReader = new BinaryReader(stream);

            //Header
            byte[] magic = binaryReader.ReadBytes(4);

            if (magic[0] != 'Z' ||
                magic[1] != 'O' ||
                magic[2] != 'N' ||
                magic[3] != 'E')
            {
                return null;
            }

            Zone zone = new Zone();
            zone.Version = binaryReader.ReadUInt32();

            if (zone.Version != 1)
            {
                return null;
            }

            zone.Offsets = new Dictionary<string, uint>();
            zone.Offsets["ecos"] = binaryReader.ReadUInt32();
            zone.Offsets["floras"] = binaryReader.ReadUInt32();
            zone.Offsets["invisibleWalls"] = binaryReader.ReadUInt32();
            zone.Offsets["objects"] = binaryReader.ReadUInt32();
            zone.Offsets["lights"] = binaryReader.ReadUInt32();
            zone.Offsets["unknowns"] = binaryReader.ReadUInt32();

            zone.QuadsPerTile = binaryReader.ReadUInt32();
            zone.TileSize = binaryReader.ReadSingle();
            zone.TileHeight = binaryReader.ReadSingle();
            zone.VertsPerTile = binaryReader.ReadUInt32();
            zone.TilesPerChunk = binaryReader.ReadUInt32();
            zone.StartX = binaryReader.ReadInt32();
            zone.StartY = binaryReader.ReadInt32();
            zone.ChunksX = binaryReader.ReadUInt32();
            zone.ChunksY = binaryReader.ReadUInt32();

            //uint currentOffset = 68;
            //long offsetStream = binaryReader.BaseStream.Position;

            //Ecos
            zone.Ecos = new List<Eco>();
            UInt32 ecosLength = binaryReader.ReadUInt32();

            for (uint i = 0; i < ecosLength; i++)
            {
                zone.Ecos.Add(Eco.ReadFromStream(binaryReader.BaseStream));
            }

            //Floras
            zone.Floras = new List<Flora>();
            UInt32 florasLength = binaryReader.ReadUInt32();

            for (uint i = 0; i < florasLength; i++)
            {
                zone.Floras.Add(Flora.ReadFromStream(binaryReader.BaseStream));
            }

            //Invisible Walls
            zone.InvisibleWalls = new List<InvisibleWall>();
            UInt32 invisibleWallsLength = binaryReader.ReadUInt32();

            for (uint i = 0; i < invisibleWallsLength; i++)
            {
                zone.InvisibleWalls.Add(InvisibleWall.ReadFromStream(binaryReader.BaseStream));
            }

            //Objects
            zone.Objects = new List<Object>();
            UInt32 objectsLength = binaryReader.ReadUInt32();

            for (uint i = 0; i < objectsLength; i++)
            {
                zone.Objects.Add(Object.ReadFromStream(binaryReader.BaseStream));
            }

            //Lights
            zone.Lights = new List<Light>();
            UInt32 lightsLength = binaryReader.ReadUInt32();

            for (uint i = 0; i < lightsLength; i++)
            {
                zone.Lights.Add(Light.ReadFromStream(binaryReader.BaseStream));
            }

            return zone;
        }
    }
}
