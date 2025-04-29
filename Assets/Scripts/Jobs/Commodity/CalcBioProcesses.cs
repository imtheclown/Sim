using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using System.Threading;

[BurstCompile]
public partial struct CaclBioProcessesJob : IJobEntity
{
    [ReadOnly] public float temperature;
    public NativeArray<int> remainingOxygen; // Oxygen stored as milligrams (mg/L * 1000)
    public EntityCommandBuffer.ParallelWriter ecb;

    public void Execute(Entity entity, [EntityIndexInQuery] int entityIndex, ref CommodityBioInfo bioInfo)
    {
        float tempFactor = CalcTemperatureFactor(temperature);
        float activityFactor = CalcActivityFactor(bioInfo.hungerLevel);

        float oxygenConsumption = GetBaseOxygenRate(bioInfo.weight) * bioInfo.weight * tempFactor * activityFactor;
        int oxygenToSubtract = (int)(oxygenConsumption * 1000f); // Convert to mg * 1000

        // Atomically subtract oxygen

        // If no more oxygen, shrimp dies
        if (remainingOxygen[0] <= 0)
        {
            ecb.DestroyEntity(entityIndex, entity);
        }
    }

    float CalcTemperatureFactor(float temp)
    {
        if (temp < 20f) return 0.7f;
        if (temp < 28f) return 1.0f;
        if (temp <= 32f) return 1.2f;
        return 1.4f;
    }

    float GetBaseOxygenRate(float weight)
    {
        if (weight < 1f) return 0.65f;   // Postlarvae
        else if (weight < 10f) return 0.4f; // Juvenile
        else return 0.3f;                // Adult
    }

    float CalcActivityFactor(float hungerLevel)
    {
        hungerLevel = math.clamp(hungerLevel, 0f, 1f);
        return 0.8f + (hungerLevel * 0.4f);
    }


}
