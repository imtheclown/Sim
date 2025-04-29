using UnityEngine;
using Unity.Entities;
public class GameCfgAuthoring : MonoBehaviour
{
    [SerializeField] private float secondsPerHour = 1;
    class Baker: Baker<GameCfgAuthoring>{
        public override void Bake(GameCfgAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new GameConfig{
                isPaused = false,
                secondsPerHour = authoring.secondsPerHour
            });
            AddComponent(entity, new GameTime{
                realTimeCounter = 0,
                hours = 0,
                days = 0,
                weeks= 0,
                months = 0,
                years= 0
            });
            AddComponent(entity, new GameEnv{});
            AddComponent(entity, new TimeChangeTracker{
                forTemperature = false,
            });
            AddComponent(entity, new TempChangeTracker{
                forPondWaterQualitySystem = false
            });
        }
    }
}

public struct GameConfig : IComponentData{
    public bool isPaused;
    public float secondsPerHour;
}

public struct GameTime: IComponentData{
    public float realTimeCounter; 
    public int hours;
    public int days;
    public int weeks;
    public int months;
    public int years;
}

public struct GameEnv: IComponentData{
    public float temperature;
}
