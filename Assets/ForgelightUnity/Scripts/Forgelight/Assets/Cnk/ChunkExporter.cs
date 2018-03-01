namespace ForgelightUnity.Forgelight.Assets.Cnk
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using ImageMagick;
    using UnityEngine;

    public class ChunkExporter
    {
        public static void ExportChunk(ForgelightGame forgelightGame, CnkLOD chunk, string directory)
        {
            string name = Path.GetFileNameWithoutExtension(chunk.Name);

            if (name == null)
            {
                return;
            }

            directory += "/" + name.Split('_')[0];

            if (!Directory.Exists(directory + "/Textures"))
            {
                Directory.CreateDirectory(directory + "/Textures");
            }

            //Textures
            try
            {
                //Material
                if (!File.Exists(directory + @"\" + name + @".mtl"))
                {
                    string[] mtl =
                    {
                        "newmtl " + name,
                        "Ka 1.000000 1.000000 1.000000",
                        "Kd 1.000000 1.000000 1.000000",
                        "Ks 0.000000 0.000000 0.000000",
                        "d 1.0",
                        "illum 1",
                        "map_Ka " + name + "_colornx" + ".dds",
                        "map_Kd " + name + "_colornx" + ".dds",
                        "map_d " + name + "_colornx" + ".dds",
                        "map_Ks " + name + "_colornx" + ".dds",
                        "map_Ns " + name + "_specny" + ".dds"
                    };

                    File.WriteAllLines(directory + @"\" + name + @".mtl", mtl);
                }
            }
            catch (IOException) {}

            //Heighmaps
            //Texture2D image = new Texture2D((int) chunk.VertsPerSide, (int) chunk.VertsPerSide);

            //byte[] imageData = image.GetRawTextureData();

            //for (int i = 0; i < chunk.HeightMaps.Count; i++)
            //{
            //    Dictionary<int, CnkLOD.HeightMap> heightmap = chunk.HeightMaps[i];

            //    uint n = chunk.VertsPerSide*chunk.VertsPerSide;

            //    for (int j = 0; j < n; j++)
            //    {
            //        int height = heightmap[j].Val1 + 4096;

            //        imageData[j*4] = (byte) (height >> 8);
            //        imageData[j*4 + 1] = (byte) (height & 0xFF);
            //        imageData[j*4 + 2] = 0;
            //        imageData[j*4 + 3] = 255;
            //    }

            //    image.LoadRawTextureData(imageData);
            //}

            //Geometry
            string path = directory + @"\" + name + ".obj";

            if (!File.Exists(path))
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    using (StreamWriter streamWriter = new StreamWriter(fileStream))
                    {
                        try
                        {
                            List<string> vertices = new List<string>();
                            List<string> uvs = new List<string>();
                            List<string> faces = new List<string>();

                            streamWriter.WriteLine("mtllib " + name + ".mtl");
                            streamWriter.WriteLine("o " + name);
                            streamWriter.WriteLine("g " + name);

                            for (int i = 0; i < 4; i++)
                            {
                                uint vertexOffset = chunk.RenderBatches[i].VertexOffset;
                                uint vertextCount = chunk.RenderBatches[i].VertexCount;

                                for (uint j = 0; j < vertextCount; j++)
                                {
                                    int k = (int) (vertexOffset + j);
                                    double x = chunk.Vertices[k].X + (i >> 1) * 64;
                                    double y = chunk.Vertices[k].Y + (i % 2) * 64;
                                    double heightNear = (double) chunk.Vertices[k].HeightNear / 64;

                                    vertices.Add("v " + x + " " + heightNear + " " + y);
                                    uvs.Add("vt " + (y / 128) + " " + (1 - x / 128));
                                }
                            }

                            for (int i = 0; i < 4; i++)
                            {
                                int indexOffset = (int) chunk.RenderBatches[i].IndexOffset;
                                uint indexCount = chunk.RenderBatches[i].IndexCount;
                                uint vertexOffset = chunk.RenderBatches[i].VertexOffset;

                                for (int j = 0; j < indexCount; j += 3)
                                {
                                    uint v0 = chunk.Indices[j + indexOffset] + vertexOffset;
                                    uint v1 = chunk.Indices[j + indexOffset + 1] + vertexOffset;
                                    uint v2 = chunk.Indices[j + indexOffset + 2] + vertexOffset;

                                    faces.Add("f " + (v2 + 1) + "/" + (v2 + 1) + " " + (v1 + 1) + "/" + (v1 + 1) + " " +
                                              (v0 + 1) + "/" + (v0 + 1));
                                }
                            }

                            foreach (string vertex in vertices)
                            {
                                streamWriter.WriteLine(vertex);
                            }

                            foreach (string uv in uvs)
                            {
                                streamWriter.WriteLine(uv);
                            }

                            streamWriter.WriteLine("usemtl " + name);

                            foreach (string face in faces)
                            {
                                streamWriter.WriteLine(face);
                            }

                            return;
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("Chunk export failed for: " + name + "\n" +
                                           e.Message + "\n" +
                                           e.StackTrace);
                        }
                    }
                }
            }
        }

        public static void ExportTextures(ForgelightGame forgelightGame, CnkLOD chunk, string directory)
        {
            string name = Path.GetFileNameWithoutExtension(chunk.Name);

            if (name == null)
            {
                return;
            }

            directory += "/" + name.Split('_')[0];

            if (!Directory.Exists(directory + @"/Textures"))
            {
                Directory.CreateDirectory(directory + @"/Textures");
            }

            MontageSettings montageSettings = new MontageSettings();
            montageSettings.TileGeometry = new MagickGeometry(2, 2);
            montageSettings.Geometry = new MagickGeometry(512, 512);
            montageSettings.BackgroundColor = MagickColor.FromRgba(0, 0, 0, 0);
            montageSettings.BorderColor = MagickColor.FromRgba(0, 0, 0, 0);
            montageSettings.BorderWidth = 0;

            //TODO Code Duplication
            //Color Map
            string colorMapPath = directory + @"/Textures/" + name + "_colornx" + ".dds";

            if (!File.Exists(colorMapPath))
            {
                using (MagickImageCollection stitchedColorMap = new MagickImageCollection())
                {
                    foreach (CnkLOD.Texture texture in chunk.Textures)
                    {
                        MagickImage textureQuad = new MagickImage(texture.ColorNXMap.ToArray());
                        stitchedColorMap.Add(textureQuad);
                    }

                    using (MagickImage result = stitchedColorMap.Montage(montageSettings))
                    {
                        result.Write(colorMapPath);
                    }
                }
            }

            //TODO code duplication
            //Specular map
            string specMapPath = directory + @"/Textures/" + name + "_specny" + ".dds";

            if (!File.Exists(specMapPath))
            {
                using (MagickImageCollection stitchedSpecMap = new MagickImageCollection())
                {
                    foreach (CnkLOD.Texture texture in chunk.Textures)
                    {
                        MagickImage textureQuad = new MagickImage(texture.SpecNyMap.ToArray());
                        stitchedSpecMap.Add(textureQuad);
                    }

                    using (MagickImage result = stitchedSpecMap.Montage(montageSettings))
                    {
                        result.Write(specMapPath);
                    }
                }
            }
        }

        //public static Texture2D LoadTextureDXT(byte[] ddsBytes, TextureFormat textureFormat)
        //{
        //    if (textureFormat != TextureFormat.DXT1 && textureFormat != TextureFormat.DXT5)
        //        throw new Exception("Invalid TextureFormat. Only DXT1 and DXT5 formats are supported by this method.");

        //    byte ddsSizeCheck = ddsBytes[4];
        //    if (ddsSizeCheck != 124)
        //        throw new Exception("Invalid DDS DXTn texture. Unable to read");  //this header byte should be 124 for DDS image files

        //    int height = ddsBytes[13] * 256 + ddsBytes[12];
        //    int width = ddsBytes[17] * 256 + ddsBytes[16];

        //    int DDS_HEADER_SIZE = 128;
        //    byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
        //    Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);

        //    Texture2D texture = new Texture2D(width, height, textureFormat, false);
        //    texture.LoadRawTextureData(dxtBytes);
        //    texture.Apply();

        //    return (texture);
        //}
    }
}