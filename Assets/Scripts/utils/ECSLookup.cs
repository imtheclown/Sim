using Unity.Entities;

public static class ECSLookup
{
    public static Entity GetSingletonEntitySafe<T>(EntityManager em) where T : unmanaged, IComponentData
    {
        var q = em.CreateEntityQuery(typeof(T));
        return q.CalculateEntityCount() > 0 ? q.GetSingletonEntity() : Entity.Null;
    }
}
