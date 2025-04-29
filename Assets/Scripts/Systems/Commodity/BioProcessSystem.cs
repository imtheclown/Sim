using Unity.Entities;
using Unity.Collections;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct BioProcessSystem: ISystem{
    public void OnCreate(ref SystemState state){
        state.RequireForUpdate<PondWaterQuality>();
        state.RequireForUpdate<CommodityBioInfo>();
        state.RequireForUpdate<GameEnv>();
    }
    public void OnUpdate(ref SystemState state){
        // var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        // var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        // var gameEnv = SystemAPI.GetSingleton<GameEnv>();
        // var waterQual = SystemAPI.GetSingleton<PondWaterQuality>();
        // var oxygenRemaining = new NativeArray<int>(1, Allocator.TempJob);
        // // oxygenRemaining[0] = (int)(waterQual.dissolvedOxygen * 1000f); // Store as int (mg/L * 1000)

        // // var breathingJob = new CaclBioProcessesJob
        // // {
        // //     temperature = gameEnv.temperature,
        // //     remainingOxygen = oxygenRemaining,
        // //     ecb = ecb
        // // };

        // var breathingJobHandle = breathingJob.ScheduleParallel(state.Dependency);
        // breathingJobHandle.Complete();
        // oxygenRemaining.Dispose();
    }
}