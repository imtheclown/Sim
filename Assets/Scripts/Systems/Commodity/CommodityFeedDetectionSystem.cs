using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial class FeedCommoditySystem : SystemBase
{
    public NativeParallelMultiHashMap<int, Entity> FeedSpatialMap;
    public float CellSize = 10f;
    private EndSimulationEntityCommandBufferSystem ecbSystem;

    protected override void OnCreate()
    {
        FeedSpatialMap = new NativeParallelMultiHashMap<int, Entity>(50, Allocator.Persistent);
        ecbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        
    }

    protected override void OnDestroy()
    {
        if (FeedSpatialMap.IsCreated)
            FeedSpatialMap.Dispose();
    }

    protected override void OnUpdate()
    {
        var query = GetEntityQuery(ComponentType.ReadOnly<GameConfig>());

        // Access the singleton component
        var gameConfig = query.GetSingleton<GameConfig>();

        if(gameConfig.isPaused){
            return;
        }

        var feedManager = SystemAPI.GetSingleton<FeedManager>();
        
        if (!FeedSpatialMap.IsCreated || FeedSpatialMap.Capacity < feedManager.currentFeedCount)
        {
            if (FeedSpatialMap.IsCreated)
                FeedSpatialMap.Dispose();

            FeedSpatialMap = new NativeParallelMultiHashMap<int, Entity>(feedManager.currentFeedCount, Allocator.Persistent);
        }

        FeedSpatialMap.Clear();
            FeedSpatialMap.Clear();

            var feedMapWriter = FeedSpatialMap.AsParallelWriter();
            float cellSize = CellSize;

            var spatialHashJob = Entities
                .WithAll<FeedSpecs>()
                .ForEach((Entity entity, in LocalTransform transform) =>
                {
                    int2 cell = SpatialHashUtility.Hash(transform.Position, cellSize);
                    int hash = SpatialHashUtility.HashInt(cell);
                    feedMapWriter.Add(hash, entity);
                }).ScheduleParallel(Dependency);

        Dependency = spatialHashJob;

        var feedMapReader = FeedSpatialMap;
        var localTransformLookup = GetComponentLookup<LocalTransform>(true);
        var feedSpecsLookup = GetComponentLookup<FeedSpecs>(false);
        var toDestroyLookup = GetComponentLookup<ToDestroyTag>(true);
        var commodityBioInfoLookup = GetComponentLookup<CommodityBioInfo>(false); 
        var ecbParallel = ecbSystem.CreateCommandBuffer().AsParallelWriter();

        float cellSizeForDetection = CellSize; // safe copy

        var detectionJob = Entities
            .WithName("FishFeedDetection")
            .WithAll<CommodityBioInfo>()
            .WithReadOnly(feedMapReader)
            .WithReadOnly(localTransformLookup)
            .WithReadOnly(toDestroyLookup)
            .WithNativeDisableParallelForRestriction(feedSpecsLookup)
            .ForEach((Entity fishEntity, 
                    int entityInQueryIndex, 
                    ref CommodityTargetFeed targetData, 
                    ref CommodityBioInfo commodity, 
                    in LocalTransform commodityTransform
                ) =>
            {
                float3 commodityPos = commodityTransform.Position;
                float3 commodityForward = math.mul(commodityTransform.Rotation, new float3(0, 0, 1));

                int2 cell = SpatialHashUtility.Hash(commodityPos, cellSizeForDetection);
                int hash = SpatialHashUtility.HashInt(cell);

                float nearestDistance = float.MaxValue;
                Entity nearestFeed = Entity.Null;
                float3 nearestPos = float3.zero;

                if (feedMapReader.TryGetFirstValue(hash, out Entity feedEntity, out NativeParallelMultiHashMapIterator<int> it))
                {
                    do
                    {
                        if(commodity.hungerLevel < 0.1f){
                            targetData.hasTarget = false;
                            continue;
                        }
                        if (localTransformLookup.HasComponent(feedEntity))
                        {
                            var feedTransform = localTransformLookup[feedEntity];
                            float3 feedPos = feedTransform.Position;
                            float3 toFeed = feedPos - commodityPos;
                            float distance = math.length(toFeed);

                            if (distance <= commodity.viewRadius)
                            {
                                float3 dir = math.normalize(toFeed);
                                float dot = math.dot(commodityForward, dir);
                                float angle = math.degrees(math.acos(math.clamp(dot, -1f, 1f)));

                                if (angle <= commodity.viewAngle * 0.5f)
                                {
                                    if (distance < nearestDistance)
                                    {
                                        nearestDistance = distance;
                                        nearestFeed = feedEntity;
                                        nearestPos = feedPos;
                                    }
                                }
                            }
                        }
                    }
                    while (feedMapReader.TryGetNextValue(out feedEntity, ref it));
                }

                if (nearestFeed != Entity.Null)
                {
                    // Target feed found
                    targetData.targetPos = nearestPos;
                    targetData.hasTarget = true;
                    ecbParallel.SetComponent(entityInQueryIndex, fishEntity, targetData);

                    if (feedSpecsLookup.HasComponent(nearestFeed))
                    {
                        FeedSpecs feedSpecs = feedSpecsLookup[nearestFeed];
                        if (nearestDistance <= 0.3f)
                        {
                            float amount = feedSpecs.ReduceContent(3f); // or your custom bite size
                            if(!toDestroyLookup.HasComponent(nearestFeed) && amount == 0){
                                ecbParallel.AddComponent(entityInQueryIndex, nearestFeed, new ToDestroyTag{});
                            }
                            if(amount > 0){
                                commodity.feedIntake += amount;
                                ecbParallel.SetComponent(entityInQueryIndex, nearestFeed, feedSpecs);
                            }
                        }
                    }
                }
                else
                {
                    // No feed found
                    targetData.targetPos = float3.zero;
                    targetData.hasTarget = false;
                    ecbParallel.SetComponent(entityInQueryIndex, fishEntity, targetData);
                }
            })
            .ScheduleParallel(spatialHashJob); // depend on spatial hashing

        Dependency = detectionJob;
        ecbSystem.AddJobHandleForProducer(Dependency); // important for ECB playback
    }
}
