using System;

public static class Utils {
    public static float Map(int value, int fromLow, int fromHigh, float toLow, float toHigh)
    {
        return (float)(value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
    }

    public static float Limit(float value, float min, float max) {
        return Math.Max(Math.Min(value, max), min);
    }

    public static float MapAndLimit(int value, int fromLow, int fromHigh, float toLow, float toHigh)
    {
        return Limit(Map(value, fromLow, fromHigh, toLow, toHigh), toLow, toHigh);
    }
}
