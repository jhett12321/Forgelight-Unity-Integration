namespace ForgelightUnity.Forgelight.Assets.Dma
{
    using System.Collections.Generic;
    using System.IO;

    public class Material
    {
        #region Structure
        public uint NameHash { get; private set; }
        public uint DataLength { get; private set; }
        public uint MaterialDefinitionHash { get; private set; }
        public List<Parameter> Parameters { get; private set; }
        public class Parameter
        {
            //http://msdn.microsoft.com/en-us/library/windows/desktop/bb205378(v=vs.85).aspx
            public enum D3DXParameterClass
            {
                Scalar = 0,
                Vector,
                MatrixRows,
                MatrixColumns,
                Object,
                Struct,
                ForceDword = 0x7fffffff
            }

            //http://msdn.microsoft.com/en-us/library/windows/desktop/bb205380(v=vs.85).aspx
            public enum D3DXParameterType
            {
                Void = 0,
                Bool,
                Int,
                Float,
                String,
                Texture,
                Texture1D,
                Texture2D,
                Texture3D,
                TextureCube,
                Sampler,
                Sampler1D,
                Sampler2D,
                Sampler3D,
                SamplerCube,
                PixelShader,
                VertexShader,
                PixelFragment,
                VertexFrament,
                Unsupported,
                ForceDword = 0x7fffffff
            }

            public uint NameHash { get; set; }
            public D3DXParameterClass Class { get; set; }
            public D3DXParameterType Type { get; set; }
            public byte[] Data { get; set; }
        }
        #endregion

        public static Material LoadFromStream(Stream stream)
        {
            BinaryReader binaryReader = new BinaryReader(stream);

            Material material = new Material();

            material.NameHash = binaryReader.ReadUInt32();
            material.DataLength = binaryReader.ReadUInt32();
            material.MaterialDefinitionHash = binaryReader.ReadUInt32();

            uint parameterCount = binaryReader.ReadUInt32();
            material.Parameters = new List<Parameter>((int) parameterCount);

            for (uint j = 0; j < parameterCount; ++j)
            {
                Parameter parameter = new Parameter();

                parameter.NameHash = binaryReader.ReadUInt32();
                parameter.Class = (Parameter.D3DXParameterClass)binaryReader.ReadUInt32();
                parameter.Type = (Parameter.D3DXParameterType)binaryReader.ReadUInt32();

                uint dataLength = binaryReader.ReadUInt32();
                parameter.Data = binaryReader.ReadBytes((int)dataLength);

                material.Parameters.Add(parameter);
            }

            return material;
        }
    }
}
