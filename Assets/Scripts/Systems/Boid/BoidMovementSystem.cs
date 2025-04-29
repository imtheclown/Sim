using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct BoidMovementSystem : ISystem{
    public void OnCreate(ref SystemState state){
        state.RequireForUpdate<BoidAgentMovement>();
        state.RequireForUpdate<GameConfig>();
        state.RequireForUpdate<PondSpecs>();
        state.RequireForUpdate<BoidTuningFactors>();
    }

    public void OnUpdate(ref SystemState state){
        var gameCfg = SystemAPI.GetSingleton<GameConfig>();
        if(gameCfg.isPaused){
            return;
        }
        var boidManager = SystemAPI.GetSingleton<BoidManager>();
        NativeParallelMultiHashMap<int, BoidAgentMovement> _boidMovementMap;
        _boidMovementMap = new NativeParallelMultiHashMap<int, BoidAgentMovement>(boidManager.activeBoidCount, Allocator.Persistent); 

        var entityQuery = SystemAPI.QueryBuilder().WithAll<BoidAgentMovement>().WithAll<LocalTransform>().Build();
        // terminate execution when there is no entities with BoidMovement and LocalToWorld compoents
        if(entityQuery.CalculateEntityCount() == 0){
            Debug.LogError("there is no existing entities with boid movement and localtransform");
            return;
        }

        NativeArray<BoidAgentMovement> boidMovements = entityQuery.ToComponentDataArray<BoidAgentMovement>(Allocator.TempJob);
        NativeArray<LocalTransform> localTransforms = entityQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob); 
                NativeArray<float3> positions = new NativeArray<float3>(localTransforms.Length, Allocator.TempJob); //create a native array for positions

        // Copy positions manually
        for (int i = 0; i < localTransforms.Length; i++)
        {
            positions[i] = localTransforms[i].Position;
        }
        // Dispose unnecessary data (NativeArray<LocalToWorld>)
        localTransforms.Dispose();

        _boidMovementMap.Clear();

        var boidAgentOtherFields = SystemAPI.GetSingleton<BoidAgentOtherFields>();

        var agentMappingJob = new SpatialHashMap{
            boidMovements = boidMovements,
            positions = positions,
            hashMap = _boidMovementMap.AsParallelWriter(),
            cellSize = (int)boidAgentOtherFields.cellSize,
        };

        var hashMapJobHandle = agentMappingJob.Schedule(boidMovements.Length, 64, state.Dependency);
        hashMapJobHandle.Complete();

        boidMovements.Dispose();
        positions.Dispose();

        var boidTuningFactor = SystemAPI.GetSingleton<BoidTuningFactors>();
        var boidOtherFields = SystemAPI.GetSingleton<BoidAgentOtherFields>();
        var calcBoidDirJob = new CalcBoidDirJob{
            boidMovementHashMap= _boidMovementMap,
            cellSize = (int)boidAgentOtherFields.cellSize,
            centeringFactor = boidTuningFactor.centeringFactor,
            matchingFactor = boidTuningFactor.matchingFactor,
            avoidingFactor = boidTuningFactor.avoidFactor,
            criticalRange = boidOtherFields.criticalRange,
            visualRange = boidAgentOtherFields.visualRange,
        };

        var calcBoidDirJobHandle = calcBoidDirJob.ScheduleParallel(state.Dependency);
        calcBoidDirJobHandle.Complete();

        _boidMovementMap.Dispose();

        //change the position of boids
        var pondSpecs = SystemAPI.GetSingleton<PondSpecs>();
        float3 halfSize = pondSpecs.getHalfSize();
        var calcBoidPositionJob = new CalcBoidPosition{
            td = SystemAPI.Time.DeltaTime,
            turnFactor = boidTuningFactor.turnFactor,
            leftLimit = pondSpecs.center.x - halfSize.x,
            rightLimit = pondSpecs.center.x + halfSize.x,
            frontLimit = pondSpecs.center.z + halfSize.z,
            backLimit = pondSpecs.center.z - halfSize.z,
            topLimit = pondSpecs.center.y + halfSize.y,
            bottomLimit = pondSpecs.center.y - halfSize.y,
            RandomGenerator = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, int.MaxValue))
        };

        var calcBoidPosJobHandle = calcBoidPositionJob.ScheduleParallel(state.Dependency);
        calcBoidPosJobHandle.Complete();
    }
}