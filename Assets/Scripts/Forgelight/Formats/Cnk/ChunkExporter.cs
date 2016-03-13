using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Forgelight.Formats.Cnk
{
    public class ChunkExporter
    {

        public static void ExportChunk(ForgelightGame forgelightGame, CnkLOD chunk, string directory)
        {
            string name = Path.GetFileNameWithoutExtension(chunk.Name);
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
                    List<string> mtl = new List<string>();

                    string[] baseMtl =
                    {
                        "newmtl " + name,
                        "Ka 1.000000 1.000000 1.000000",
                        "Kd 1.000000 1.000000 1.000000",
                        "Ks 0.000000 0.000000 0.000000",
                        "d 1.0",
                        "illum 2",
                        "map_Ka " + name + "_colornx" + ".png",
                        "map_Kd " + name + "_colornx" + ".png",
                        "map_d " + name + "_colornx" + ".png",
                        "map_Ks " + name + "_colornx" + ".png",
                        "map_Ns " + name + "_specny" + ".png"
                    };

                    File.WriteAllLines(directory + @"\" + name + @".mtl", mtl.ToArray());
                }
            }
            catch (IOException) {}

            //Geometry
            string path = directory + @"\" + name + ".obj";
            using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
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
                            float k = (int) (vertexOffset + j);
                            float x = chunk.Vertices[(int) k].X + (i >> 1) * 64;
                            float y = chunk.Vertices[(int) k].Y + (i % 2) * 64;
                            float heightNear = (float)chunk.Vertices[(int) k].HeightNear / 64 ;

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

                            faces.Add("f " + (v2 + 1) + "/" + (v2 + 1) + " " + (v1 + 1) + "/" + (v1 + 1) + " " + (v0 + 1) + "/" + (v0 + 1));
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
                }
            }
        }

        //Since creating Texture2D's is not thread safe, we need to call this in the main thread.
        //TODO Broken - Textures don't seem to be stitched/imported correctly.
        //public static void ExportTextures(ForgelightGame forgelightGame, CnkLOD chunk, string directory)
        //{
        //    string name = Path.GetFileNameWithoutExtension(chunk.Name);
        //    directory += "/" + name.Split('_')[0];

        //    //TODO Code Duplication
        //    //Color Map
        //    Texture2D stitchedColorMap = new Texture2D(1024, 1024);
        //    for (int i = 0; i < chunk.Textures.Count; i++)
        //    {
        //        CnkLOD.Texture texture = chunk.Textures[i];
        //        Texture2D dataStore = new Texture2D(512, 512);

        //        dataStore.LoadImage(texture.ColorNXMap.ToArray());

        //        for (int j = 0; j < dataStore.GetPixels().Length; j++)
        //        {
        //            //Local pixel coordinate.
        //            int x = j % 512;
        //            int y = j / 512;

        //            Color pixel = dataStore.GetPixel(x, y);

        //            //Stitched coordinate.
        //            int stitchedX = ((i % 2) * 512) + x;
        //            int stitchedY = ((i / 2) * 512) + y;

        //            stitchedColorMap.SetPixel(stitchedX, stitchedY, pixel);
        //        }

        //        stitchedColorMap.Apply();
        //    }

        //    string colorMapPath = directory + @"\Textures\" + name + "_colornx" + ".png";
        //    byte[] colorMap = stitchedColorMap.EncodeToPNG();

        //    if (!File.Exists(colorMapPath))
        //    {
        //        using (FileStream file = File.Create(colorMapPath))
        //        {
        //            file.Write(colorMap, 0, colorMap.Length);
        //        }
        //    }

        //    //TODO code duplication
        //    //Specular map
        //    Texture2D stitchedSpecMap = new Texture2D(1024, 1024);
        //    for (int i = 0; i < chunk.Textures.Count; i++)
        //    {
        //        CnkLOD.Texture texture = chunk.Textures[i];
        //        Texture2D dataStore = new Texture2D(512, 512);

        //        dataStore.LoadImage(texture.SpecNyMap.ToArray());

        //        for (int j = 0; j < dataStore.GetPixels().Length; j++)
        //        {
        //            //Local pixel coordinate.
        //            int x = j % 512;
        //            int y = j / 512;

        //            Color pixel = dataStore.GetPixel(x, y);

        //            //Stitched coordinate.
        //            int stitchedX = ((i % 2) * 512) + x;
        //            int stitchedY = ((i / 2) * 512) + y;

        //            stitchedSpecMap.SetPixel(stitchedX, stitchedY, pixel);
        //        }

        //        stitchedSpecMap.Apply();
        //    }

        //    string specMapPath = directory + @"\Textures\" + name + "_specny" + ".png";
        //    byte[] specMap = stitchedSpecMap.EncodeToPNG();

        //    if (!File.Exists(specMapPath))
        //    {
        //        using (FileStream file = File.Create(specMapPath))
        //        {
        //            file.Write(specMap, 0, specMap.Length);
        //        }
        //    }
        //}
    }
}