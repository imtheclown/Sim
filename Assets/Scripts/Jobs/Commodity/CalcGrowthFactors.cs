using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;

[BurstCompile]
public partial struct CalcGrowthFactors : IJobEntity
{
    [ReadOnly] public float temperature;

    public void Execute(ref CommodityGrowthFactors factors, ref CommodityBioInfo bioInfo)
    {
        factors.capFeedConsumption = CalcMaxFeedCap(bioInfo.weight, temperature);
    }

    float CalcMaxFeedCap(float weight, float temp)
    {
        float tempFactor = CalcTemperatureFactor(temp);
        float baseCoefficient = GetBaseCoefficient(weight);
        return baseCoefficient * weight * tempFactor;
    }

    float GetBaseCoefficient(float currentSize)
    {
        if (currentSize < 5f) return 0.08f;      // very young shrimp
        else if (currentSize < 15f) return 0.05f; // growing shrimp
        else return 0.03f;                       // near-harvest shrimp
    }
    float CalcTemperatureFactor(float temp)
    {
        float minTemp = 20f;
        float optimalTemp = 24f;
        float maxTemp = 28f;

        if (temp < minTemp || temp > maxTemp)
            return 0f; // No feeding at bad temps
        
        float diff = math.abs(temp - optimalTemp);
        return math.max(0f, 1f - diff / (optimalTemp - minTemp));
    }
}
