namespace ForgelightUnity.Editor.Forgelight.Importers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Assets.Cnk;
    using Assets.Pack;
    using ImageMagick;
    using UnityEngine;

    public class TerrainLODImporter : ForgelightImporter<CnkLOD, TerrainLODImporter.ThreadData>
    {
        public class ThreadData
        {
            public StringBuilder StringBuilder = new StringBuilder();
        }

        protected override string ProgressItemPrefix
        {
            get { return "Exporting Chunk: "; }
        }

        protected override AssetType AssetType
        {
            get { return AssetType.CNK1; }
        }

        private const int CNKLOD_POOL_SIZE = 100;
        private object textureExportLock = new object();

        public TerrainLODImporter() : base(CNKLOD_POOL_SIZE) {}

        protected override void Import(AssetRef asset, ThreadData data, object oLock)
        {
            //De-serialize
            using (MemoryStream terrainMemoryStream = asset.Pack.CreateAssetMemoryStreamByName(asset.Name))
            {
                CnkLOD chunk = null;
                while (chunk == null)
                {
                    chunk = ObjectPool.GetPooledObject();
                }

                bool result = chunk.InitializeFromStream(asset.Name, asset.DisplayName, terrainMemoryStream);

                if (result)
                {
                    ExportChunk(chunk, data.StringBuilder);
                    ExportTextures(chunk);
                }

                ObjectPool.ReturnObjectToPool(chunk);
            }
        }

        private void CreateMaterial(string directory, string name)
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

        public void ExportChunk(CnkLOD chunk, StringBuilder stringBuilder)
        {
            string name = Path.GetFileNameWithoutExtension(chunk.Name);

            if (name == null)
            {
                return;
            }

            string directory = ResourceDir + "/Terrain/" + name.Split('_')[0];

            if (!Directory.Exists(directory + "/Textures"))
            {
                Directory.CreateDirectory(directory + "/Textures");
            }

            // Material
            CreateMaterial(directory, name);

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
                try
                {
                    stringBuilder.Length = 0;

                    List<string> vertices = new List<string>();
                    List<string> uvs = new List<string>();
                    List<string> faces = new List<string>();

                    stringBuilder.AppendLine("mtllib " + name + ".mtl");
                    stringBuilder.AppendLine("o " + name);
                    stringBuilder.AppendLine("g " + name);

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
                        stringBuilder.AppendLine(vertex);
                    }

                    foreach (string uv in uvs)
                    {
                        stringBuilder.AppendLine(uv);
                    }

                    stringBuilder.AppendLine("usemtl " + name);

                    foreach (string face in faces)
                    {
                        stringBuilder.AppendLine(face);
                    }

                    File.WriteAllText(path, stringBuilder.ToString());
                }
                catch (Exception e)
                {
                    Debug.LogError("Chunk export failed for: " + name + "\n" +
                                    e.Message + "\n" +
                                    e.StackTrace);
                }
            }
        }

        public void ExportTextures(CnkLOD chunk)
        {
            string name = Path.GetFileNameWithoutExtension(chunk.Name);

            if (name == null)
            {
                return;
            }

            string directory = ResourceDir + "/Terrain/" + name.Split('_')[0];

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

            // Color Map
            string colorMapPath = directory + @"/Textures/" + name + "_colornx" + ".dds";
            if (!File.Exists(colorMapPath))
            {
                CreateTexture(montageSettings, colorMapPath, chunk.Textures.Select(texture => texture.ColorNXMap.ToArray()));
            }

            // Specular map
            string specMapPath = directory + @"/Textures/" + name + "_specny" + ".dds";
            if (!File.Exists(specMapPath))
            {
                CreateTexture(montageSettings, specMapPath, chunk.Textures.Select(texture => texture.SpecNyMap.ToArray()));
            }
        }

        // TODO This library shouldn't be required.
        private void CreateTexture(MontageSettings montageSettings, string path, IEnumerable<byte[]> textures)
        {
            lock (textureExportLock)
            {
                using (MagickImageCollection stichedTexture = new MagickImageCollection())
                {
                    foreach (byte[] texture in textures)
                    {
                        stichedTexture.Add(new MagickImage(texture));
                    }

                    using (MagickImage result = stichedTexture.Montage(montageSettings))
                    {
                        result.Write(path);
                    }
                }
            }
        }

        public int RemapIndex(int startIndex, int startGridSizeX, int newGridSizeX)
        {
            int newIndex = startIndex;

            newIndex += (startIndex / startGridSizeX) * newGridSizeX;

            return newIndex;
        }

        public int ShiftIndex(int index, int xAmount, int yAmount, int gridSizeX)
        {
            index += xAmount;
            index += yAmount * gridSizeX;
            return index;
        }
    }
}