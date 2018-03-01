namespace ForgelightUnity.Forgelight.Utils
{
    using System.IO;
    using System.Text;

    public static class BinaryReaderUtils
    {
        public static string ReadNullTerminatedString(this BinaryReader binaryReader)
        {
            string str = "";
            char ch;
            while ((ch = binaryReader.ReadChar()) != 0)
            {
                str = str + ch;
            }

            return str;
        }

        public static void WriteNullTerminiatedString(this BinaryWriter binaryWriter, string value)
        {
            byte[] buffer = Encoding.Default.GetBytes(value);
            binaryWriter.Write(buffer);
            binaryWriter.Write((byte)0);
        }
    }
}
