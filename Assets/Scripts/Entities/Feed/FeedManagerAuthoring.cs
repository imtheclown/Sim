using UnityEngine;
using Unity.Entities;

class FeedManagerAuthoring : MonoBehaviour{
    [SerializeField] private GameObject feed;
    [SerializeField] private int feedToSpawnCount = 100;
    class Baker: Baker<FeedManagerAuthoring> {
        public override void Bake(FeedManagerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new FeedManager{
                currentFeedCount = 0,
                totalFeedSpawned = 0,
                feedToSpawnCount = authoring.feedToSpawnCount,
                agent = GetEntity(authoring.feed, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct FeedManager : IComponentData{
    public int currentFeedCount;
    public int totalFeedSpawned;
    public int feedToSpawnCount;
    public Entity agent;
}