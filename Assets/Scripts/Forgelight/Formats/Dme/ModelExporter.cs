using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Forgelight.Formats.Dma;
using UnityEngine;

namespace Forgelight.Formats.Dme
{
    public class ModelExporter
    {
        public static void ExportModel(Model model, string directory)
        {
            //TODO: Figure out what to do with non-version 4 models.
            if (model == null || model.Version != 4)
            {
                return;
            }

            //Validate this mesh.
            for (Int32 i = 0; i < model.Meshes.Length; ++i)
            {
                Mesh mesh = model.Meshes[i];

                if (
                    !MaterialDefinitionManager.Instance.MaterialDefinitions.ContainsKey(
                        model.Materials[(Int32) mesh.MaterialIndex].MaterialDefinitionHash))
                    return;
            }

            NumberFormatInfo format = new NumberFormatInfo();
            format.NumberDecimalSeparator = ".";

            Directory.CreateDirectory(directory + @"\Textures");

            List<string> usedTextures = new List<string>();

            for (Int32 i = 0; i < model.Meshes.Length; ++i)
            {
                Mesh mesh = model.Meshes[i];

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
                using (
                    MemoryStream textureMemoryStream = AssetManager.Instance.CreateAssetMemoryStreamByName(textureString)
                    )
                {
                    if (textureMemoryStream == null)
                        continue;

                    using (
                        FileStream file = new FileStream(directory + @"\Textures\" + textureString, FileMode.Create,
                            FileAccess.Write))
                    {
                        byte[] bytes = new byte[textureMemoryStream.Length];
                        textureMemoryStream.Read(bytes, 0, (int) textureMemoryStream.Length);
                        file.Write(bytes, 0, bytes.Length);
                    }
                }
            }

            string path = directory + @"\" + Path.GetFileNameWithoutExtension(model.Name) + ".obj";

            using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                {
                    //Custom Material
                    for (Int32 i = 0; i < model.Meshes.Length; ++i)
                    {
                        Mesh mesh = model.Meshes[i];

                        if (mesh.BaseDiffuse != null)
                        {
                            if (
                                !File.Exists(directory + @"\" + Path.GetFileNameWithoutExtension(mesh.BaseDiffuse) +
                                             @".mtl"))
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

                                File.WriteAllLines(
                                    directory + @"\" + Path.GetFileNameWithoutExtension(mesh.BaseDiffuse) + @".mtl",
                                    mtl.ToArray());
                            }

                            streamWriter.WriteLine("mtllib " + Path.GetFileNameWithoutExtension(mesh.BaseDiffuse) +
                                                   ".mtl");
                        }
                    }

                    for (Int32 i = 0; i < model.Meshes.Length; ++i)
                    {
                        Mesh mesh = model.Meshes[i];

                        MaterialDefinition materialDefinition =
                            MaterialDefinitionManager.Instance.MaterialDefinitions[
                                model.Materials[(Int32) mesh.MaterialIndex].MaterialDefinitionHash];
                        VertexLayout vertexLayout =
                            MaterialDefinitionManager.Instance.VertexLayouts[
                                materialDefinition.DrawStyles[0].VertexLayoutNameHash];

                        //position
                        VertexLayout.Entry.DataTypes positionDataType;
                        Int32 positionOffset;
                        Int32 positionStreamIndex;

                        vertexLayout.GetEntryInfoFromDataUsageAndUsageIndex(VertexLayout.Entry.DataUsages.Position, 0,
                            out positionDataType, out positionStreamIndex, out positionOffset);

                        Mesh.VertexStream positionStream = mesh.VertexStreams[positionStreamIndex];

                        for (Int32 j = 0; j < mesh.VertexCount; ++j)
                        {
                            Vector3 position = ReadVector3(positionOffset, positionStream, j);

                            streamWriter.WriteLine("v " + position.x.ToString(format) + " " +
                                                   position.y.ToString(format) + " " + position.z.ToString(format));
                        }

                        //texture coordinates
                        VertexLayout.Entry.DataTypes texCoord0DataType;
                        Int32 texCoord0Offset = 0;
                        Int32 texCoord0StreamIndex = 0;

                        Boolean texCoord0Present =
                            vertexLayout.GetEntryInfoFromDataUsageAndUsageIndex(VertexLayout.Entry.DataUsages.Texcoord,
                                0, out texCoord0DataType, out texCoord0StreamIndex, out texCoord0Offset);

                        if (texCoord0Present)
                        {
                            Mesh.VertexStream texCoord0Stream = mesh.VertexStreams[texCoord0StreamIndex];

                            for (Int32 j = 0; j < mesh.VertexCount; ++j)
                            {
                                Vector2 texCoord;

                                switch (texCoord0DataType)
                                {
                                    case VertexLayout.Entry.DataTypes.Float2:
                                        texCoord.x = BitConverter.ToSingle(texCoord0Stream.Data,
                                            (j*texCoord0Stream.BytesPerVertex) + 0);
                                        texCoord.y = 1.0f -
                                                     BitConverter.ToSingle(texCoord0Stream.Data,
                                                         (j*texCoord0Stream.BytesPerVertex) + 4);
                                        break;
                                    case VertexLayout.Entry.DataTypes.float16_2:
                                        texCoord.x = Half.FromBytes(texCoord0Stream.Data,
                                            (j*texCoord0Stream.BytesPerVertex) + texCoord0Offset + 0);
                                        texCoord.y = 1.0f -
                                                     Half.FromBytes(texCoord0Stream.Data,
                                                         (j*texCoord0Stream.BytesPerVertex) + texCoord0Offset + 2);
                                        break;
                                    default:
                                        texCoord.x = 0;
                                        texCoord.y = 0;
                                        break;
                                }

                                streamWriter.WriteLine("vt " + texCoord.x.ToString(format) + " " +
                                                       texCoord.y.ToString(format));
                            }
                        }
                    }

                    //faces
                    UInt32 vertexCount = 0;

                    for (Int32 i = 0; i < model.Meshes.Length; ++i)
                    {
                        Mesh mesh = model.Meshes[i];

                        streamWriter.WriteLine("g Mesh" + i);

                        //Custom Material
                        if (mesh.BaseDiffuse != null)
                        {
                            streamWriter.WriteLine("usemtl " + Path.GetFileNameWithoutExtension(mesh.BaseDiffuse));
                        }

                        for (Int32 j = 0; j < mesh.IndexCount; j += 3)
                        {
                            UInt32 index0, index1, index2;

                            switch (mesh.IndexSize)
                            {
                                case 2:
                                    index0 = vertexCount + BitConverter.ToUInt16(mesh.IndexData, (j*2) + 0) + 1;
                                    index1 = vertexCount + BitConverter.ToUInt16(mesh.IndexData, (j*2) + 2) + 1;
                                    index2 = vertexCount + BitConverter.ToUInt16(mesh.IndexData, (j*2) + 4) + 1;
                                    break;
                                case 4:
                                    index0 = vertexCount + BitConverter.ToUInt32(mesh.IndexData, (j*4) + 0) + 1;
                                    index1 = vertexCount + BitConverter.ToUInt32(mesh.IndexData, (j*4) + 4) + 1;
                                    index2 = vertexCount + BitConverter.ToUInt32(mesh.IndexData, (j*4) + 8) + 1;
                                    break;
                                default:
                                    index0 = 0;
                                    index1 = 0;
                                    index2 = 0;
                                    break;
                            }

                            streamWriter.WriteLine("f " + index2 + "/" + index2 + "/" + index2 + " " + index1 + "/" +
                                                   index1 + "/" + index1 + " " + index0 + "/" + index0 + "/" + index0);
                        }

                        vertexCount += (UInt32) mesh.VertexCount;
                    }
                }
            }
        }

        private static Vector3 ReadVector3(Int32 offset, Mesh.VertexStream vertexStream, Int32 index)
        {
            Vector3 vector3 = new Vector3();

            vector3.x = BitConverter.ToSingle(vertexStream.Data, (vertexStream.BytesPerVertex*index) + offset + 0);
            vector3.y = BitConverter.ToSingle(vertexStream.Data, (vertexStream.BytesPerVertex*index) + offset + 4);
            vector3.z = BitConverter.ToSingle(vertexStream.Data, (vertexStream.BytesPerVertex*index) + offset + 8);

            return vector3;
        }
    }
}