using Unity.Burst;
using Unity.Entities;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class DestroyEntitiesWithTagSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem ecbSystem;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecbParallel = ecbSystem.CreateCommandBuffer().AsParallelWriter();

        Entities
            .WithAll<ToDestroyTag>() // <<< Change this to whatever tag you want
            .ForEach((Entity entity, int entityInQueryIndex) =>
            {
                ecbParallel.DestroyEntity(entityInQueryIndex, entity);
            })
            .ScheduleParallel();

        ecbSystem.AddJobHandleForProducer(Dependency);
    }
}
