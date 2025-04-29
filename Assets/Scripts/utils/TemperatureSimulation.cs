using UnityEngine;

public static class TemperatureSimulator
{
    // Configuration - adjust as needed
    private static float baseMinTemp = 15f;   // Nighttime low
    private static float baseMaxTemp = 30f;   // Daytime high
    private static float dailyVariationRange = 1.5f; // +/- degrees for randomness
    private static float seasonalAmplitude = 5f;     // Peak shift due to seasons

    // Call this each frame or tick
    public static float GetTemperature(float hour, int month = 0)
    {
        // Daily cycle: simulate temperature based on hour (0-23)
        float normalizedHour = (hour - 6f) / 24f * 2f * Mathf.PI;
        float dailyCurve = Mathf.Sin(normalizedHour);

        // Add seasonal effect: month is 0-11
        float seasonNormalized = (month / 12f) * 2f * Mathf.PI;
        float seasonalOffset = Mathf.Sin(seasonNormalized) * seasonalAmplitude;

        // Random fluctuation (a bit different each time)
        float randomFluctuation = Random.Range(-dailyVariationRange, dailyVariationRange);

        // Final temperature
        float minTemp = baseMinTemp + seasonalOffset;
        float maxTemp = baseMaxTemp + seasonalOffset;
        float temp = Mathf.Lerp(minTemp, maxTemp, (dailyCurve + 1f) / 2f); // Lerp with [0,1] range

        return temp + randomFluctuation;
    }
}
