using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class FeedSpatialHashSystem : SystemBase
{
    public NativeParallelMultiHashMap<int, Entity> FeedSpatialMap;
    public float CellSize = 10f;

    protected override void OnCreate()
    {
        FeedSpatialMap = new NativeParallelMultiHashMap<int, Entity>(1000, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        if (FeedSpatialMap.IsCreated)
            FeedSpatialMap.Dispose();
    }

    protected override void OnUpdate()
    {
        FeedSpatialMap.Clear();

        var feedMap = FeedSpatialMap.AsParallelWriter();
        float cellSize = CellSize;

        var handle = Entities
            .WithAll<FeedSpecs>()
            .ForEach((Entity entity, in LocalTransform transform) =>
            {
                int2 cell = SpatialHashUtility.Hash(transform.Position, cellSize);
                int hash = SpatialHashUtility.HashInt(cell);
                feedMap.Add(hash, entity);
            }).ScheduleParallel(Dependency);

        Dependency = handle; // Update system dependency

    }
}
