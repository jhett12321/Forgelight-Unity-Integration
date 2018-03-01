namespace ForgelightUnity.Forgelight.Assets.Dme
{
    using System.Collections.Generic;
    using System.IO;
    using Dma;

    public class Mesh
    {
        //The diffuse map. Forgelight Ref: BaseDiffuse, baseDiffuse
        public string BaseDiffuse { get; set; }

        //The normal map. Forgelight Ref: Bump, BumpMap
        public string BumpMap { get; set; }

        //The specular map. Forgelight Ref: Spec
        public string SpecMap { get; set; }

        #region Structure
        public uint MaterialIndex { get; set; }
        public uint Unknown1 { get; set; }
        public uint Unknown2 { get; set; }
        public uint Unknown3 { get; set; }
        public uint IndexSize { get; private set; }
        public uint IndexCount { get; private set; }
        public uint VertexCount { get; set; }

        public VertexStream[] VertexStreams { get; private set; }
        public class VertexStream
        {
            public int BytesPerVertex { get; set; }
            public byte[] Data { get;  set; }
        }

        public byte[] IndexData { get; private set; }
        #endregion

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

                VertexStream vertexStream = new VertexStream();

                vertexStream.BytesPerVertex = (int) bytesPerVertex;
                vertexStream.Data = binaryReader.ReadBytes((int) (mesh.VertexCount * bytesPerVertex));

                mesh.VertexStreams[j] = vertexStream;
            }

            // read indices
            mesh.IndexData = binaryReader.ReadBytes((int) mesh.IndexCount*(int) mesh.IndexSize);

            return mesh;
        }
    }
}
