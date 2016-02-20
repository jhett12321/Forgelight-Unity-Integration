using System;
using System.ComponentModel;
using System.IO;
using Forgelight.Utils;

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
        };

        private Asset(Pack pack)
        {
            Pack = pack;
            Name = String.Empty;
            Size = 0;
            AbsoluteOffset = 0;
            Type = Types.Unknown;
        }

        public static Asset LoadBinary(Pack pack, Stream stream)
        {
            BinaryReaderBigEndian reader = new BinaryReaderBigEndian(stream);

            Asset asset = new Asset(pack);

            UInt32 count = reader.ReadUInt32();
            asset.Name = new String(reader.ReadChars((Int32) count));
            asset.AbsoluteOffset = reader.ReadUInt32();
            asset.Size = reader.ReadUInt32();
            asset.Crc32 = reader.ReadUInt32();

            // Set the type of the asset based on the extension
            {
                // First get the extension without the leading '.'
                string extension = Path.GetExtension(asset.Name).Substring(1);
                try
                {
                    asset.Type = (Asset.Types) Enum.Parse(typeof (Types), extension, true);
                }
                catch (ArgumentException exception)
                {
                    // This extension isn't mapped in the enum
                    System.Diagnostics.Debug.Write(exception.ToString());
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

        public String Name { get; private set; }
        public UInt32 Size { get; private set; }
        public UInt32 AbsoluteOffset { get; private set; }
        public UInt32 Crc32 { get; private set; }

        public Asset.Types Type { get; private set; }
    }
}