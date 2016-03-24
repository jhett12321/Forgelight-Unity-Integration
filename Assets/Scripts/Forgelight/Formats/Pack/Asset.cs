using System;
using System.ComponentModel;
using System.IO;
using Forgelight.Utils;
using UnityEngine;

namespace Forgelight.Pack
{
    public class Asset
    {
        public enum Types
        {
            ADR,
            AGR,
            CDT,
            CNK0,
            CNK1,
            CNK2,
            CNK3,
            CRC,
            DDS,
            DMA,
            DME,
            DMV,
            DSK,
            ECO,
            FSB,
            FXO,
            GFX,
            LST,
            NSA,
            TXT,
            XML,
            ZONE,
            Unknown
        }

        private Asset(Pack pack)
        {
            Pack = pack;
            Name = string.Empty;
            Size = 0;
            AbsoluteOffset = 0;
            Type = Types.Unknown;
        }

        public static Asset LoadBinary(Pack pack, Stream stream)
        {
            BinaryReaderBigEndian reader = new BinaryReaderBigEndian(stream);

            Asset asset = new Asset(pack);

            uint count = reader.ReadUInt32();
            asset.Name = new string(reader.ReadChars((int) count));
            asset.AbsoluteOffset = reader.ReadUInt32();
            asset.Size = reader.ReadUInt32();
            asset.Crc32 = reader.ReadUInt32();

            // Set the type of the asset based on the extension
            {
                // First get the extension without the leading '.'
                string extension = Path.GetExtension(asset.Name).Substring(1);
                try
                {
                    asset.Type = (Types) Enum.Parse(typeof (Types), extension, true);
                }
                catch (ArgumentException)
                {
                    // This extension isn't mapped in the enum
                    Debug.LogWarning("Unknown Forgelight File Type: " + extension);
                    asset.Type = Types.Unknown;
                }
            }

            return asset;
        }

        public override string ToString()
        {
            return Name;
        }

        [Browsable(false)]
        public Pack Pack { get; private set; }

        public string Name { get; private set; }
        public uint Size { get; private set; }
        public uint AbsoluteOffset { get; private set; }
        public uint Crc32 { get; private set; }

        public Types Type { get; private set; }
    }
}