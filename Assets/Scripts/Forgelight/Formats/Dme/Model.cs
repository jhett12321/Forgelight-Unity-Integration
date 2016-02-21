using System;
using System.Collections.Generic;
using System.IO;
using Forgelight.Utils.Cryptography;
using UnityEngine;

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
        public UInt32 Version { get; private set; }
        public String Name { get; private set; }

        public List<Dma.Material> Materials { get; private set; }

        //Bounding Box
        private Vector3 min;
        public Vector3 Min { get { return min; } }
        private Vector3 max;
        public Vector3 Max { get { return max; } }

        public Mesh[] Meshes { get; private set; }
        public List<String> TextureStrings { get; private set; }
        public BoneMap[] BoneMaps { get; private set; }

        public static Model LoadFromStream(String name, Stream stream)
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

            UInt32 modelHeaderOffset = binaryReader.ReadUInt32();

            model.Name = name;

            //materials
            model.TextureStrings = new List<String>();
            model.Materials = new List<Dma.Material>();
            Dma.Dma.LoadFromStream(binaryReader.BaseStream, model.TextureStrings, model.Materials);

            //Bounding Box
            model.min.x = binaryReader.ReadSingle();
            model.min.y = binaryReader.ReadSingle();
            model.min.z = binaryReader.ReadSingle();

            model.max.x = binaryReader.ReadSingle();
            model.max.y = binaryReader.ReadSingle();
            model.max.z = binaryReader.ReadSingle();

            //meshes
            UInt32 meshCount = binaryReader.ReadUInt32();

            model.Meshes = new Mesh[meshCount];

            for (Int32 i = 0; i < meshCount; ++i)
            {
                Mesh mesh = Mesh.LoadFromStream(binaryReader.BaseStream, model.Materials);

                if (mesh != null)
                {
                    model.Meshes[i] = mesh;
                }

                //Textures
                Dma.Material material = model.Materials[(int) mesh.MaterialIndex];
                foreach (Dma.Material.Parameter parameter in material.Parameters)
                {
                    LookupTexture(mesh, parameter, model.TextureStrings);

                    if (mesh.BaseDiffuse != null && mesh.BumpMap != null && mesh.SpecMap != null)
                    {
                        break;
                    }
                }
            }

            //bone maps
            UInt32 boneMapCount = binaryReader.ReadUInt32();

            model.BoneMaps = new BoneMap[boneMapCount];

            for (Int32 i = 0; i < boneMapCount; ++i)
            {
                BoneMap boneMap = BoneMap.LoadFromStream(binaryReader.BaseStream);

                if (boneMap != null)
                {
                    model.BoneMaps[i] = boneMap;
                }
            }

            //bone map entries
            UInt32 boneMapEntryCount = binaryReader.ReadUInt32();

            BoneMapEntry[] boneMapEntries = new BoneMapEntry[boneMapEntryCount];

            for (Int32 i = 0; i < boneMapEntryCount; ++i)
            {
                BoneMapEntry boneMapEntry = BoneMapEntry.LoadFromStream(binaryReader.BaseStream);

                boneMapEntries[i] = boneMapEntry;
            }

            return model;
        }

        private static void LookupTexture(Mesh mesh, Dma.Material.Parameter paramater, List<String> textureStrings)
        {
            if (paramater.Data.Length != 4 && paramater.Type != Dma.Material.Parameter.D3DXParameterType.Texture ||
                paramater.Class != Dma.Material.Parameter.D3DXParameterClass.Object)
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
