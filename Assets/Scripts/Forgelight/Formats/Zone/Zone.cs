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
        public Int32 ChunksX { get; private set; }
        public Int32 ChunksY { get; private set; }

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


            return zone;
        }
    }
}
