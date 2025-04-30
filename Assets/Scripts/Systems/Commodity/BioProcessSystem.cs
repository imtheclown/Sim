using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class PondBioProcess : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<TimeChangeTracker>();
    }
    protected override void OnUpdate()
    {
        var query = GetEntityQuery(ComponentType.ReadOnly<GameConfig>());

        // Access the singleton component
        var gameConfig = query.GetSingleton<GameConfig>();

        if(gameConfig.isPaused){
            return;
        }
        var timeTrackerQuery = GetEntityQuery(ComponentType.ReadWrite<TimeChangeTracker>());
        var timeTracker = timeTrackerQuery.GetSingletonRW<TimeChangeTracker>();
        if(!timeTracker.ValueRO.forBioProcessSystem){
            return;
        }

        float deltaTime = SystemAPI.Time.DeltaTime; // Time passed this frame in seconds
        Entities
            .WithName("PondBioProcess")
            .ForEach((ref CommodityBioInfo bioInfo, ref LocalTransform transform) =>
            {
                // Digestion process
                float digestAmount = bioInfo.feedIntake * bioInfo.digestionRate/4 ; // digestionRate is per hour
                digestAmount = math.min(digestAmount, bioInfo.feedIntake); // don't digest more than intake
                bioInfo.feedIntake -= digestAmount;

                // Growth based on digestion
                bioInfo.weight += digestAmount * bioInfo.growthFactor;

                // Update hunger level
                bioInfo.hungerLevel = 1f - math.saturate(bioInfo.feedIntake / bioInfo.maxFeedRate);

                // Cap values
                bioInfo.feedIntake = math.max(0f, bioInfo.feedIntake);
                bioInfo.weight = math.clamp(bioInfo.weight, 0f, bioInfo.maxWeight);
                bioInfo.hungerLevel = math.clamp(bioInfo.hungerLevel, 0f, 1f);

                float weightRatio = bioInfo.weight / bioInfo.maxWeight; // Ratio of current weight to max weight
                transform.Scale = math.clamp(weightRatio, 0f, 1f);
                bioInfo.maxFeedRate = CalculateMaxFeedRate(bioInfo.weight);
            })
            .ScheduleParallel();
        timeTracker.ValueRW.forBioProcessSystem = false;    
    }
    public static float CalculateMaxFeedRate(float weightMg)
    {
        float weightG = weightMg / 1000f;

        if (weightG < 1f) return weightMg * 0.20f;
        else if (weightG < 5f) return weightMg * 0.12f;
        else if (weightG < 15f) return weightMg * 0.06f;
        else if (weightG < 25f) return weightMg * 0.03f;
        else return weightMg * 0.02f;
    }
}
