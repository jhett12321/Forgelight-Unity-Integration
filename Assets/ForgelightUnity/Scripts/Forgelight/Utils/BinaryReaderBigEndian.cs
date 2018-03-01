namespace ForgelightUnity.Forgelight.Utils
{
    using System;
    using System.IO;

    public class BinaryReaderBigEndian : BinaryReader
    {
        public BinaryReaderBigEndian(Stream stream)
            : base(stream)
        {
        }

        //public override int Read(byte[] buffer, int index, int count)
        //{
        //    return base.Read(buffer, index, count);
        //}

        public override short ReadInt16()
        {
            byte[] bytes = ReadBytes(2);
            Array.Reverse(bytes);
            return BitConverter.ToInt16(bytes, 0);
        }

        public override ushort ReadUInt16()
        {
            byte[] bytes = ReadBytes(2);
            Array.Reverse(bytes);
            return BitConverter.ToUInt16(bytes, 0);
        }

        public override int ReadInt32()
        {
            byte[] bytes = ReadBytes(4);
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        public override uint ReadUInt32()
        {
            byte[] bytes = ReadBytes(4);
            Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public override long ReadInt64()
        {
            byte[] bytes = ReadBytes(8);
            Array.Reverse(bytes);
            return BitConverter.ToInt64(bytes, 0);
        }

        public override ulong ReadUInt64()
        {
            byte[] bytes = ReadBytes(8);
            Array.Reverse(bytes);
            return BitConverter.ToUInt64(bytes, 0);
        }

        public override float ReadSingle()
        {
            byte[] bytes = ReadBytes(4);
            Array.Reverse(bytes);
            return BitConverter.ToSingle(bytes, 0);
        }

        public override double ReadDouble()
        {
            byte[] bytes = ReadBytes(8);
            Array.Reverse(bytes);
            return BitConverter.ToDouble(bytes, 0);
        }
    }
}