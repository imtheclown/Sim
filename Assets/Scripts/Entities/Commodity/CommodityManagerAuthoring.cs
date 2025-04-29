using UnityEngine;
using Unity.Entities;

class CommodityManagerAuthoring : MonoBehaviour{
    [SerializeField] private float assimilationEfficiency;
    [SerializeField] private float baseMetabolicRate;
    [SerializeField] private float q10;
    [SerializeField] private float baseTemperature;
    class Baker: Baker<CommodityManagerAuthoring>{
        public override void Bake(CommodityManagerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new CommodityManager{
                assimilationEfficiency = authoring.assimilationEfficiency,
                baseMetabolicRate = authoring.baseMetabolicRate,
                q10 = authoring.q10,
                baseTemperature = authoring.baseTemperature,
            });
        }
    }
}

public struct CommodityManager: IComponentData{
    public float assimilationEfficiency;
    public float baseMetabolicRate;
    public float q10;
    public float baseTemperature;
}