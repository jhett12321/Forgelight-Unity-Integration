using System.IO;

namespace Assets.Scripts.Forgelight.Utils
{
    public static class BinaryReaderUtils
    {
        public static string ReadNullTerminatedString(this BinaryReader binaryReader)
        {
            string str = "";
            char ch;
            while ((int) (ch = binaryReader.ReadChar()) != 0)
            {
                str = str + ch;
            }

            return str;
        }

    }
}
