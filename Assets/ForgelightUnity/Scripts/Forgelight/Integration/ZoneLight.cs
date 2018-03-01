namespace ForgelightUnity.Forgelight.Integration
{
    using Attributes;
    using UnityEngine;
    using LightType = Assets.Zone.LightType;

    [ExecuteInEditMode]
    [SelectionBase]
    public class ZoneLight : CullableObject
    {
        [HideInInspector]
        public Light lightObject;

        public string Name;

        #region Representable Parameters
        [Header("Representable Parameters")]
        public LightType Type;
        public float Range;
        public Color Color;

        [Header("Other Parameters")]
        public string ColorName;
        public float UnknownFloat1;
        //public Vector4 Position { get; private set; }
        //public Vector4 Rotation { get; private set; }
        public float InnerRange;
        public byte UnknownByte1;
        public byte UnknownByte2;
        public byte UnknownByte3;
        public byte UnknownByte4;
        public byte UnknownByte5;
        public Vector4 UnknownVector1;
        public string UnknownString1;
        public uint ID;
        #endregion

        public void OnValidate()
        {
            if (lightObject == null)
            {
                lightObject = GetComponent<Light>() ?? gameObject.AddComponent<Light>();
            }

            switch (Type)
            {
                case LightType.Pointlight:
                    lightObject.type = UnityEngine.LightType.Point;
                    break;
                case LightType.Spotlight:
                    lightObject.type = UnityEngine.LightType.Spot;
                    break;
            }

            lightObject.range = Range;
            lightObject.color = Color;
        }
    }
}
