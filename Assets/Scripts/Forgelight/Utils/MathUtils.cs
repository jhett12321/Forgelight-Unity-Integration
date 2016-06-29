using System;
using UnityEngine;

namespace Forgelight.Utils
{
    public static class MathUtils
    {
        public static float RemapProgress(float val, float targetMin, float targetMax)
        {
            float retVal;
            float oldRange = 1.0f;

            if (Math.Abs(oldRange) < Mathf.Epsilon)
            {
                retVal = targetMin;
            }

            else
            {
                float newRange = (targetMax - targetMin);

                retVal = (((val - 0.0f)*newRange)/oldRange) + targetMin;
            }

            return retVal;
        }

        private static Matrix4x4 GetInversionMatrix()
        {
            Matrix4x4 retval = new Matrix4x4();
            retval[0, 0] = -1;
            retval[0, 1] = 0;
            retval[0, 2] = 0;
            retval[0, 3] = 0;

            retval[1, 0] = 0;
            retval[1, 1] = 1;
            retval[1, 2] = 0;
            retval[1, 3] = 0;

            retval[2, 0] = 0;
            retval[2, 1] = 0;
            retval[2, 2] = 1;
            retval[2, 3] = 0;

            retval[3, 0] = 0;
            retval[3, 1] = 0;
            retval[3, 2] = 0;
            retval[3, 3] = 1;

            return retval;
        }

        public static Vector3 ToRadians(this Vector3 eulerAngles)
        {
            return new Vector3(eulerAngles.x * Mathf.Deg2Rad, eulerAngles.y * Mathf.Deg2Rad, eulerAngles.z * Mathf.Deg2Rad);
        }

        public static Matrix4x4 ConvertTransform(Vector3 pos, Vector3 rot, Vector3 scale, bool radians, bool swapRotXY)
        {
            if (radians)
            {
                rot.x *= Mathf.Rad2Deg;
                rot.y *= Mathf.Rad2Deg;
                rot.z *= Mathf.Rad2Deg;
            }

            if (swapRotXY)
            {
                float rotX = rot.y;

                rot.y = rot.x;
                rot.x = rotX;
            }

            Quaternion rotation = Quaternion.Euler(rot);

            Matrix4x4 m = Matrix4x4.TRS(pos, rotation, scale);
            return m * GetInversionMatrix();
        }

        /// <summary>
        /// Extract translation from transform matrix.
        /// </summary>
        /// <returns>
        /// Translation offset.
        /// </returns>
        public static Vector3 ExtractTranslationFromMatrix(this Matrix4x4 matrix)
        {
            Vector3 translate;
            translate.x = matrix.m03;
            translate.y = matrix.m13;
            translate.z = matrix.m23;
            return translate;
        }

        /// <summary>
        /// Extract rotation quaternion from transform matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <returns>
        /// Quaternion representation of rotation transform.
        /// </returns>
        public static Quaternion ExtractRotationFromMatrix(this Matrix4x4 matrix)
        {
            Vector3 forward;
            forward.x = matrix.m02;
            forward.y = matrix.m12;
            forward.z = matrix.m22;

            Vector3 upwards;
            upwards.x = matrix.m01;
            upwards.y = matrix.m11;
            upwards.z = matrix.m21;

            return Quaternion.LookRotation(forward, upwards);
        }

        /// <summary>
        /// Extract scale from transform matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <returns>
        /// Scale vector.
        /// </returns>
        public static Vector3 ExtractScaleFromMatrix(this Matrix4x4 matrix)
        {
            Vector3 scale = new Vector3(
                matrix.GetColumn(0).magnitude,
                matrix.GetColumn(1).magnitude,
                matrix.GetColumn(2).magnitude
                );
            if (Vector3.Cross(matrix.GetColumn(0), matrix.GetColumn(1)).normalized != (Vector3)matrix.GetColumn(2).normalized)
            {
                scale.x *= -1;
            }
            return scale;
        }

        /// <summary>
        /// Extract position, rotation and scale from TRS matrix.
        /// </summary>
        /// <param name="localPosition">Output position.</param>
        /// <param name="localRotation">Output rotation.</param>
        /// <param name="localScale">Output scale.</param>
        public static void DecomposeMatrix(this Matrix4x4 matrix, out Vector3 localPosition, out Quaternion localRotation, out Vector3 localScale)
        {
            localPosition = matrix.ExtractTranslationFromMatrix();
            localRotation = matrix.ExtractRotationFromMatrix();
            localScale = matrix.ExtractScaleFromMatrix();
        }

        /// <summary>
        /// Set transform component from TRS matrix.
        /// </summary>
        /// <param name="transform">Transform component.</param>
        public static void SetTransformFromMatrix(this Matrix4x4 matrix, Transform transform)
        {
            transform.localPosition = matrix.ExtractTranslationFromMatrix();
            transform.localRotation = matrix.ExtractRotationFromMatrix();
            transform.localScale = matrix.ExtractScaleFromMatrix();
        }

        /// <summary>
        /// Get translation matrix.
        /// </summary>
        /// <param name="offset">Translation offset.</param>
        /// <returns>
        /// The translation transform matrix.
        /// </returns>
        public static Matrix4x4 TranslationMatrix(Vector3 offset)
        {
            Matrix4x4 matrix = Matrix4x4.identity;
            matrix.m03 = offset.x;
            matrix.m13 = offset.y;
            matrix.m23 = offset.z;
            return matrix;
        }
    }
}
