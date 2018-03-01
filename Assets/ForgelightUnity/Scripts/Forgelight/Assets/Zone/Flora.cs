using System.IO;
using Assets.Scripts.Forgelight.Utils;

namespace Forgelight.Assets.Zone
{
    public class Flora
    {
        #region Structure
        public string Name { get; private set; }
        public string Texture { get; private set; }
        public string Model { get; private set; }
        public bool UnknownBoolean1 { get; private set; }
        public float UnknownFloat1 { get; private set; }
        public float UnknownFloat2 { get; private set; }
        public float UnknownFloat3 { get; private set; }
        public float UnknownFloat4 { get; private set; }
        public float UnknownFloat5 { get; private set; }

        #endregion

        public static Flora ReadFromStream(Stream stream, ZoneType zoneType)
        {
            Flora flora = new Flora();
            BinaryReader binaryReader = new BinaryReader(stream);

            flora.Name = binaryReader.ReadNullTerminatedString();
            flora.Texture = binaryReader.ReadNullTerminatedString();
            flora.Model = binaryReader.ReadNullTerminatedString();
            flora.UnknownBoolean1 = binaryReader.ReadBoolean();
            flora.UnknownFloat1 = binaryReader.ReadSingle();
            flora.UnknownFloat2 = binaryReader.ReadSingle();

            if (zoneType == ZoneType.H1Z1)
            {
                flora.UnknownFloat3 = binaryReader.ReadSingle();
                flora.UnknownFloat4 = binaryReader.ReadSingle();
                flora.UnknownFloat5 = binaryReader.ReadSingle();
            }

            return flora;
        }

        public void WriteToStream(BinaryWriter binaryWriter, ZoneType zoneType)
        {
            binaryWriter.WriteNullTerminiatedString(Name);
            binaryWriter.WriteNullTerminiatedString(Texture);
            binaryWriter.WriteNullTerminiatedString(Model);

            binaryWriter.Write(UnknownBoolean1);
            binaryWriter.Write(UnknownFloat1);
            binaryWriter.Write(UnknownFloat2);

            if (zoneType == ZoneType.H1Z1)
            {
                binaryWriter.Write(UnknownFloat3);
                binaryWriter.Write(UnknownFloat4);
                binaryWriter.Write(UnknownFloat5);
            }
        }
    }
}
