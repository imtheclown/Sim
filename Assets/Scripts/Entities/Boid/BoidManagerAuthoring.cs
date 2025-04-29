using Unity.Entities;
using UnityEngine;

class BoidManagerAuthoring: MonoBehaviour{
    [SerializeField] private float turnFactor;
    [SerializeField] private float centeringFactor;
    [SerializeField] private float matchingFactor;
    [SerializeField] private float avoidFactor;
    [SerializeField] private int startingBoidCount;
    [SerializeField] private GameObject agentPrefab;
    [SerializeField] private float criticalRange = 0.3f;
    [SerializeField] private float visualRange = 1f;
    [SerializeField] private float cellSize = 3;
    class Baker:Baker<BoidManagerAuthoring>{
        public override void Bake(BoidManagerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new BoidTuningFactors {
                turnFactor = authoring.turnFactor,
                centeringFactor = authoring.centeringFactor,
                matchingFactor = authoring.matchingFactor,
                avoidFactor = authoring.avoidFactor
            });
            AddComponent(entity, new BoidManager{
                boidToSpawnCount = authoring.startingBoidCount,
                boidSpawnedCount = 0,
                activeBoidCount = 0,
                agent = GetEntity(authoring.agentPrefab, TransformUsageFlags.Dynamic)
            });
            AddComponent(entity, new BoidAgentOtherFields{
                cellSize = authoring.cellSize,
                visualRange = authoring.visualRange,
                criticalRange = authoring.criticalRange
            });
        }
    }
}

public struct BoidTuningFactors : IComponentData {
    public float turnFactor;
    public float matchingFactor;
    public float centeringFactor;
    public float avoidFactor;
}

public struct BoidManager: IComponentData{
    public Entity agent;
    public int boidToSpawnCount;
    public int boidSpawnedCount;
    public int activeBoidCount;
}

public struct BoidAgentOtherFields: IComponentData{
    public float criticalRange;
    public float visualRange;
    public float cellSize;
}
