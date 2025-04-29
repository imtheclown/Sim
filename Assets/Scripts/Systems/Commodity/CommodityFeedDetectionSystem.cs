using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(FeedSpatialHashSystem))]
public partial class CommodityFeedDetectionSystem : SystemBase
{
    private FeedSpatialHashSystem spatialHashSystem;

    protected override void OnCreate()
    {
        spatialHashSystem = World.GetExistingSystemManaged<FeedSpatialHashSystem>();
    }

    protected override void OnUpdate()
    {
        var feedMap = spatialHashSystem.FeedSpatialMap;
        float cellSize = spatialHashSystem.CellSize;

        var localTransformLookup = GetComponentLookup<LocalTransform>(true);

        Entities
            .WithName("FishFeedDetection")
            .WithAll<CommodityBase>()
            .ForEach((Entity fishEntity, ref CommodityTargetFeed targetData, in CommodityBase commodity, in LocalTransform commodityTransform) =>
            {
                float3 commodityPos = commodityTransform.Position;
                float3 commodityForward = math.mul(commodityTransform.Rotation, new float3(0, 0, 1));

                int2 cell = SpatialHashUtility.Hash(commodityPos, cellSize);
                int hash = SpatialHashUtility.HashInt(cell);

                float nearestDistance = float.MaxValue;
                Entity nearestFeed = Entity.Null;

                if (feedMap.TryGetFirstValue(hash, out Entity feedEntity, out NativeParallelMultiHashMapIterator<int> it))
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
                                    }
                                }
                            }
                        }
                    }
                    while (feedMap.TryGetNextValue(out feedEntity, ref it));
                }

                // Always update targetData even if no feed found
                targetData.target = nearestFeed;

            })
            .WithReadOnly(feedMap)
            .WithReadOnly(localTransformLookup)
            .ScheduleParallel();
    }
}
