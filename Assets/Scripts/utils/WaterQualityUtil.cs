using UnityEngine;

public static class WaterQualityUtil
{
    // Compute max DO based on temperature
    public static float ComputeMaxDO(float temperature)
    {
        return Mathf.Clamp(14f - 0.3f * (temperature - 20f), 5f, 14f);
    }

    // Update DO based on current value and temperature
    public static float UpdateDissolvedOxygen(float temperature, float currentDO)
    {
        float DOmax = ComputeMaxDO(temperature);
        float depletion = 0.01f + 0.001f * (temperature - 20f);
        float recovery = Mathf.Min(0.005f, DOmax - currentDO);
        return Mathf.Clamp(currentDO - depletion + recovery, 0f, DOmax);
    }

    // Update Ammonia based on current value and DO
    public static float UpdateAmmonia(float currentNH3, float currentDO)
    {
        float newNH3 = currentNH3 + 0.002f;

        if (currentDO > 5f)
        {
            newNH3 -= 0.001f * (currentDO - 5f);
        }

        return Mathf.Clamp(newNH3, 0f, 10f);
    }

    // Update pH based on current NH3
    public static float UpdatePH(float currentPH, float currentNH3)
    {
        return Mathf.Clamp(currentPH - 0.001f * currentNH3 + 0.0005f, 6f, 8.5f);
    }
}
