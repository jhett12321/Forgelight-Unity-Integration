namespace ForgelightUnity.Forgelight.Assets.Dma
{
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;

    public static class Dma
    {
        public static void LoadFromStream(Stream stream, ICollection<string> textures, ICollection<Material> materials)
        {
            if (stream == null || textures == null || materials == null)
                return;

            BinaryReader binaryReader = new BinaryReader(stream);

            //header
            char[] magic = binaryReader.ReadChars(4);

            if (magic[0] != 'D' ||
                magic[1] != 'M' ||
                magic[2] != 'A' ||
                magic[3] != 'T')
            {
                return;
            }

            uint version = binaryReader.ReadUInt32();

            if (version != 1)
            {
                Debug.LogWarning("Could not decode dma. Unknown dma version " + version);
                return;
            }

            //textures
            uint texturesLength = binaryReader.ReadUInt32();
            char[] buffer = binaryReader.ReadChars((int) texturesLength);
            int startIndex = 0;

            for (int i = 0; i < buffer.Length; ++i)
            {
                if (buffer[i] == '\0')
                {
                    int length = i - startIndex;

                    string textureName = new string(buffer, startIndex, length);
                    startIndex = i + 1;

                    textures.Add(textureName);
                }
            }

            //materials
            uint materialCount = binaryReader.ReadUInt32();

            for (int i = 0; i < materialCount; ++i)
            {
                Material material = Material.LoadFromStream(stream);
                materials.Add(material);
            }
        }
    }
}
