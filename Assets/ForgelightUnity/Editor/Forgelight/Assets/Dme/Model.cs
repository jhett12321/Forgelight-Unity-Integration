namespace ForgelightUnity.Editor.Forgelight.Assets.Dme
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Dma;
    using UnityEngine;
    using Utils;
    using Utils.Cryptography;
    using Material = Dma.Material;

    public class Model : Asset, IPoolable
    {
        public override string Name { get; protected set; }
        public override string DisplayName { get; protected set; }
        public ModelType ModelType { get; private set; }

        private enum TextureType
        {
            Invalid,
            Diffuse,
            Bump,
            Spec
        }

        #region Structure
        //Header
        public uint Version { get; private set; }
        public uint ModelHeaderOffset { get; private set; }

        //DMA
        public List<string> TextureStrings { get; private set; }
        public List<Material> Materials { get; private set; }

        //Bounding Box
        public Vector3 Min { get; private set; }
        public Vector3 Max { get; private set; }

        //Meshes
        public List<Mesh> Meshes { get; private set; }

        //Bone Maps
        public List<BoneMap> BoneMaps { get; private set; }

        //Bone Map Entries
        public List<BoneMapEntry> BoneMapEntries { get; private set; }
        #endregion

        public Model()
        {
            TextureStrings = new List<string>();
            Materials = new List<Material>();
            Meshes = new List<Mesh>();
            BoneMaps = new List<BoneMap>();
            BoneMapEntries = new List<BoneMapEntry>();
        }

        public void Reset()
        {
            TextureStrings.Clear();
            Materials.Clear();
            Meshes.Clear();
            BoneMaps.Clear();
            BoneMapEntries.Clear();
        }

        public bool InitializeFromStream(string name, string displayName, Stream stream)
        {
            using (BinaryReader binaryReader = new BinaryReader(stream))
            {
                //Header
                byte[] magic = binaryReader.ReadBytes(4);

                if (magic[0] != 'D' ||
                    magic[1] != 'M' ||
                    magic[2] != 'O' ||
                    magic[3] != 'D')
                {
                    return false;
                }

                Version = binaryReader.ReadUInt32();

                if (!Enum.IsDefined(typeof(ModelType), (int)Version))
                {
                    Debug.LogWarning("Could not decode model " + name + ". Unknown DME version " + Version);
                    return false;
                }

                ModelType = (ModelType)Version;

                Name = name;
                DisplayName = displayName;

                ModelHeaderOffset = binaryReader.ReadUInt32();

                //DMA
                Dma.LoadFromStream(binaryReader.BaseStream, TextureStrings, Materials);

                //Bounding Box
                Min = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
                Max = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());

                //Meshes
                uint meshCount = binaryReader.ReadUInt32();

                for (int i = 0; i < meshCount; ++i)
                {
                    Mesh mesh = Mesh.LoadFromStream(binaryReader.BaseStream, Materials);

                    if (mesh == null)
                    {
                        continue;
                    }

                    Material material = Materials[(int) mesh.MaterialIndex];
                    foreach (Material.Parameter parameter in material.Parameters)
                    {
                        LookupTextures(mesh, parameter, TextureStrings);

                        if (mesh.BaseDiffuse != null && mesh.BumpMap != null && mesh.SpecMap != null)
                        {
                            break;
                        }
                    }

                    Meshes.Add(mesh);
                }

                //Bone Maps
                uint boneMapCount = binaryReader.ReadUInt32();

                for (int i = 0; i < boneMapCount; ++i)
                {
                    BoneMap boneMap = BoneMap.LoadFromStream(binaryReader.BaseStream);

                    if (boneMap != null)
                    {
                        BoneMaps.Add(boneMap);
                    }
                }

                //Bone Map Entries
                uint boneMapEntryCount = binaryReader.ReadUInt32();

                for (int i = 0; i < boneMapEntryCount; ++i)
                {
                    BoneMapEntry boneMapEntry = BoneMapEntry.LoadFromStream(binaryReader.BaseStream);

                    if (boneMapEntry != null)
                    {
                        BoneMapEntries.Add(boneMapEntry);
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Finds the correct diffuse, specular and packed normal maps for the given mesh.
        /// </summary>
        /// <param name="mesh">The origin mesh.</param>
        /// <param name="parameter">The material parameter containing the hashed texture name, or some other parameter.</param>
        /// <param name="textureStrings">A list of available textures for this mesh.</param>
        private static void LookupTextures(Mesh mesh, Material.Parameter parameter, List<string> textureStrings)
        {
            if (parameter.Data.Length != 4 && parameter.Type != Material.Parameter.D3DXParameterType.Texture || parameter.Class != Material.Parameter.D3DXParameterClass.Object)
            {
                return;
            }

            TextureType textureType = GetTextureType(parameter.NameHash);

            if (textureType == TextureType.Invalid)
            {
                return;
            }

            uint textureHash = BitConverter.ToUInt32(parameter.Data, 0);

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
