using Unity.Entities;

public struct TimeChangeTracker: IComponentData{
    //fields here are dirty flags for systems
    //should be set true for each change in the temperature in the GameEnv component
    //each system should only close its designated flag
    public bool forTemperature;


    // Static method to set all flags to true
    // list all of the systems here that uses/dependent to this state
    public void SetAllFlagsTrue()
    {
        forTemperature = true;
    }
}