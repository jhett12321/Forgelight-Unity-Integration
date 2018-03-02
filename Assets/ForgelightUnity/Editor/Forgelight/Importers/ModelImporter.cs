namespace ForgelightUnity.Editor.Forgelight.Importers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using Assets.Dma;
    using Assets.Dme;
    using Assets.Pack;
    using UnityEngine;
    using Mesh = Assets.Dme.Mesh;

    public class ModelImporter : ForgelightImporter<Model, ModelImporter.ThreadData>
    {
        public class ThreadData
        {
            public StringBuilder StringBuilder;
            public byte[] TextureBuffer;
        }

        private const int MODEL_POOL_SIZE = 1000;

        NumberFormatInfo format = new NumberFormatInfo();

        // Locks
        private object materialLock = new object();
        private object textureLock = new object();

        public ModelImporter() : base(MODEL_POOL_SIZE)
        {
            format.NumberDecimalSeparator = ".";
        }

        protected override string ProgressItemPrefix
        {
            get { return "Exporting Model: "; }
        }

        protected override AssetType AssetType
        {
            get { return AssetType.DME; }
        }

        protected override void Import(AssetRef asset, ThreadData data, object oLock)
        {
            //Don't export if the file already exists.
            if (File.Exists(ResourceDir + "/Models/" + Path.GetFileNameWithoutExtension(asset.Name) + ".obj"))
            {
                return;
            }

            //De-serialize
            using (MemoryStream modelMemoryStream = asset.Pack.CreateAssetMemoryStreamByName(asset.Name))
            {
                Model model = null;
                while (model == null)
                {
                    model = ObjectPool.GetPooledObject();
                }

                bool deserializeResult = model.InitializeFromStream(asset.Name, asset.DisplayName, modelMemoryStream);

                if (deserializeResult)
                {
                    ExportModel(model, data.StringBuilder, ref data.TextureBuffer);
                }

                ObjectPool.ReturnObjectToPool(model);
            }
        }

        /// <summary>
        /// Exports a model to the given directory.
        /// </summary>
        private void ExportModel(Model model, StringBuilder stringBuilder, ref byte[] textureBuffer)
        {
            //TODO: Figure out what to do with non-version 4 models.
            if (model == null || model.Version != 4)
            {
                return;
            }

            string directory = ResourceDir + "/Models";
            string path = directory + @"\" + Path.GetFileNameWithoutExtension(model.Name) + ".obj";

            if (File.Exists(path))
            {
                return;
            }

            // Validate meshes attached to the model
            foreach (Mesh mesh in model.Meshes)
            {
                if (!ForgelightGame.MaterialDefinitionManager.MaterialDefinitions.ContainsKey(model.Materials[(int) mesh.MaterialIndex].MaterialDefinitionHash))
                {
                    return;
                }
            }

            // The texture directory may not exist yet.
            Directory.CreateDirectory(directory + @"\Textures");

            // We reset the string builder so we don't have any previous buffer left over.
            stringBuilder.Length = 0;

            // Materials and Textures
            foreach (Mesh mesh in model.Meshes)
            {
                if (mesh.BaseDiffuse != null)
                {
                    ExportTexture(mesh.BaseDiffuse, directory, ref textureBuffer);
                    ExportMaterial(mesh, directory);
                    stringBuilder.AppendLine("mtllib " + Path.GetFileNameWithoutExtension(mesh.BaseDiffuse) + ".mtl");
                }

                if (mesh.SpecMap != null)
                {
                    ExportTexture(mesh.SpecMap, directory, ref textureBuffer);
                }

                if (mesh.BumpMap != null)
                {
                    ExportTexture(mesh.BumpMap, directory, ref textureBuffer);
                }
            }

            // Meshes
            foreach (Mesh mesh in model.Meshes)
            {
                MaterialDefinition materialDefinition = ForgelightGame.MaterialDefinitionManager.MaterialDefinitions[model.Materials[(int) mesh.MaterialIndex].MaterialDefinitionHash];
                VertexLayout vertexLayout = ForgelightGame.MaterialDefinitionManager.VertexLayouts[materialDefinition.DrawStyles[0].VertexLayoutNameHash];

                //position
                VertexLayout.Entry.DataTypes positionDataType;
                int positionOffset;
                int positionStreamIndex;

                vertexLayout.GetEntryInfoFromDataUsageAndUsageIndex(VertexLayout.Entry.DataUsages.Position, 0, out positionDataType, out positionStreamIndex, out positionOffset);

                Mesh.VertexStream positionStream = mesh.VertexStreams[positionStreamIndex];

                for (int j = 0; j < mesh.VertexCount; ++j)
                {
                    Vector3 position = ReadVector3(positionOffset, positionStream, j);

                    stringBuilder.AppendLine("v " + position.x.ToString(format) + " " +
                                                position.y.ToString(format) + " " + position.z.ToString(format));
                }

                //texture coordinates
                VertexLayout.Entry.DataTypes texCoord0DataType;
                int texCoord0Offset;
                int texCoord0StreamIndex;

                bool texCoord0Present = vertexLayout.GetEntryInfoFromDataUsageAndUsageIndex(
                    VertexLayout.Entry.DataUsages.Texcoord, 0, out texCoord0DataType, out texCoord0StreamIndex,
                    out texCoord0Offset);

                if (texCoord0Present)
                {
                    Mesh.VertexStream texCoord0Stream = mesh.VertexStreams[texCoord0StreamIndex];

                    for (int j = 0; j < mesh.VertexCount; ++j)
                    {
                        Vector2 texCoord;

                        switch (texCoord0DataType)
                        {
                            case VertexLayout.Entry.DataTypes.Float2:
                            {
                                texCoord.x = BitConverter.ToSingle(texCoord0Stream.Data,
                                    (j * texCoord0Stream.BytesPerVertex) + 0);
                                texCoord.y = 1.0f - BitConverter.ToSingle(texCoord0Stream.Data,
                                                    (j * texCoord0Stream.BytesPerVertex) + 4);
                                break;
                            }

                            case VertexLayout.Entry.DataTypes.float16_2:
                            {
                                texCoord.x = Half.FromBytes(texCoord0Stream.Data,
                                    (j * texCoord0Stream.BytesPerVertex) + texCoord0Offset + 0);
                                texCoord.y = 1.0f - Half.FromBytes(texCoord0Stream.Data,
                                                    (j * texCoord0Stream.BytesPerVertex) + texCoord0Offset + 2);
                                break;
                            }

                            default:
                                texCoord.x = 0;
                                texCoord.y = 0;
                                break;
                        }

                        stringBuilder.AppendLine("vt " + texCoord.x.ToString(format) + " " +
                                                    texCoord.y.ToString(format));
                    }
                }
            }

            // Faces
            uint vertexCount = 0;

            for (int i = 0; i < model.Meshes.Count; ++i)
            {
                Mesh mesh = model.Meshes[i];
                stringBuilder.AppendLine("g Mesh" + i);

                // Specify Material
                if (mesh.BaseDiffuse != null)
                {
                    stringBuilder.AppendLine("usemtl " + Path.GetFileNameWithoutExtension(mesh.BaseDiffuse));
                }

                for (int j = 0; j < mesh.IndexCount; j += 3)
                {
                    uint index0, index1, index2;

                    switch (mesh.IndexSize)
                    {
                        case 2:
                            index0 = vertexCount + BitConverter.ToUInt16(mesh.IndexData, (j * 2) + 0) + 1;
                            index1 = vertexCount + BitConverter.ToUInt16(mesh.IndexData, (j * 2) + 2) + 1;
                            index2 = vertexCount + BitConverter.ToUInt16(mesh.IndexData, (j * 2) + 4) + 1;
                            break;
                        case 4:
                            index0 = vertexCount + BitConverter.ToUInt32(mesh.IndexData, (j * 4) + 0) + 1;
                            index1 = vertexCount + BitConverter.ToUInt32(mesh.IndexData, (j * 4) + 4) + 1;
                            index2 = vertexCount + BitConverter.ToUInt32(mesh.IndexData, (j * 4) + 8) + 1;
                            break;
                        default:
                            index0 = 0;
                            index1 = 0;
                            index2 = 0;
                            break;
                    }

                    stringBuilder.AppendLine("f " + index2 + "/" + index2 + "/" + index2 + " " + index1 + "/" +
                                                index1 + "/" + index1 + " " + index0 + "/" + index0 + "/" +
                                                index0);
                }

                vertexCount += mesh.VertexCount;

                File.WriteAllText(path, stringBuilder.ToString());
            }
        }

        private void ExportMaterial(Mesh mesh, string directory)
        {
            if (File.Exists(directory + @"\" + Path.GetFileNameWithoutExtension(mesh.BaseDiffuse) + @".mtl"))
            {
                return;
            }

            lock (materialLock)
            {
                if (File.Exists(directory + @"\" + Path.GetFileNameWithoutExtension(mesh.BaseDiffuse) + @".mtl"))
                {
                    return;
                }

                List<string> mtl = new List<string>();

                string[] baseMtl =
                {
                    "newmtl " + Path.GetFileNameWithoutExtension(mesh.BaseDiffuse),
                    "Ka 1.000000 1.000000 1.000000",
                    "Kd 1.000000 1.000000 1.000000",
                    "Ks 0.000000 0.000000 0.000000",
                    "d 1.0",
                    "illum 2",
                    "map_Ka " + mesh.BaseDiffuse,
                    "map_Kd " + mesh.BaseDiffuse,
                    "map_d " + mesh.BaseDiffuse
                };

                mtl.AddRange(baseMtl);

                if (mesh.SpecMap != null)
                {
                    mtl.Add("map_Ks " + mesh.BaseDiffuse);
                    mtl.Add("map_Ns " + mesh.SpecMap);
                }

                if (mesh.BumpMap != null)
                {
                    mtl.Add("bump " + mesh.BumpMap);
                }

                File.WriteAllLines(ResourceDir + "/Models" + @"\" + Path.GetFileNameWithoutExtension(mesh.BaseDiffuse) + @".mtl", mtl.ToArray());
            }
        }

        private void ExportTexture(string textureString, string directory, ref byte[] textureBuffer)
        {
            if (File.Exists(directory + @"\Textures\" + textureString))
            {
                return;
            }

            lock (textureLock)
            {
                if (File.Exists(directory + @"\Textures\" + textureString))
                {
                    return;
                }

                using (MemoryStream textureMemoryStream = ForgelightGame.CreateAssetMemoryStreamByName(textureString))
                {
                    if (textureMemoryStream == null)
                    {
                        return;
                    }

                    try
                    {
                        if (textureBuffer.Length < textureMemoryStream.Length)
                        {
                            Array.Resize(ref textureBuffer, (int) textureMemoryStream.Length);
                        }

                        textureMemoryStream.Read(textureBuffer, 0, (int) textureMemoryStream.Length);

                        using (FileStream fileStream = new FileStream(directory + @"\Textures\" + textureString, FileMode.Create))
                        {
                            fileStream.Write(textureBuffer, 0, (int) textureMemoryStream.Length);
                        }
                    }
                    catch (IOException)
                    {
                    }
                }
            }
        }

        private static Vector3 ReadVector3(int offset, Mesh.VertexStream vertexStream, int index)
        {
            Vector3 vector3 = new Vector3();

            vector3.x = BitConverter.ToSingle(vertexStream.Data, (vertexStream.BytesPerVertex*index) + offset + 0);
            vector3.y = BitConverter.ToSingle(vertexStream.Data, (vertexStream.BytesPerVertex*index) + offset + 4);
            vector3.z = BitConverter.ToSingle(vertexStream.Data, (vertexStream.BytesPerVertex*index) + offset + 8);

            return vector3;
        }
    }
}