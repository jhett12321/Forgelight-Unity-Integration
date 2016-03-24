using System;
using System.Collections.Generic;
using System.IO;
using Forgelight.Utils.Cryptography;
using UnityEngine;
using Material = Forgelight.Formats.Dma.Material;

namespace Forgelight.Formats.Dme
{
    enum TextureType
    {
        Invalid,
        Diffuse,
        Bump,
        Spec
    }

    public class Model
    {
        public uint Version { get; private set; }
        public uint ModelHeaderOffset { get; private set; }

        public string Name { get; private set; }

        public List<Material> Materials { get; private set; }

        //Bounding Box
        private Vector3 min;
        public Vector3 Min { get { return min; } }
        private Vector3 max;
        public Vector3 Max { get { return max; } }

        public List<Mesh> Meshes { get; private set; }
        public List<string> TextureStrings { get; private set; }
        public List<BoneMap> BoneMaps { get; private set; }

        public static Model LoadFromStream(string name, Stream stream)
        {
            BinaryReader binaryReader = new BinaryReader(stream);

            //header
            byte[] magic = binaryReader.ReadBytes(4);

            if (magic[0] != 'D' ||
                magic[1] != 'M' ||
                magic[2] != 'O' ||
                magic[3] != 'D')
            {
                return null;
            }
            Model model = new Model();

            model.Version = binaryReader.ReadUInt32();

            if (model.Version != 4)
            {
                return null;
            }

            model.ModelHeaderOffset = binaryReader.ReadUInt32();

            model.Name = name;

            //materials
            model.TextureStrings = new List<string>();
            model.Materials = new List<Material>();
            Dma.Dma.LoadFromStream(binaryReader.BaseStream, model.TextureStrings, model.Materials);

            //Bounding Box
            model.min.x = binaryReader.ReadSingle();
            model.min.y = binaryReader.ReadSingle();
            model.min.z = binaryReader.ReadSingle();

            model.max.x = binaryReader.ReadSingle();
            model.max.y = binaryReader.ReadSingle();
            model.max.z = binaryReader.ReadSingle();

            //meshes
            uint meshCount = binaryReader.ReadUInt32();

            model.Meshes = new List<Mesh>((int) meshCount);

            for (int i = 0; i < meshCount; ++i)
            {
                Mesh mesh = Mesh.LoadFromStream(binaryReader.BaseStream, model.Materials);

                if (mesh == null)
                {
                    continue;
                }

                //Textures
                Material material = model.Materials[(int) mesh.MaterialIndex];
                foreach (Material.Parameter parameter in material.Parameters)
                {
                    LookupTexture(mesh, parameter, model.TextureStrings);

                    if (mesh.BaseDiffuse != null && mesh.BumpMap != null && mesh.SpecMap != null)
                    {
                        break;
                    }
                }

                model.Meshes.Add(mesh);
            }

            //bone maps
            uint boneMapCount = binaryReader.ReadUInt32();
            model.BoneMaps = new List<BoneMap>((int) boneMapCount);

            for (int i = 0; i < boneMapCount; ++i)
            {
                BoneMap boneMap = BoneMap.LoadFromStream(binaryReader.BaseStream);

                if (boneMap != null)
                {
                    model.BoneMaps.Add(boneMap);
                }
            }

            //bone map entries
            //uint boneMapEntryCount = binaryReader.ReadUInt32();
            //BoneMapEntry[] boneMapEntries = new BoneMapEntry[boneMapEntryCount];

            //for (int i = 0; i < boneMapEntryCount; ++i)
            //{
            //    BoneMapEntry boneMapEntry = BoneMapEntry.LoadFromStream(binaryReader.BaseStream);

            //    boneMapEntries[i] = boneMapEntry;
            //}

            return model;
        }

        private static void LookupTexture(Mesh mesh, Material.Parameter paramater, List<string> textureStrings)
        {
            if (paramater.Data.Length != 4 && paramater.Type != Material.Parameter.D3DXParameterType.Texture ||
                paramater.Class != Material.Parameter.D3DXParameterClass.Object)
            {
                return;
            }

            TextureType textureType = GetTextureType(paramater.NameHash);

            if (textureType == TextureType.Invalid)
            {
                return;
            }

            uint textureHash = BitConverter.ToUInt32(paramater.Data, 0);

            foreach (string textureString in textureStrings)
            {
                if (Jenkins.OneAtATime(textureString.ToUpper()) == textureHash)
                {
                    switch (textureType)
                    {
                        case TextureType.Diffuse:
                            mesh.BaseDiffuse = textureString;
                            break;
                        case TextureType.Bump:
                            mesh.BumpMap = textureString;
                            break;
                        case TextureType.Spec:
                            mesh.SpecMap = textureString;
                            break;
                    }

                    return;
                }
            }
        }

        private static TextureType GetTextureType(uint parameterHash)
        {
            uint baseDiffuseHash = Jenkins.OneAtATime("baseDiffuse");
            uint BaseDiffuseHash = Jenkins.OneAtATime("BaseDiffuse");
            uint basediffuseHash = Jenkins.OneAtATime("basediffuse");

            uint SpecHash = Jenkins.OneAtATime("Spec");
            uint specHash = Jenkins.OneAtATime("spec");

            uint BumpHash = Jenkins.OneAtATime("Bump");
            uint BumpMapHash = Jenkins.OneAtATime("BumpMap");

            if (parameterHash == baseDiffuseHash || parameterHash == BaseDiffuseHash || parameterHash == basediffuseHash)
            {
                return TextureType.Diffuse;
            }

            if (parameterHash == SpecHash || parameterHash == specHash)
            {
                return TextureType.Spec;
            }

            if (parameterHash == BumpHash || parameterHash == BumpMapHash)
            {
                return TextureType.Bump;
            }

            return TextureType.Invalid;
        }
    }
}
