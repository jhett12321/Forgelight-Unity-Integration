namespace ForgelightUnity.Forgelight.Assets.Zone
{
    using System.IO;
    using UnityEngine;
    using Utils;

    public enum LightType
    {
        Pointlight = 1,
        Spotlight = 2,
    }

    public class Light
    {
        #region Structure
        public string Name { get; set; }
        public string ColorName { get; set; }
        public LightType Type { get; set; }
        public float UnknownFloat1 { get; set; }
        public Vector4 Position { get; set; }
        public Vector4 Rotation { get; set; }
        public float Range { get; set; }
        public float InnerRange { get; set; }
        public Color Color { get; set; }
        public byte UnknownByte1 { get; set; }
        public byte UnknownByte2 { get; set; }
        public byte UnknownByte3 { get; set; }
        public byte UnknownByte4 { get; set; }
        public byte UnknownByte5 { get; set; }
        public Vector4 UnknownVector1 { get; set; }
        public string UnknownString1 { get; set; }
        public uint ID { get; set; }
        #endregion

        public static Light ReadFromStream(Stream stream)
        {
            Light light = new Light();
            BinaryReader binaryReader = new BinaryReader(stream);

            light.Name = binaryReader.ReadNullTerminatedString();
            light.ColorName = binaryReader.ReadNullTerminatedString();
            light.Type = (LightType)binaryReader.ReadByte();
            light.UnknownFloat1 = binaryReader.ReadSingle();
            light.Position = new Vector4(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
            light.Rotation = new Vector4(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
            light.Range = binaryReader.ReadSingle();
            light.InnerRange = binaryReader.ReadSingle();

            byte a = binaryReader.ReadByte();
            byte r = binaryReader.ReadByte();
            byte g = binaryReader.ReadByte();
            byte b = binaryReader.ReadByte();

            light.Color = new Color(r/255.0f, g/255.0f, b/255.0f, a/255.0f);
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
            binaryWriter.Write((byte)Type);
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
