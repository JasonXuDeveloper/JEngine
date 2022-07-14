namespace BM
{
    public static class ValueHelper
    {
        public static float GetMinValue(float value1, float value2)
        {
            if (value1 > value2)
            {
                return value2;
            }
            return value1;
        }
    }
}