using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using System.Diagnostics;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct BoidSpawnSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BoidManager>();
        state.RequireForUpdate<BoidTuningFactors>();
        state.RequireForUpdate<PondSpecs>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var gameCfg = SystemAPI.GetSingleton<GameConfig>();
        if (gameCfg.isPaused)
            return;

        var boidManager = SystemAPI.GetSingletonRW<BoidManager>();

        if (boidManager.ValueRO.boidToSpawnCount > 0)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                               .CreateCommandBuffer(state.WorldUnmanaged);

            var pondSpecs = SystemAPI.GetSingleton<PondSpecs>();

            float3 spawnPosition = pondSpecs.center + new float3(
                UnityEngine.Random.Range(-pondSpecs.length, pondSpecs.length),
                UnityEngine.Random.Range(-pondSpecs.height, pondSpecs.height),
                UnityEngine.Random.Range(-pondSpecs.width, pondSpecs.width)
            );

            UnityEngine.Debug.Log($"spawn position: ${spawnPosition}");
            var entity = ecb.Instantiate(boidManager.ValueRO.agent);
            ecb.SetComponent(entity, LocalTransform.FromPositionRotationScale(spawnPosition, quaternion.identity, 0.3f));

            boidManager.ValueRW.boidToSpawnCount--;
            boidManager.ValueRW.boidSpawnedCount++;
            boidManager.ValueRW.activeBoidCount++;
        }
    }
}
