namespace ForgelightUnity.Forgelight.Assets.Zone
{
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;
    using Utils;

    public class Object
    {
        #region Structure
        public string ActorDefinition { get; set; }
        public float RenderDistance { get; set; }
        public List<Instance> Instances { get; set; }
        public class Instance
        {
            public Vector4 Position { get; set; }
            public Vector4 Rotation { get; set; }
            public Vector4 Scale { get; set; }
            public uint ID { get; set; }
            public bool DontCastShadows { get; set; }
            public float LODMultiplier { get; set; }

            public uint UnknownDword1 { get; set; }
            public uint UnknownDword2 { get; set; }
            public uint UnknownDword3 { get; set; }
            public uint UnknownDword4 { get; set; }
            public uint UnknownDword5 { get; set; }

        }
        #endregion

        public static Object ReadFromStream(Stream stream, ZoneType zoneType)
        {
            Object obj = new Object();
            BinaryReader binaryReader = new BinaryReader(stream);

            obj.ActorDefinition = binaryReader.ReadNullTerminatedString();
            obj.RenderDistance = binaryReader.ReadSingle();

            obj.Instances = new List<Instance>();
            uint instancesLength = binaryReader.ReadUInt32();

            for (uint i = 0; i < instancesLength; i++)
            {
                Instance instance = new Instance();

                instance.Position = new Vector4(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
                instance.Rotation = new Vector4(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
                instance.Scale = new Vector4(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
                instance.ID = binaryReader.ReadUInt32();
                instance.DontCastShadows = binaryReader.ReadBoolean();
                instance.LODMultiplier = binaryReader.ReadSingle();

                if (zoneType == ZoneType.H1Z1)
                {
                    instance.UnknownDword1 = binaryReader.ReadUInt32();
                    instance.UnknownDword2 = binaryReader.ReadUInt32();
                    instance.UnknownDword3 = binaryReader.ReadUInt32();
                    instance.UnknownDword4 = binaryReader.ReadUInt32();
                    instance.UnknownDword5 = binaryReader.ReadUInt32();
                }

                obj.Instances.Add(instance);
            }

            return obj;
        }

        public void WriteToStream(BinaryWriter binaryWriter, ZoneType zoneType)
        {
            binaryWriter.WriteNullTerminiatedString(ActorDefinition);
            binaryWriter.Write(RenderDistance);

            binaryWriter.Write((uint) Instances.Count);

            foreach (Instance instance in Instances)
            {
                Vector4 position = instance.Position;
                binaryWriter.Write(position.x);
                binaryWriter.Write(position.y);
                binaryWriter.Write(position.z);
                binaryWriter.Write(position.w);

                Vector4 rotation = instance.Rotation;
                binaryWriter.Write(rotation.x);
                binaryWriter.Write(rotation.y);
                binaryWriter.Write(rotation.z);
                binaryWriter.Write(rotation.w);

                Vector4 scale = instance.Scale;
                binaryWriter.Write(scale.x);
                binaryWriter.Write(scale.y);
                binaryWriter.Write(scale.z);
                binaryWriter.Write(scale.w);

                binaryWriter.Write(instance.ID);
                binaryWriter.Write(instance.DontCastShadows);
                binaryWriter.Write(instance.LODMultiplier);

                if (zoneType == ZoneType.H1Z1)
                {
                    binaryWriter.Write(instance.UnknownDword1);
                    binaryWriter.Write(instance.UnknownDword2);
                    binaryWriter.Write(instance.UnknownDword3);
                    binaryWriter.Write(instance.UnknownDword4);
                    binaryWriter.Write(instance.UnknownDword5);
                }
            }
        }
    }
}
