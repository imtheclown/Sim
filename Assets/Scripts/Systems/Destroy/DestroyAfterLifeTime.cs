using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class DestroyAfterLifetimeSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem ecbSystem;

    protected override void OnCreate()
    {
        ecbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var deltaTime = SystemAPI.Time.DeltaTime; // How much time passed this frame
        var ecbParallel = ecbSystem.CreateCommandBuffer().AsParallelWriter();

        Entities
            .WithName("DestroyAfterLifetime")
            .ForEach((Entity entity, int entityInQueryIndex, ref LifetimeComponent lifetime) =>
            {
                lifetime.timeLeft -= deltaTime;

                if (lifetime.timeLeft <= 0f)
                {
                    ecbParallel.DestroyEntity(entityInQueryIndex, entity);
                }
            })
            .ScheduleParallel();

        ecbSystem.AddJobHandleForProducer(Dependency);
    }
}
