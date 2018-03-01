namespace ForgelightUnity.Forgelight.Assets.Zone
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;
    using Utils;

    public class Eco
    {
        #region Structure
        public uint Index { get; private set; }
        public string Name { get; private set; }
        public string ColorNXMap { get; private set; }
        public string SpecBlendNyMap { get; private set; }
        public uint DetailRepeat { get; private set; }
        public float BlendStrength { get; private set; }
        public float SpecMin { get; private set; }
        public float SpecMax { get; private set; }
        public float SpecSmoothnessMin { get; private set; }
        public float SpecSmoothnessMax { get; private set; }
        public string PhysicsMaterial { get; private set; }
        public List<Layer> Layers { get; private set; }
        public class Layer
        {
            public class Tint
            {
                public Color Color { get; set; }
                public uint Percentage { get; set; }
            }

            public float Density { get; set; }
            public float MinScale { get; set; }
            public float MaxScale { get; set; }
            public float SlopePeak { get; set; }
            public float SlopeExtent { get; set; }
            public float MinElevation { get; set; }
            public float MaxElevation { get; set; }
            public byte MinAlpha { get; set; }
            public string Flora { get; set; }
            public List<Tint> Tints { get; set; }
        }
        #endregion

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
            uint layerCount = binaryReader.ReadUInt32();

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
                uint tintCount = binaryReader.ReadUInt32();

                for (uint j = 0; j < tintCount; j++)
                {
                    Layer.Tint tint = new Layer.Tint();

                    byte r = binaryReader.ReadByte();
                    byte g = binaryReader.ReadByte();
                    byte b = binaryReader.ReadByte();
                    byte a = binaryReader.ReadByte();

                    tint.Color = new Color(r/255.0f, g/255.0f, b/255.0f, a/255.0f);

                    tint.Percentage = binaryReader.ReadUInt32();

                    layer.Tints.Add(tint);
                }

                eco.Layers.Add(layer);
            }

            return eco;
        }

        public void WriteToStream(BinaryWriter binaryWriter)
        {
            //Eco
            binaryWriter.Write(Index);

            binaryWriter.WriteNullTerminiatedString(Name);
            binaryWriter.WriteNullTerminiatedString(ColorNXMap);
            binaryWriter.WriteNullTerminiatedString(SpecBlendNyMap);
            binaryWriter.Write(DetailRepeat);
            binaryWriter.Write(BlendStrength);
            binaryWriter.Write(SpecMin);
            binaryWriter.Write(SpecMax);
            binaryWriter.Write(SpecSmoothnessMin);
            binaryWriter.Write(SpecSmoothnessMax);
            binaryWriter.WriteNullTerminiatedString(PhysicsMaterial);

            //Layers
            binaryWriter.Write((uint) Layers.Count);

            foreach (Layer layer in Layers)
            {
                binaryWriter.Write(layer.Density);
                binaryWriter.Write(layer.MinScale);
                binaryWriter.Write(layer.MaxScale);
                binaryWriter.Write(layer.SlopePeak);
                binaryWriter.Write(layer.SlopeExtent);
                binaryWriter.Write(layer.MinElevation);
                binaryWriter.Write(layer.MaxElevation);
                binaryWriter.Write(layer.MinAlpha);
                binaryWriter.WriteNullTerminiatedString(layer.Flora);

                //Tints
                binaryWriter.Write((uint) layer.Tints.Count);

                foreach (Layer.Tint tint in layer.Tints)
                {
                    binaryWriter.Write(Convert.ToByte(tint.Color.r * 255));
                    binaryWriter.Write(Convert.ToByte(tint.Color.g * 255));
                    binaryWriter.Write(Convert.ToByte(tint.Color.b * 255));
                    binaryWriter.Write(Convert.ToByte(tint.Color.a * 255));

                    binaryWriter.Write(tint.Percentage);
                }
            }
        }
    }
}
