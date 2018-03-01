namespace ForgelightUnity.Forgelight.Assets.Zone
{
    using System.IO;
    using UnityEngine;
    using Utils;

    public class Decal
    {
        #region Structure
        public float UnknownFloat1 { get; private set; }
        public Vector4 Position { get; private set; }
        public float UnknownFloat2 { get; private set; }
        public float UnknownFloat3 { get; private set; }
        public float UnknownFloat4 { get; private set; }
        public float UnknownFloat5 { get; private set; }
        public uint DecimalDigits6And4 { get; private set; } //Shaql: I mean, uh, the last 4 digits in decimal seem to be similar or same for several values, thus probably have some significance
        public string Name { get; private set; }
        public float UnknownFloat6 { get; private set; }
        public float UnknownFloat7 { get; private set; }
        public float UnknownFloat8 { get; private set; }
        public uint UnknownUInt { get; private set; }
        #endregion

        public static Decal ReadFromStream(Stream stream)
        {
            Decal decal = new Decal();
            BinaryReader binaryReader = new BinaryReader(stream);

            decal.UnknownFloat1 = binaryReader.ReadSingle();
            decal.Position = new Vector4(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
            decal.UnknownFloat2 = binaryReader.ReadSingle();
            decal.UnknownFloat3 = binaryReader.ReadSingle();
            decal.UnknownFloat4 = binaryReader.ReadSingle();
            decal.UnknownFloat5 = binaryReader.ReadSingle();
            decal.DecimalDigits6And4 = binaryReader.ReadUInt32();
            decal.Name = binaryReader.ReadNullTerminatedString();
            decal.UnknownFloat6 = binaryReader.ReadSingle();
            decal.UnknownFloat7 = binaryReader.ReadSingle();
            decal.UnknownFloat8 = binaryReader.ReadSingle();
            decal.UnknownUInt = binaryReader.ReadUInt32();

            return decal;
        }

        public void WriteToStream(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(UnknownFloat1);
            binaryWriter.Write(Position.x);
            binaryWriter.Write(Position.y);
            binaryWriter.Write(Position.z);
            binaryWriter.Write(Position.w);
            binaryWriter.Write(UnknownFloat2);
            binaryWriter.Write(UnknownFloat3);
            binaryWriter.Write(UnknownFloat4);
            binaryWriter.Write(UnknownFloat5);
            binaryWriter.Write(DecimalDigits6And4);
            binaryWriter.WriteNullTerminiatedString(Name);
            binaryWriter.Write(UnknownFloat6);
            binaryWriter.Write(UnknownFloat7);
            binaryWriter.Write(UnknownFloat8);
            binaryWriter.Write(UnknownUInt);
        }
    }
}
