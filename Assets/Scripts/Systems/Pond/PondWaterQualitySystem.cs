using UnityEngine;
using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct PondWaterQualitySystem : ISystem{
    public void OnCreate(ref SystemState state){
        state.RequireForUpdate<GameEnv>();
        state.RequireForUpdate<TempChangeTracker>();
        state.RequireForUpdate<PondWaterQuality>();
    }

    public void OnUpdate(ref SystemState state){
        var gameCfg = SystemAPI.GetSingleton<GameConfig>();
        if(gameCfg.isPaused){
            return;
        }
        
        var tempFlag = SystemAPI.GetSingletonRW<TempChangeTracker>();
        if(tempFlag.ValueRO.forPondWaterQualitySystem){
            var gameEnv = SystemAPI.GetSingleton<GameEnv>();

            var waterQual = SystemAPI.GetSingletonRW<PondWaterQuality>();
            float DO = WaterQualityUtil.UpdateDissolvedOxygen(gameEnv.temperature, waterQual.ValueRO.dissolvedOxygen);
            float ammonia = WaterQualityUtil.UpdateAmmonia(waterQual.ValueRO.ammonia, waterQual.ValueRO.dissolvedOxygen);
            float pH = WaterQualityUtil.UpdatePH(waterQual.ValueRO.pH, waterQual.ValueRO.ammonia);

            waterQual.ValueRW.dissolvedOxygen = DO;
            waterQual.ValueRW.ammonia = ammonia;
            waterQual.ValueRW.pH = pH;

            tempFlag.ValueRW.forPondWaterQualitySystem = false;

            Debug.Log($"DO: {DO} AMMONIA: {ammonia} PH: {pH}");
        }
    }
}