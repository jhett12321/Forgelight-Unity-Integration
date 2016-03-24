using System;
using UnityEngine;

namespace Forgelight.Utils
{
    public class MathUtils
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

                retVal = (((val - 0.0f) * newRange) / oldRange) + targetMin;
            }

            return retVal;
        }
    }
}
