public static class Utils {
    public static float Map(int value, int fromLow, int fromHigh, int toLow, int toHigh)
    {
        return (float)(value - fromLow) * (toHigh - toLow) / (fromHigh - fromLow) + toLow;
    }
}