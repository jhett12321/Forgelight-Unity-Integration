namespace Forgelight.Utils
{
    public class MathUtils
    {
        public static float Remap(float val, float min, float max, float targetMin, float targetMax)
        {
            float retVal = 0.0f;
            float oldRange = (max - min);

            if (oldRange == 0)
            {
                retVal = targetMin;
            }

            else
            {
                float newRange = (targetMax - targetMin);

                retVal = (((val - min) * newRange) / oldRange) + targetMin;
            }

            return retVal;
        }
    }
}
