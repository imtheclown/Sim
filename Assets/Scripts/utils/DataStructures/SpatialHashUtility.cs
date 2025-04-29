using Unity.Mathematics;

public static class SpatialHashUtility
{
    public static int2 Hash(float3 position, float cellSize)
    {
        return new int2(
            (int)math.floor(position.x / cellSize),
            (int)math.floor(position.z / cellSize)
        );
    }

    public static int HashInt(int2 cell)
    {
        // Simple way to compress int2 to one int key
        return cell.x * 73856093 ^ cell.y * 19349663;
    }
}
