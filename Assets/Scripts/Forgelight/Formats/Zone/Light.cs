using System;
using System.IO;
using Assets.Scripts.Forgelight.Utils;
using UnityEngine;

namespace Forgelight.Formats.Zone
{
    public class Light
    {
        public string Name { get; private set; }
        public string ColorName { get; private set; }
        public Byte Type { get; private set; }
        public float UnknownFloat1 { get; private set; }
        public Vector4 Position { get; private set; }
        public Vector4 Rotation { get; private set; }
        public float Range { get; private set; }
        public float InnerRange { get; private set; }
        public Color Color { get; private set; }
        public Byte UnknownByte1 { get; private set; }
        public Byte UnknownByte2 { get; private set; }
        public Byte UnknownByte3 { get; private set; }
        public Byte UnknownByte4 { get; private set; }
        public Byte UnknownByte5 { get; private set; }
        public Vector4 UnknownVector1 { get; private set; }
        public string UnknownString1 { get; private set; }
        public UInt32 ID { get; private set; }

        public static Light ReadFromStream(Stream stream)
        {
            Light light = new Light();
            BinaryReader binaryReader = new BinaryReader(stream);

            light.Name = binaryReader.ReadNullTerminatedString();
            light.ColorName = binaryReader.ReadNullTerminatedString();
            light.Type = binaryReader.ReadByte();
            light.UnknownFloat1 = binaryReader.ReadSingle();
            light.Position = new Vector4(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
            light.Rotation = new Vector4(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
            light.Range = binaryReader.ReadSingle();
            light.InnerRange = binaryReader.ReadSingle();

            Byte a = binaryReader.ReadByte();
            Byte r = binaryReader.ReadByte();
            Byte g = binaryReader.ReadByte();
            Byte b = binaryReader.ReadByte();

			light.Color = new Color((float)r/255, (float)g/255, (float)b/255, (float)a/255);
            light.UnknownByte1 = binaryReader.ReadByte();
            light.UnknownByte2 = binaryReader.ReadByte();
            light.UnknownByte3 = binaryReader.ReadByte();
            light.UnknownByte4 = binaryReader.ReadByte();
            light.UnknownByte5 = binaryReader.ReadByte();

            light.UnknownVector1 = new Vector4(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
            light.UnknownString1 = binaryReader.ReadNullTerminatedString();
            light.ID = binaryReader.ReadUInt32();

            return light;
        }

    }
}
