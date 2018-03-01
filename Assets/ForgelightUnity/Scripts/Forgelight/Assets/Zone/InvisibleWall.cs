namespace ForgelightUnity.Forgelight.Assets.Zone
{
    using System.IO;

    public class InvisibleWall
    {
        #region Structure
        public uint UnknownUInt32 { get; private set; }
        public float UnknownFloat1 { get; private set; }
        public float UnknownFloat2 { get; private set; }
        public float UnknownFloat3 { get; private set; }
        #endregion

        public static InvisibleWall ReadFromStream(Stream stream)
        {
            InvisibleWall invisibleWall = new InvisibleWall();
            BinaryReader binaryReader = new BinaryReader(stream);

            invisibleWall.UnknownUInt32 = binaryReader.ReadUInt32();
            invisibleWall.UnknownFloat1 = binaryReader.ReadSingle();
            invisibleWall.UnknownFloat2 = binaryReader.ReadSingle();
            invisibleWall.UnknownFloat3 = binaryReader.ReadSingle();

            return invisibleWall;
        }

        public void WriteToStream(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(UnknownUInt32);
            binaryWriter.Write(UnknownFloat1);
            binaryWriter.Write(UnknownFloat2);
            binaryWriter.Write(UnknownFloat3);
        }
    }
}
