using System.Collections.Generic;
using System.IO;
using Forgelight.Formats.Dma;

namespace Forgelight.Formats.Dme
{
    public class Mesh
    {
        public class VertexStream
        {
            public static VertexStream LoadFromStream(Stream stream, int vertexCount, int bytesPerVertex)
            {
                VertexStream vertexStream = new VertexStream();

                vertexStream.BytesPerVertex = bytesPerVertex;

                BinaryReader binaryReader = new BinaryReader(stream);

                vertexStream.Data = binaryReader.ReadBytes(vertexCount*bytesPerVertex);

                return vertexStream;
            }

            public int BytesPerVertex { get; private set; }
            public byte[] Data { get; private set; }
        }

        public VertexStream[] VertexStreams { get; private set; }
        public byte[] IndexData { get; private set; }

        public uint MaterialIndex { get; set; }
        public uint Unknown1 { get; set; }
        public uint Unknown2 { get; set; }
        public uint Unknown3 { get; set; }
        public uint Unknown4 { get; set; }
        public uint VertexCount { get; set; }
        public uint IndexCount { get; private set; }
        public uint IndexSize { get; private set; }

        //The diffuse map. Forgelight Ref: BaseDiffuse, baseDiffuse
        public string BaseDiffuse { get; set; }

        //The normal map. Forgelight Ref: Bump, BumpMap
        public string BumpMap { get; set; }

        //The specular map. Forgelight Ref: Spec
        public string SpecMap { get; set; }

        public static Mesh LoadFromStream(Stream stream, ICollection<Material> materials)
        {
            BinaryReader binaryReader = new BinaryReader(stream);

            Mesh mesh = new Mesh();

            mesh.MaterialIndex = binaryReader.ReadUInt32();
            mesh.Unknown1 = binaryReader.ReadUInt32();
            mesh.Unknown2 = binaryReader.ReadUInt32();
            mesh.Unknown3 = binaryReader.ReadUInt32();
            uint vertexStreamCount = binaryReader.ReadUInt32();
            mesh.IndexSize = binaryReader.ReadUInt32();
            mesh.IndexCount = binaryReader.ReadUInt32();
            mesh.VertexCount = binaryReader.ReadUInt32();

            mesh.VertexStreams = new VertexStream[(int) vertexStreamCount];

            // read vertex streams
            for (int j = 0; j < vertexStreamCount; ++j)
            {
                uint bytesPerVertex = binaryReader.ReadUInt32();

                VertexStream vertexStream = VertexStream.LoadFromStream(binaryReader.BaseStream,
                    (int) mesh.VertexCount, (int) bytesPerVertex);

                if (vertexStream != null)
                {
                    mesh.VertexStreams[j] = vertexStream;
                }
            }

            // read indices
            mesh.IndexData = binaryReader.ReadBytes((int) mesh.IndexCount*(int) mesh.IndexSize);

            return mesh;
        }
    }
}
