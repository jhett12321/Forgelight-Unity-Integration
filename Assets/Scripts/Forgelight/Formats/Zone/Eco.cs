using System;
using System.IO;
using System.Collections.Generic;
using Assets.Scripts.Forgelight.Utils;
using UnityEngine;

namespace Forgelight.Formats.Zone
{
    public class Eco
    {
        public class Layer
        {
            public class Tint
            {
                public Color Color { get; set; }
                public UInt32 Percentage { get; set; }
            }

            public float Density { get; set; }
            public float MinScale { get; set; }
            public float MaxScale { get; set; }
            public float SlopePeak { get; set; }
            public float SlopeExtent { get; set; }
            public float MinElevation { get; set; }
            public float MaxElevation { get; set; }
            public Byte MinAlpha { get; set; }
            public string Flora { get; set; }
            public List<Tint> Tints { get; set; }
        }

        public UInt32 Index { get; private set; }
        public string Name { get; private set; }
        public string ColorNXMap { get; private set; }
        public string SpecBlendNyMap { get; private set; }
        public UInt32 DetailRepeat { get; private set; }
        public float BlendStrength { get; private set; }
        public float SpecMin { get; private set; }
        public float SpecMax { get; private set; }
        public float SpecSmoothnessMin { get; private set; }
        public float SpecSmoothnessMax { get; private set; }
        public string PhysicsMaterial { get; private set; }
        public List<Layer> Layers { get; private set; }

        public static Eco ReadFromStream(Stream stream)
        {
            Eco eco = new Eco();

            BinaryReader binaryReader = new BinaryReader(stream);

            eco.Index = binaryReader.ReadUInt32();

            eco.Name = binaryReader.ReadNullTerminatedString();
            eco.ColorNXMap = binaryReader.ReadNullTerminatedString();
            eco.SpecBlendNyMap = binaryReader.ReadNullTerminatedString();
            eco.DetailRepeat = binaryReader.ReadUInt32();
            eco.BlendStrength = binaryReader.ReadSingle();
            eco.SpecMin = binaryReader.ReadSingle();
            eco.SpecMax = binaryReader.ReadSingle();
            eco.SpecSmoothnessMin = binaryReader.ReadSingle();
            eco.SpecSmoothnessMax = binaryReader.ReadSingle();
            eco.PhysicsMaterial = binaryReader.ReadNullTerminatedString();

            eco.Layers = new List<Layer>();
            UInt32 layerCount = binaryReader.ReadUInt32();

            for (uint i = 0; i < layerCount; i++)
            {
                Layer layer = new Layer();
                layer.Density = binaryReader.ReadSingle();
                layer.MinScale = binaryReader.ReadSingle();
                layer.MaxScale = binaryReader.ReadSingle();
                layer.SlopePeak = binaryReader.ReadSingle();
                layer.SlopeExtent = binaryReader.ReadSingle();
                layer.MinElevation = binaryReader.ReadSingle();
                layer.MaxElevation = binaryReader.ReadSingle();
                layer.MinAlpha = binaryReader.ReadByte();
                layer.Flora = binaryReader.ReadNullTerminatedString();

                layer.Tints = new List<Layer.Tint>();
                UInt32 tintCount = binaryReader.ReadUInt32();

                for (uint j = 0; j < tintCount; j++)
                {
                    Layer.Tint tint = new Layer.Tint();

                    Byte r = binaryReader.ReadByte();
                    Byte g = binaryReader.ReadByte();
                    Byte b = binaryReader.ReadByte();
                    Byte a = binaryReader.ReadByte();

					tint.Color = new Color((float)r/255, (float)g/255, (float)b/255, (float)a/255);

                    tint.Percentage = binaryReader.ReadUInt32();

                    layer.Tints.Add(tint);
                }

                eco.Layers.Add(layer);
            }

            return eco;
        }

    }
}
