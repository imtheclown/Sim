using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
public class FeedAuthoring : MonoBehaviour
{
    [SerializeField] private float feedContent = 20;
    class Baker: Baker<FeedAuthoring> {
        public override void Bake(FeedAuthoring authoring)
        {
           var entity = GetEntity(TransformUsageFlags.Dynamic);
           AddComponent(entity, new FeedSpecs{
                content=authoring.feedContent
           });
        }
    }
}

public struct FeedSpecs: IComponentData {
    public float content; //in mg
    public float timeBeforeExpiration;

    public float ReduceContent(float biteSize)
    {
        content = math.max(0f, content - biteSize);
        return content;
    }
}
