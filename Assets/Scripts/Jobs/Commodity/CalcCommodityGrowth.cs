using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.VisualScripting;


public partial struct CalcCommodityGrowth: IJobEntity{
    [ReadOnly] public float assimilationEfficiency;
    [ReadOnly] public float q10;
    [ReadOnly] public float baseMetabolicRate;
    [ReadOnly] public float temperature;
    [ReadOnly] public float baseTemperature;
    public void Execute(ref CommodityBioInfo bioInfo){
        double metabolicMultiplier = math.pow(q10, (temperature - baseTemperature) / 10.0);
        double metabolicCost = baseMetabolicRate * bioInfo.weight * metabolicMultiplier;

        var amountOfFeed = bioInfo.digestionRate < bioInfo.feedIntake? bioInfo.digestionRate: bioInfo.feedIntake;

        // Calculate energy gained from feed
        double energyGain = amountOfFeed * assimilationEfficiency;

        // Net growth
        double netGrowth = energyGain - metabolicCost;

        // Prevent negative weight
        if (netGrowth < 0 && math.abs(netGrowth) > bioInfo.weight)
            netGrowth = -bioInfo.weight;

        bioInfo.weight += (float)netGrowth;
    }
}