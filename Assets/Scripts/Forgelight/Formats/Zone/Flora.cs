using System;
using System.IO;
using Assets.Scripts.Forgelight.Utils;

namespace Forgelight.Formats.Zone
{
    public class Flora
    {
        public string Name { get; private set; }
        public string Texture { get; private set; }
        public string Model { get; private set; }
        public bool UnknownBoolean1 { get; private set; }
        public float UnknownFloat1 { get; private set; }
        public float UnknownFloat2 { get; private set; }

        public static Flora ReadFromStream(Stream stream)
        {
            Flora flora = new Flora();
            BinaryReader binaryReader = new BinaryReader(stream);

            flora.Name = binaryReader.ReadNullTerminatedString();
            flora.Texture = binaryReader.ReadNullTerminatedString();
            flora.Model = binaryReader.ReadNullTerminatedString();
            flora.UnknownBoolean1 = binaryReader.ReadBoolean();
            flora.UnknownFloat1 = binaryReader.ReadSingle();
            flora.UnknownFloat2 = binaryReader.ReadSingle();

            return flora;
        }

        public void WriteToStream(BinaryWriter binaryWriter)
        {
            binaryWriter.WriteNullTerminiatedString(Name);
            binaryWriter.WriteNullTerminiatedString(Texture);
            binaryWriter.WriteNullTerminiatedString(Model);

            binaryWriter.Write(UnknownBoolean1);
            binaryWriter.Write(UnknownFloat1);
            binaryWriter.Write(UnknownFloat2);
        }
    }
}
