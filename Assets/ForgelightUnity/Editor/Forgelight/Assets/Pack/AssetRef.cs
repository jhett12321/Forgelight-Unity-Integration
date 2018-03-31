namespace ForgelightUnity.Editor.Forgelight.Assets.Pack
{
    using System;
    using System.IO;
    using UnityEngine;
    using Syroot.BinaryData;

    public class AssetRef
    {
        public Pack Pack { get; private set; }

        /// <summary>
        /// The name (+extension) of this asset.
        /// </summary>
        public string Name { get; private set; }
        public string DisplayName { get; private set; }
        public uint Size { get; private set; }
        public uint AbsoluteOffset { get; private set; }
        public uint Crc32 { get; private set; }

        public AssetType AssetType { get; private set; }

        private AssetRef(Pack pack)
        {
            Pack = pack;
            Name = string.Empty;
            Size = 0;
            AbsoluteOffset = 0;
            AssetType = AssetType.Unknown;
        }

        public static AssetRef LoadBinary(Pack pack, Stream stream)
        {
            AssetRef assetRef;

            using (BinaryDataReader reader = new BinaryDataReader(stream, true))
            {
                reader.ByteOrder = ByteOrder.BigEndian;
                assetRef = new AssetRef(pack);

                uint count = reader.ReadUInt32();
                assetRef.Name = new string(reader.ReadChars((int) count));
                assetRef.DisplayName = assetRef.Name + " (" + pack.Name + ')';
                assetRef.AbsoluteOffset = reader.ReadUInt32();
                assetRef.Size = reader.ReadUInt32();
                assetRef.Crc32 = reader.ReadUInt32();

                // Set the type of the asset based on the extension
                {
                    // First get the extension without the leading '.'
                    string extension = Path.GetExtension(assetRef.Name).Substring(1);

                    try
                    {
                        assetRef.AssetType = (AssetType) Enum.Parse(typeof (AssetType), extension, true);
                    }
                    catch (ArgumentException)
                    {
                        // This extension isn't mapped in the enum
                        Debug.LogWarning("Unknown Forgelight File Type: " + extension);
                        assetRef.AssetType = AssetType.Unknown;
                    }
                }
            }

            return assetRef;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}