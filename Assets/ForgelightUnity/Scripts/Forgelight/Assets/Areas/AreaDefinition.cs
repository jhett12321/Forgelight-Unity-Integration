namespace ForgelightUnity.Forgelight.Assets.Areas
{
    using System.Collections.Generic;
    using UnityEngine;

    public class AreaDefinition
    {
        //Common
        public string ID;
        public string Name;
        public string Shape;
        public Vector3 Pos1;

        //Sphere Shape
        public float Radius;

        //Box Shape
        public Vector3 Pos2;
        public Vector3 Rot;

        //Properties
        public List<Property> Properties;
    }
}
