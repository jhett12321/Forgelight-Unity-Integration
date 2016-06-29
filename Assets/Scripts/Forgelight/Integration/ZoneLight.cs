using Forgelight.Attributes;
using UnityEngine;
using LightType = Forgelight.Formats.Zone.LightType;

[ExecuteInEditMode]
[SelectionBase]
public class ZoneLight : MonoBehaviour
{
    [HideInInspector]
    public Light lightObject;

    public string Name;
    public string ColorName;

    [Header("Other Parameters")]
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

    #region Representable Parameters
    [Header("Representable Parameters")]
    private LightType type;
    [ExposeProperty]
    public LightType Type
    {
        get { return type; }
        set
        {
            if (lightObject == null)
            {
                return;
            }

            switch (value)
            {
                case LightType.Pointlight:
                    lightObject.type = UnityEngine.LightType.Point;
                    break;
                case LightType.Spotlight:
                    lightObject.type = UnityEngine.LightType.Spot;
                    break;
            }

            type = value;
        }
    }

    private float range;
    [ExposeProperty]
    public float Range
    {
        get
        {
            return range;
        }
        set
        {
            if (lightObject == null)
            {
                return;
            }

            lightObject.range = value;
            range = value;
        }
    }

    private Color color;
    public Color Color
    {
        get { return color; }
        set
        {
            if (lightObject == null)
            {
                return;
            }

            lightObject.color = value;
            color = value;
        }
    }

    #endregion
}
