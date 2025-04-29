using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
public class PondAuthoring : MonoBehaviour
{
    [SerializeField] private float width;
    [SerializeField] private float height;
    [SerializeField] private float length;
    [SerializeField] private float3 center;
    class Baker: Baker<PondAuthoring>{
        public override void Bake(PondAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new PondWaterQuality{
                dissolvedOxygen = 70,
                ammonia = 0.2f,
                pH = 7.5f
            });
            AddComponent(entity, new PondSpecs {
                center = authoring.center,
                width = authoring.width,
                height = authoring.height,
                length = authoring.length
            });
        }
    }
}

public struct PondWaterQuality : IComponentData{
    public float dissolvedOxygen;
    public float ammonia;
    public float pH;
}

public struct PondSpecs: IComponentData{
    public float3 center;
    public float length;
    public float width;
    public float height;

    public float3 getHalfSize(){
        return new float3(length, height, width) * 0.5f;
    }
}


