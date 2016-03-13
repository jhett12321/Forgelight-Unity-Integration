using System;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.Forgelight.Utils;
using UnityEngine;

namespace Forgelight.Formats.Zone
{
    public class Object
    {
        public class Instance
        {
            public Vector4 Position { get; set; }
            public Vector4 Rotation { get; set; }
            public Vector4 Scale { get; set; }
            public UInt32 ID { get; set; }
            public bool DontCastShadows { get; set; }
            public float LODMultiplier { get; set; }
        }

        public string ActorDefinition { get; set; }
        public float RenderDistance { get; set; }
        public List<Instance> Instances { get; set; }

        public static Object ReadFromStream(Stream stream)
        {
            Object obj = new Object();
            BinaryReader binaryReader = new BinaryReader(stream);

            obj.ActorDefinition = binaryReader.ReadNullTerminatedString();
            obj.RenderDistance = binaryReader.ReadSingle();

            obj.Instances = new List<Instance>();
            UInt32 instancesLength = binaryReader.ReadUInt32();

            for (uint i = 0; i < instancesLength; i++)
            {
                Instance instance = new Instance();

                instance.Position = new Vector4(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
                instance.Rotation = new Vector4(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
                instance.Scale = new Vector4(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
                instance.ID = binaryReader.ReadUInt32();
                instance.DontCastShadows = binaryReader.ReadBoolean();
                instance.LODMultiplier = binaryReader.ReadSingle();

                obj.Instances.Add(instance);
            }

            return obj;
        }

        public void WriteToStream(BinaryWriter binaryWriter)
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
            }
        }
    }
}
