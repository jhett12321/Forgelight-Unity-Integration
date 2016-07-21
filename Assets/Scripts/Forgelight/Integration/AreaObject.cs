using System.Collections.Generic;
using UnityEngine;
using Forgelight.Attributes;

namespace Forgelight.Integration
{
    public class AreaObject : CullableObject
    {
        //Common
        [ReadOnly]
        public string ID;
        [ReadOnly]
        public string Name;
        [ReadOnly]
        public string Shape;
        [ReadOnly]
        public Vector3 Pos1;

        //Sphere Shape
        [ReadOnly]
        public float Radius;

        //Box Shape
        [ReadOnly]
        public Vector3 Pos2;
        [ReadOnly]
        public Vector3 Rot;

        //Properties
        [ReadOnly]
        public List<string> Properties;
    }
}
