namespace ForgelightUnity.Forgelight.Assets.Dme
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using Dma;
    using UnityEngine;

    public class ModelExporter
    {
        public static void ExportModel(ForgelightGame forgelightGame, Model model, string directory)
        {
            //TODO: Figure out what to do with non-version 4 models.
            if (model == null || model.Version != 4)
            {
                return;
            }

            //Validate this mesh.
            for (int i = 0; i < model.Meshes.Count; ++i)
            {
                Mesh mesh = model.Meshes[i];

                if (!forgelightGame.MaterialDefinitionManager.MaterialDefinitions.ContainsKey(model.Materials[(int) mesh.MaterialIndex].MaterialDefinitionHash))
                {
                    return;
                }
            }

            NumberFormatInfo format = new NumberFormatInfo();
            format.NumberDecimalSeparator = ".";

            Directory.CreateDirectory(directory + @"\Textures");

            List<string> usedTextures = new List<string>();

            foreach (Mesh mesh in model.Meshes)
            {
                if (mesh.BaseDiffuse != null)
                {
                    usedTextures.Add(mesh.BaseDiffuse);
                }

                if (mesh.SpecMap != null)
                {
                    usedTextures.Add(mesh.SpecMap);
                }

                if (mesh.BumpMap != null)
                {
                    usedTextures.Add(mesh.BumpMap);
                }
            }

            foreach (string textureString in usedTextures)
            {
                using (MemoryStream textureMemoryStream = forgelightGame.CreateAssetMemoryStreamByName(textureString))
                {
                    if (textureMemoryStream == null)
                    {
                        continue;
                    }

                    if (!File.Exists(directory + @"\Textures\" + textureString))
                    {
                        try
                        {
                            using (FileStream file = File.Create(directory + @"\Textures\" + textureString))
                            {
                                byte[] bytes = new byte[textureMemoryStream.Length];
                                textureMemoryStream.Read(bytes, 0, (int)textureMemoryStream.Length);
                                file.Write(bytes, 0, bytes.Length);
                            }
                        }
                        catch (IOException) {}
                    }
                }
            }

            string path = directory + @"\" + Path.GetFileNameWithoutExtension(model.Name) + ".obj";

            if (!File.Exists(path))
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    using (StreamWriter streamWriter = new StreamWriter(fileStream))
                    {
                        //Custom Material
                        foreach (Mesh mesh in model.Meshes)
                        {
                            if (mesh.BaseDiffuse != null)
                            {
                                if (!File.Exists(directory + @"\" + Path.GetFileNameWithoutExtension(mesh.BaseDiffuse) + @".mtl"))
                                {
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

                                    try
                                    {
                                        File.WriteAllLines(directory + @"\" + Path.GetFileNameWithoutExtension(mesh.BaseDiffuse) + @".mtl", mtl.ToArray());
                                    }

                                    //Another thread is already writing this material. No need to take any further action.
                                    catch (IOException) {}
                                }

                                streamWriter.WriteLine("mtllib " + Path.GetFileNameWithoutExtension(mesh.BaseDiffuse) + ".mtl");
                            }
                        }

                        foreach (Mesh mesh in model.Meshes)
                        {
                            MaterialDefinition materialDefinition = forgelightGame.MaterialDefinitionManager.MaterialDefinitions[model.Materials[(int)mesh.MaterialIndex].MaterialDefinitionHash];
                            VertexLayout vertexLayout = forgelightGame.MaterialDefinitionManager.VertexLayouts[materialDefinition.DrawStyles[0].VertexLayoutNameHash];

                            //position
                            VertexLayout.Entry.DataTypes positionDataType;
                            int positionOffset;
                            int positionStreamIndex;

                            vertexLayout.GetEntryInfoFromDataUsageAndUsageIndex(VertexLayout.Entry.DataUsages.Position, 0, out positionDataType, out positionStreamIndex, out positionOffset);

                            Mesh.VertexStream positionStream = mesh.VertexStreams[positionStreamIndex];

                            for (int j = 0; j < mesh.VertexCount; ++j)
                            {
                                Vector3 position = ReadVector3(positionOffset, positionStream, j);

                                streamWriter.WriteLine("v " + position.x.ToString(format) + " " + position.y.ToString(format) + " " + position.z.ToString(format));
                            }

                            //texture coordinates
                            VertexLayout.Entry.DataTypes texCoord0DataType;
                            int texCoord0Offset;
                            int texCoord0StreamIndex;

                            bool texCoord0Present = vertexLayout.GetEntryInfoFromDataUsageAndUsageIndex(VertexLayout.Entry.DataUsages.Texcoord, 0, out texCoord0DataType, out texCoord0StreamIndex, out texCoord0Offset);

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
                                            texCoord.x = BitConverter.ToSingle(texCoord0Stream.Data, (j * texCoord0Stream.BytesPerVertex) + 0);
                                            texCoord.y = 1.0f - BitConverter.ToSingle(texCoord0Stream.Data, (j * texCoord0Stream.BytesPerVertex) + 4);
                                            break;
                                        }

                                        case VertexLayout.Entry.DataTypes.float16_2:
                                        {
                                            texCoord.x = Half.FromBytes(texCoord0Stream.Data, (j * texCoord0Stream.BytesPerVertex) + texCoord0Offset + 0);
                                            texCoord.y = 1.0f - Half.FromBytes(texCoord0Stream.Data, (j * texCoord0Stream.BytesPerVertex) + texCoord0Offset + 2);
                                            break;
                                        }

                                        default:
                                            texCoord.x = 0;
                                            texCoord.y = 0;
                                            break;
                                    }

                                    streamWriter.WriteLine("vt " + texCoord.x.ToString(format) + " " + texCoord.y.ToString(format));
                                }
                            }
                        }

                        //faces
                        uint vertexCount = 0;

                        for (int i = 0; i < model.Meshes.Count; ++i)
                        {
                            Mesh mesh = model.Meshes[i];
                            streamWriter.WriteLine("g Mesh" + i);

                            //Custom Material
                            if (mesh.BaseDiffuse != null)
                            {
                                streamWriter.WriteLine("usemtl " + Path.GetFileNameWithoutExtension(mesh.BaseDiffuse));
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

                                streamWriter.WriteLine("f " + index2 + "/" + index2 + "/" + index2 + " " + index1 + "/" + index1 + "/" + index1 + " " + index0 + "/" + index0 + "/" + index0);
                            }

                            vertexCount += mesh.VertexCount;
                        }
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