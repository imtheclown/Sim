using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
public class BoidAuthoring: MonoBehaviour{
    [SerializeField] private float length;
    [SerializeField] private float width;
    [SerializeField] private float height;
    class Baker: Baker<BoidAuthoring>{
        public override void Bake(BoidAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new BoidAgentMovement{
            });
            AddComponent(entity, new BoidAgentVolume{
            });
        }
    }
}

public struct BoidAgentMovement : IComponentData{
    public float3 direction;
    public float3 futureDirection;
    public float3 currentPos;
}

public struct BoidAgentVolume : IComponentData{
    public float length;
    public float width;
    public float height;
}
