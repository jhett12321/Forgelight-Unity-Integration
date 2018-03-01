namespace ForgelightUnity.Forgelight.Assets.Dme
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Dma;
    using UnityEngine;
    using Utils.Cryptography;
    using Material = Dma.Material;

    public class Model : Asset
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

        public static Model LoadFromStream(string name, string displayName, Stream stream)
        {
            BinaryReader binaryReader = new BinaryReader(stream);

            //Header
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

            if (!Enum.IsDefined(typeof(ModelType), (int)model.Version))
            {
                Debug.LogWarning("Could not decode model " + name + ". Unknown DME version " + model.Version);
                return null;
            }

            model.ModelType = (ModelType)model.Version;

            model.Name = name;
            model.DisplayName = displayName;

            model.ModelHeaderOffset = binaryReader.ReadUInt32();

            //DMA
            model.TextureStrings = new List<string>();
            model.Materials = new List<Material>();
            Dma.LoadFromStream(binaryReader.BaseStream, model.TextureStrings, model.Materials);

            //Bounding Box
            model.Min = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
            model.Max = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());

            //Meshes
            uint meshCount = binaryReader.ReadUInt32();

            model.Meshes = new List<Mesh>((int) meshCount);

            for (int i = 0; i < meshCount; ++i)
            {
                Mesh mesh = Mesh.LoadFromStream(binaryReader.BaseStream, model.Materials);

                if (mesh == null)
                {
                    continue;
                }

                Material material = model.Materials[(int) mesh.MaterialIndex];
                foreach (Material.Parameter parameter in material.Parameters)
                {
                    LookupTextures(mesh, parameter, model.TextureStrings);

                    if (mesh.BaseDiffuse != null && mesh.BumpMap != null && mesh.SpecMap != null)
                    {
                        break;
                    }
                }

                model.Meshes.Add(mesh);
            }

            //Bone Maps
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

            //Bone Map Entries
            uint boneMapEntryCount = binaryReader.ReadUInt32();
            model.BoneMapEntries = new List<BoneMapEntry>((int) boneMapEntryCount);

            for (int i = 0; i < boneMapEntryCount; ++i)
            {
                BoneMapEntry boneMapEntry = BoneMapEntry.LoadFromStream(binaryReader.BaseStream);

                if (boneMapEntry != null)
                {
                    model.BoneMapEntries.Add(boneMapEntry);
                }
            }

            return model;
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
