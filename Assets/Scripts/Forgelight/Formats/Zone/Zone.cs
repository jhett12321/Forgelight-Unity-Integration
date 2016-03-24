using System.Collections.Generic;
using System.IO;

namespace Forgelight.Formats.Zone
{
    public class Zone
    {
        public string Name { get; set; }

        //Header
        public uint Version { get; private set; }
        public Dictionary<string, uint> Offsets { get; private set; }
        public uint QuadsPerTile { get; private set; }
        public float TileSize { get; private set; }
        public float TileHeight { get; private set; }
        public uint VertsPerTile { get; private set; }
        public uint TilesPerChunk { get; private set; }
        public int StartX { get; private set; }
        public int StartY { get; private set; }
        public uint ChunksX { get; private set; }
        public uint ChunksY { get; private set; }

        //Data
        public List<Eco> Ecos { get; private set; }
        public List<Flora> Floras { get; private set; }
        public List<InvisibleWall> InvisibleWalls { get; private set; }
        public List<Object> Objects { get; private set; }
        public List<Light> Lights { get; private set; }
        public List<Unknown> Unknowns { get; private set; }

        public static Zone LoadFromStream(string Name, Stream stream)
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
            zone.Name = Name;
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

            //Ecos
            zone.Ecos = new List<Eco>();
            uint ecosLength = binaryReader.ReadUInt32();

            for (uint i = 0; i < ecosLength; i++)
            {
                zone.Ecos.Add(Eco.ReadFromStream(binaryReader.BaseStream));
            }

            //Floras
            zone.Floras = new List<Flora>();
            uint florasLength = binaryReader.ReadUInt32();

            for (uint i = 0; i < florasLength; i++)
            {
                zone.Floras.Add(Flora.ReadFromStream(binaryReader.BaseStream));
            }

            //Invisible Walls
            zone.InvisibleWalls = new List<InvisibleWall>();
            uint invisibleWallsLength = binaryReader.ReadUInt32();

            for (uint i = 0; i < invisibleWallsLength; i++)
            {
                zone.InvisibleWalls.Add(InvisibleWall.ReadFromStream(binaryReader.BaseStream));
            }

            //Objects
            zone.Objects = new List<Object>();
            uint objectsLength = binaryReader.ReadUInt32();

            for (uint i = 0; i < objectsLength; i++)
            {
                zone.Objects.Add(Object.ReadFromStream(binaryReader.BaseStream));
            }

            //Lights
            zone.Lights = new List<Light>();
            uint lightsLength = binaryReader.ReadUInt32();

            for (uint i = 0; i < lightsLength; i++)
            {
                zone.Lights.Add(Light.ReadFromStream(binaryReader.BaseStream));
            }

            //Unknowns
            uint unknownsLength = binaryReader.ReadUInt32();
            zone.Unknowns = new List<Unknown>((int) unknownsLength);

            //for (int i = 0; i < unknownsLength; i++)
            //{
            //    //zone.Unknowns.Add(Unknown.ReadFromStream(binaryReader.BaseStream));
            //    //???
            //}

            return zone;
        }

        public static void SerializeZoneToStream(Zone zone, Stream stream)
        {
            if (!stream.CanWrite || !stream.CanSeek)
            {
                return;
            }

            BinaryWriter binaryWriter = new BinaryWriter(stream);

            //Header
            byte[] magic = new byte[4];

            magic[0] = (byte) 'Z';
            magic[1] = (byte) 'O';
            magic[2] = (byte) 'N';
            magic[3] = (byte) 'E';

            binaryWriter.Write(magic);
            binaryWriter.Write(zone.Version);

            //Offsets
            long offsetsPosition = binaryWriter.BaseStream.Position;

            //Allocates space for the offset locations.
            binaryWriter.Write(0u);
            binaryWriter.Write(0u);
            binaryWriter.Write(0u);
            binaryWriter.Write(0u);
            binaryWriter.Write(0u);
            binaryWriter.Write(0u);

            //binaryWriter.Write(zone.Offsets["ecos"]);
            //binaryWriter.Write(zone.Offsets["floras"]);
            //binaryWriter.Write(zone.Offsets["invisibleWalls"]);
            //binaryWriter.Write(zone.Offsets["objects"]);
            //binaryWriter.Write(zone.Offsets["lights"]);
            //binaryWriter.Write(zone.Offsets["unknowns"]);

            Dictionary<string, uint> offsets = new Dictionary<string, uint>();

            //Misc Header
            binaryWriter.Write(zone.QuadsPerTile);
            binaryWriter.Write(zone.TileSize);
            binaryWriter.Write(zone.TileHeight);
            binaryWriter.Write(zone.VertsPerTile);
            binaryWriter.Write(zone.TilesPerChunk);
            binaryWriter.Write(zone.StartX);
            binaryWriter.Write(zone.StartY);
            binaryWriter.Write(zone.ChunksX);
            binaryWriter.Write(zone.ChunksY);

            //Ecos
            offsets["ecos"] = (uint) binaryWriter.BaseStream.Position;
            binaryWriter.Write((uint) zone.Ecos.Count);

            foreach (Eco eco in zone.Ecos)
            {
                eco.WriteToStream(binaryWriter);
            }

            //Floras
            offsets["floras"] = (uint) binaryWriter.BaseStream.Position;
            binaryWriter.Write((uint) zone.Floras.Count);

            foreach (Flora flora in zone.Floras)
            {
                flora.WriteToStream(binaryWriter);
            }

            offsets["invisibleWalls"] = (uint) binaryWriter.BaseStream.Position;
            binaryWriter.Write((uint) zone.InvisibleWalls.Count);

            foreach (InvisibleWall invisibleWall in zone.InvisibleWalls)
            {
                invisibleWall.WriteToStream(binaryWriter);
            }

            offsets["objects"] = (uint) binaryWriter.BaseStream.Position;
            binaryWriter.Write((uint) zone.Objects.Count);

            foreach (Object obj in zone.Objects)
            {
                obj.WriteToStream(binaryWriter);
            }

            offsets["lights"] = (uint) binaryWriter.BaseStream.Position;
            binaryWriter.Write((uint) zone.Lights.Count);

            foreach (Light light in zone.Lights)
            {
                light.WriteToStream(binaryWriter);
            }

            offsets["unknowns"] = (uint) binaryWriter.BaseStream.Position;
            binaryWriter.Write((uint) zone.Unknowns.Count);

            //???
            //foreach (Unknown unknown in zone.Unknowns)
            //{

            //}

            //Update offset values.
            binaryWriter.BaseStream.Seek(offsetsPosition, SeekOrigin.Begin);
            binaryWriter.Write(offsets["ecos"]);
            binaryWriter.Write(offsets["floras"]);
            binaryWriter.Write(offsets["invisibleWalls"]);
            binaryWriter.Write(offsets["objects"]);
            binaryWriter.Write(offsets["lights"]);
            binaryWriter.Write(offsets["unknowns"]);
        }
    }
}
