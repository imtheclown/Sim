using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;


class CommodityAuthoring : MonoBehaviour{
    [SerializeField] private float viewAngle;
    [SerializeField] private float viewRadius;
    class Baker: Baker<CommodityAuthoring>{
        public override void Bake(CommodityAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new CommodityBioInfo {
                weight = 5f,
                hungerLevel = 0.5f,
                feedIntake = 0,
                digestionRate = 0,
                growthFactor = 1,
                viewRadius = authoring.viewRadius,
                viewAngle = authoring.viewAngle
            });
            AddComponent(entity, new CommodityGrowthFactors{});
            AddComponent(entity, new CommodityTargetFeed{});

        }
    }
}

public struct CommodityBioInfo : IComponentData{
    public float weight; //in mg
    public float hungerLevel; //min 0, max 1
    public float feedIntake; // amount of feed consumed in mg
    public float digestionRate; //amount of feed digest per hour in mg
    public float growthFactor; //amount of weight gained in mg]

    public float viewRadius;
    public float viewAngle;
}

public struct CommodityGrowthFactors: IComponentData{
    public float capFeedConsumption;
}

public struct CommodityTargetFeed: IComponentData{
    public float3 targetPos;
    public bool hasTarget;
}




