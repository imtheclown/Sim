using System.Reflection;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
//creates a multivalue hashmap
//each key is determine by the Localtransform of the agents
//basically the coordinates of the agent within the space 
public struct SpatialHashMap: IJobParallelFor {
    [ReadOnly] public NativeArray<BoidAgentMovement> boidMovements;
    [ReadOnly] public NativeArray<float3> positions;
    [ReadOnly] public int cellSize;
    public NativeParallelMultiHashMap<int, BoidAgentMovement>.ParallelWriter hashMap;

    public void Execute(int index){
        int hashKey = Hash3D(positions[index], cellSize);

        var movement = boidMovements[index];
        movement.currentPos = positions[index];

        hashMap.Add(hashKey, movement);
    }

    public static int Hash3D(float3 position, float cellsize){
        int x = (int)math.floor(position.x /cellsize);
        int y = (int)math.floor(position.y/cellsize);
        int z = (int)math.floor(position.z/cellsize);

        return (int)(x + y * cellsize + z * cellsize * cellsize);
    }
}