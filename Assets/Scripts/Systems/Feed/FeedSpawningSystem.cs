using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct FeedSpawningSystem: ISystem{
    public void OnCreate(ref SystemState state){
        state.RequireForUpdate<FeedManager>();
        state.RequireForUpdate<PondSpecs>();
    }

    public void OnUpdate(ref SystemState state){
        var gameCfg = SystemAPI.GetSingleton<GameConfig>();
        if(gameCfg.isPaused){
            return;
        }

        var feedManager = SystemAPI.GetSingletonRW<FeedManager>();

        if(feedManager.ValueRO.feedToSpawnCount > 0){
            var ecb =SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                               .CreateCommandBuffer(state.WorldUnmanaged);
            
                        var pondSpecs = SystemAPI.GetSingleton<PondSpecs>();

            float3 spawnPosition = pondSpecs.center + new float3(
                UnityEngine.Random.Range(-pondSpecs.length, pondSpecs.length) * 0.5f,
                pondSpecs.height,
                UnityEngine.Random.Range(-pondSpecs.width, pondSpecs.width) * 0.5f
            );
            var entity = ecb.Instantiate(feedManager.ValueRO.agent) ;
            feedManager.ValueRW.feedToSpawnCount --;
            feedManager.ValueRW.currentFeedCount ++;
            feedManager.ValueRW.totalFeedSpawned ++;
            ecb.SetComponent(entity, LocalTransform.FromPositionRotationScale(spawnPosition, quaternion.identity, 0.3f));
        }
    }
}