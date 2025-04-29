using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs; // For JobHandle

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class FeedCommoditySystem : SystemBase
{
    public NativeParallelMultiHashMap<int, Entity> FeedSpatialMap;
    public float CellSize = 10f;
    private EndSimulationEntityCommandBufferSystem ecbSystem;

    protected override void OnCreate()
    {
        // Initialize the spatial hash map
        FeedSpatialMap = new NativeParallelMultiHashMap<int, Entity>(1000, Allocator.Persistent);
        ecbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnDestroy()
    {
        // Dispose of the spatial hash map
        if (FeedSpatialMap.IsCreated)
            FeedSpatialMap.Dispose();
    }

    protected override void OnUpdate()
    {
        // Step 1: Update the spatial hash map (FeedSpatialHashSystem functionality)
        FeedSpatialMap.Clear();

        var feedMapWriter = FeedSpatialMap.AsParallelWriter();

        float cellSize = CellSize;

        JobHandle spatialHashJob = Entities
            .WithAll<FeedSpecs>()
            .ForEach((Entity entity, in LocalTransform transform) =>
            {
                int2 cell = SpatialHashUtility.Hash(transform.Position, cellSize);
                int hash = SpatialHashUtility.HashInt(cell);

                feedMapWriter.Add(hash, entity);
            }).ScheduleParallel(Dependency);

        // Update the system's Dependency
        Dependency = spatialHashJob;

        // Step 2: Detect feed for entities (CommodityFeedDetectionSystem functionality)
        var feedMapReader = FeedSpatialMap; // Pass feed map as a local variable
        var localTransformLookup = GetComponentLookup<LocalTransform>(true);
        var feedSpecsLookup = GetComponentLookup<FeedSpecs>(false);
        var ecbParallel = ecbSystem.CreateCommandBuffer().AsParallelWriter();

        JobHandle detectionJob = Entities
            .WithName("FishFeedDetection")
            .WithAll<CommodityBase>()
            .ForEach((Entity fishEntity, int entityInQueryIndex, ref CommodityTargetFeed targetData, in CommodityBase commodity, in LocalTransform commodityTransform) =>
            {
                float3 commodityPos = commodityTransform.Position;
                float3 commodityForward = math.mul(commodityTransform.Rotation, new float3(0, 0, 1));

                int2 cell = SpatialHashUtility.Hash(commodityPos, cellSize);
                int hash = SpatialHashUtility.HashInt(cell);

                float nearestDistance = float.MaxValue;
                Entity nearestFeed = Entity.Null;
                float3 nearestPos = float3.zero;

                if (feedMapReader.TryGetFirstValue(hash, out Entity feedEntity, out NativeParallelMultiHashMapIterator<int> it))
                {
                    do
                    {
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
                // Always update targetData even if no feed is found
                if(nearestFeed != Entity.Null){
                    targetData.targetPos = nearestPos;
                    targetData.hasTarget = true;
                    ecbParallel.SetComponent(entityInQueryIndex, fishEntity, targetData);
                    if(feedSpecsLookup.HasComponent(nearestFeed)){
                        FeedSpecs feedSpecs = feedSpecsLookup[nearestFeed];
                        if(nearestDistance <= 0.3f){
                            feedSpecs.ReduceContent(3);
                        }
                        ecbParallel.SetComponent(entityInQueryIndex, nearestFeed, feedSpecs);
                    }
                }else{
                    targetData.hasTarget = false;
                    ecbParallel.SetComponent(entityInQueryIndex, fishEntity, targetData);
                }

            })
            .WithReadOnly(feedMapReader)
            .WithReadOnly(localTransformLookup)
            .ScheduleParallel(spatialHashJob); // Ensure this job depends on the spatial hash job

        // Update the system's Dependency
        Dependency = detectionJob;
    }
}