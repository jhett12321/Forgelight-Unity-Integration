using System.IO;
using Assets.Scripts.Forgelight.Utils;
using UnityEngine;

namespace Forgelight.Formats.Zone
{
    public class Light
    {
        public string Name { get; private set; }
        public string ColorName { get; private set; }
        public byte Type { get; private set; }
        public float UnknownFloat1 { get; private set; }
        public Vector4 Position { get; private set; }
        public Vector4 Rotation { get; private set; }
        public float Range { get; private set; }
        public float InnerRange { get; private set; }
        public Color Color { get; private set; }
        public byte UnknownByte1 { get; private set; }
        public byte UnknownByte2 { get; private set; }
        public byte UnknownByte3 { get; private set; }
        public byte UnknownByte4 { get; private set; }
        public byte UnknownByte5 { get; private set; }
        public Vector4 UnknownVector1 { get; private set; }
        public string UnknownString1 { get; private set; }
        public uint ID { get; private set; }

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

            byte a = binaryReader.ReadByte();
            byte r = binaryReader.ReadByte();
            byte g = binaryReader.ReadByte();
            byte b = binaryReader.ReadByte();

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

        public void WriteToStream(BinaryWriter binaryWriter)
        {
            binaryWriter.WriteNullTerminiatedString(Name);
            binaryWriter.WriteNullTerminiatedString(ColorName);
            binaryWriter.Write(Type);
            binaryWriter.Write(UnknownFloat1);

            binaryWriter.Write(Position.x);
            binaryWriter.Write(Position.y);
            binaryWriter.Write(Position.z);
            binaryWriter.Write(Position.w);

            binaryWriter.Write(Rotation.x);
            binaryWriter.Write(Rotation.y);
            binaryWriter.Write(Rotation.z);
            binaryWriter.Write(Rotation.w);

            binaryWriter.Write(Range);
            binaryWriter.Write(InnerRange);

            binaryWriter.Write((byte)(Color.a * 255));
            binaryWriter.Write((byte)(Color.r * 255));
            binaryWriter.Write((byte)(Color.g * 255));
            binaryWriter.Write((byte)(Color.b * 255));

            binaryWriter.Write(UnknownByte1);
            binaryWriter.Write(UnknownByte2);
            binaryWriter.Write(UnknownByte3);
            binaryWriter.Write(UnknownByte4);
            binaryWriter.Write(UnknownByte5);

            binaryWriter.Write(UnknownVector1.x);
            binaryWriter.Write(UnknownVector1.y);
            binaryWriter.Write(UnknownVector1.z);
            binaryWriter.Write(UnknownVector1.w);

            binaryWriter.WriteNullTerminiatedString(UnknownString1);
            binaryWriter.Write(ID);
        }
    }
}
