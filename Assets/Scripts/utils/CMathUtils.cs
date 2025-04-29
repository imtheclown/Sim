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
}
