namespace ForgelightUnity.Forgelight.Utils
{
    using UnityEngine;

    public enum TransformMode
    {
        Standard,
        Area
    }

    public struct TransformData
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;

        public TransformData(Vector3 pos, Vector3 rot, Vector3 scale)
        {
            Position = pos;
            Rotation = rot;
            Scale = scale;
        }
    }

    public static class MathUtils
    {
        public static float Remap01(this float value, float targetMin, float targetMax)
        {
            return value.Remap(0, 1, targetMin, targetMax);
        }

        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public static Vector3 ToRadians(this Vector3 eulerAngles)
        {
            return new Vector3(eulerAngles.x * Mathf.Deg2Rad, eulerAngles.y * Mathf.Deg2Rad, eulerAngles.z * Mathf.Deg2Rad);
        }

        public static TransformData ConvertTransform(Vector3 pos, Vector3 rot, Vector3 scale, bool fromForgelight, TransformMode transformMode)
        {
            if (fromForgelight)
            {
                rot.x *= Mathf.Rad2Deg;
                rot.y *= Mathf.Rad2Deg;
                rot.z *= Mathf.Rad2Deg;
            }

            //Make sure we are within 360 degrees.
            rot.x = Mathf.Repeat(rot.x, 360.0f);
            rot.y = Mathf.Repeat(rot.y, 360.0f);
            rot.z = Mathf.Repeat(rot.z, 360.0f);

            //Flip our x axis.
            pos.x = -pos.x;

            //Don't perform any transform modifications to area definitions.
            if (transformMode == TransformMode.Area)
            {
                return new TransformData(pos, rot, scale);
            }

            //x becomes y, y becomes x, z is inversed.
            float rotX;
            float rotY;

            if (fromForgelight)
            {
                rotX = rot.y;
                rotY = -rot.x;
            }
            else
            {
                rotX = -rot.y;
                rotY = rot.x;
            }

            float rotZ = -rot.z;

            rot.x = rotX;
            rot.y = rotY;
            rot.z = rotZ;

            return new TransformData(pos, rot, scale);
        }
    }
}
