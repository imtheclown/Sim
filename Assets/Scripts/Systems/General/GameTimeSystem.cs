using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct GameTimeSystem : ISystem
{
    public void OnCreate(ref SystemState state){
        state.RequireForUpdate<GameTime>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var gameCfg = SystemAPI.GetSingleton<GameConfig>();
        //do not add game time when the game is paused
        if(gameCfg.isPaused){
            return;
        }
        var timeComp = SystemAPI.GetSingletonRW<GameTime>();

        float currentHour = timeComp.ValueRO.realTimeCounter + SystemAPI.Time.DeltaTime;
        if (currentHour >= gameCfg.secondsPerHour) //check if a game time hour has passed;
        {
            var timeTracker = SystemAPI.GetSingletonRW<TimeChangeTracker>();
            timeTracker.ValueRW.SetAllFlagsTrue();//set the states to true
            timeComp.ValueRW.realTimeCounter = 0;//reset counter
            timeComp.ValueRW.hours++; //increment in game hours

            if (timeComp.ValueRW.hours >= 24) //check if in game day has passed
            {
                timeComp.ValueRW.hours = 0;//reset the hours
                timeComp.ValueRW.days++; //increment in-days;

                if (timeComp.ValueRW.days >= 7)//check if an in-game week has passed
                {
                    timeComp.ValueRW.days = 0; //reset the days counter
                    timeComp.ValueRW.weeks++;   //increment in-game hours

                    if (timeComp.ValueRW.weeks >= 4) //check if a month has passed
                    {
                        timeComp.ValueRW.weeks = 0; //reset in-game week counter
                        timeComp.ValueRW.months++; //increment in-game month

                        if (timeComp.ValueRW.months >= 12) //check if a year has passed
                        {
                            timeComp.ValueRW.months = 0; //reset in-game month;
                            timeComp.ValueRW.years++; // incerement in-game year;
                            //year can be infinite
                        }
                    }
                }
            }
            UnityEngine.Debug.Log($"Time - Year: {timeComp.ValueRO.years}, Month: {timeComp.ValueRO.months}, Week: {timeComp.ValueRO.weeks}, Day: {timeComp.ValueRO.days}, Hour: {timeComp.ValueRO.hours}, RT: {timeComp.ValueRO.realTimeCounter}");
        }
        else
        {
            //update the current hour
            timeComp.ValueRW.realTimeCounter = currentHour;
        }
    }
}
