using UnityEngine;
using Unity.Entities;

class FeedManagerAuthoring : MonoBehaviour{
    class Baker: Baker<FeedManagerAuthoring> {
        public override void Bake(FeedManagerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new FeedManager{
                currentFeedCount = 0,
                totalFeedSpawned = 0,
                feedToSpawnCount = 0,
            });
        }
    }
}

public struct FeedManager : IComponentData{
    public int currentFeedCount;
    public int totalFeedSpawned;
    public int feedToSpawnCount;
}