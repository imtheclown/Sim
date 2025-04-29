using Unity.Entities;

public struct TempChangeTracker : IComponentData{
    public bool forPondWaterQualitySystem;

    public void SetAllFlagsTrue()
    {
        forPondWaterQualitySystem = true;
    }
}