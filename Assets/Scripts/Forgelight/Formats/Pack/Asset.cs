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
            ADR,    //Actor Definition - https://github.com/psemu/ps2ls/wiki/Adr
            CNK0,   //Terrain Data - https://github.com/psemu/ps2ls/wiki/CNK0-and-CNK1-to-CNKn
            CNK1,   //Terrain Data - https://github.com/psemu/ps2ls/wiki/CNK0-and-CNK1-to-CNKn
            CNK2,   //Terrain Data - https://github.com/psemu/ps2ls/wiki/CNK0-and-CNK1-to-CNKn
            CNK3,   //Terrain Data - https://github.com/psemu/ps2ls/wiki/CNK0-and-CNK1-to-CNKn
            CNK4,   //Terrain Data - https://github.com/psemu/ps2ls/wiki/CNK0-and-CNK1-to-CNKn
            CNK5,   //Terrain Data - https://github.com/psemu/ps2ls/wiki/CNK0-and-CNK1-to-CNKn
            DDS,    //Texture Format
            PNG,    //Image Format
            JPG,    //Image Format
            TGA,    //Image Format
            DMA,    //Material Definition - https://github.com/psemu/ps2ls/wiki/Dma
            DME,    //Mesh Data - https://github.com/psemu/ps2ls/wiki/Dme
            DMV,    //Mesh Data/Occlusion - https://github.com/psemu/ps2ls/wiki/Dmv
            ECO,    //Environment clutter, flora, etc. https://github.com/psemu/ps2ls/wiki/ECO
            FSB,    //FMod Sound Banks - https://github.com/psemu/ps2ls/wiki/FSB
            WAV,    //Audio Format
            FXO,    //Compiled DX Shaders.
            GFX,    //Scaleform - https://github.com/psemu/ps2ls/wiki/Gfx
            LST,    //Scaleform, referenced by GFX - https://github.com/psemu/ps2ls/wiki/Lst
            NSA,    //Morpheme animation file - https://github.com/psemu/ps2ls/wiki/Nsa
            TXT,    //Text file.
            INI,    //Text/Configuration File.
            XML,    //XML Document.
            ZONE,   //Object, Light and other placement data - https://github.com/psemu/ps2ls/wiki/Zone
            AGR,    //Model Group
            CDT,    //Collision (non-vehicle)
            CRC,    //Scaleform
            DSK,
            TOME,   //Occlusion
            DEF,    //File name is Color?
            FXD,
            AGS,
            APX,    //Collision (dynamic)
            MRN,    //Animations
            PSSB,   //Shader?
            PRSB,   //Shader?
            APB,    //Apex databases
            VNFO,   //Occlusion
            DB,
            XSSB,   //Shader?
            XRSB,   //Shader?
            PLAYERSTUDIO,
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