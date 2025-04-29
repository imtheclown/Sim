using UnityEngine;
using Unity.Entities;

public class FeedAuthoring : MonoBehaviour
{
    class Baker: Baker<FeedAuthoring> {
        public override void Bake(FeedAuthoring authoring)
        {
            throw new System.NotImplementedException();
        }
    }
}

public struct FeedSpecs {
    public float content;
    public float timeBeforeExpiration;
}
