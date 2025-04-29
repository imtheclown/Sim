using Unity.Mathematics;

public static class CMathUtils
{
    public static float3 SoftClampFloat3(float3 value, float max = 0.2f)
    {
        return max * math.tanh(value / max);
    }

    public static float SoftClamp(float value, float max = 0.2f)
    {
        return max * math.tanh(value / max);
    }
    public static string FormatNumber(int number)
    {
        if (number >= 1_000_000)
            return $"{number / 1_000_000}m"; // Millions
        if (number >= 1_000)
            return $"{number / 1_000}k";     // Thousands
        if (number >= 100)
            return $"{number/100}h";              // 100-999 as normal
        return number.ToString();            // <100 as normal
    }
}
