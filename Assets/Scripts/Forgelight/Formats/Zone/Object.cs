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

        public string ActorDefinition { get; private set; }
        public float RenderDistance { get; private set; }
        public List<Instance> Instances { get; private set; }

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
    }
}
