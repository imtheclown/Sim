using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
public class FeedAuthoring : MonoBehaviour
{
    [SerializeField] private float feedContent = 20;
    [SerializeField] private float lifeLimit = 5;
    class Baker: Baker<FeedAuthoring> {
        public override void Bake(FeedAuthoring authoring)
        {
           var entity = GetEntity(TransformUsageFlags.Dynamic);
           AddComponent(entity, new FeedSpecs{
                content=authoring.feedContent
           });
        //    AddComponent(entity, new LifetimeComponent{
        //     timeLeft= authoring.lifeLimit
        //    });
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
