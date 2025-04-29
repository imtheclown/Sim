using UnityEngine;
using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct GameEnvSystem: ISystem{
    public void OnCreate(ref SystemState state){
        state.RequireForUpdate<GameEnv>();
        state.RequireForUpdate<TempChangeTracker>();
    }

    public void OnUpdate(ref SystemState state){
        var gameCfg = SystemAPI.GetSingleton<GameConfig>();
        if(gameCfg.isPaused){
            return;
        }

        var timeState = SystemAPI.GetSingletonRW<TimeChangeTracker>();
        if(timeState.ValueRO.forTemperature){
            var timeComp = SystemAPI.GetSingletonRW<GameTime>();
            float newTemp = TemperatureSimulator.GetTemperature(timeComp.ValueRO.hours, timeComp.ValueRO.months);
            var envComp = SystemAPI.GetSingletonRW<GameEnv>();
            envComp.ValueRW.temperature = newTemp;
            timeState.ValueRW.forTemperature = false; //set the dirty flag to false
            var tempState = SystemAPI.GetSingletonRW<TempChangeTracker>();
            tempState.ValueRW.SetAllFlagsTrue();
        }
    }
}
