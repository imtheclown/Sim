
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct CalcBoidDirJob: IJobEntity{
    [ReadOnly] public NativeParallelMultiHashMap<int, BoidAgentMovement> boidMovementHashMap;
    [ReadOnly] public float cellSize;
    [ReadOnly] public float centeringFactor;
    [ReadOnly] public float matchingFactor;
    [ReadOnly] public float avoidingFactor;
    [ReadOnly] public float criticalRange;
    [ReadOnly] public float visualRange;

    public void Execute(ref BoidAgentMovement boidMovement, in BoidAgentVolume boidVolume,  in LocalTransform localTransform){
        float3 center = localTransform.Position;
        NativeArray<float3> endpoints = new NativeArray<float3>(8, Allocator.Temp);

        //edges of the boid volume
        float3 min = center - new float3(boidVolume.length, boidVolume.height, boidVolume.width) * 0.5f;
        float3 max = center + new float3(boidVolume.length, boidVolume.height, boidVolume.width) * 0.5f;

        //determine the endpoints of the cube occupied by the boid volume
        for (int i = 0; i < 8; i++)
        {
            int zIndex = i % 2;
            int yIndex = (i / 2) % 2;
            int xIndex = i / 4;

            endpoints[i] = new float3(
                (xIndex == 0) ? min.x : max.x,
                (yIndex == 0) ? min.y : max.y,
                (zIndex == 0) ? min.z : max.z
            );
        }

        //determine the cubes within the space where the boid volume overlaps
        NativeList<int> cellPositions = new NativeList<int>(Allocator.Temp);
        for (int i = 0; i < endpoints.Length; i++){
            int cellIndex = SpatialHashMap.Hash3D(endpoints[i], cellSize);

            if(!isPresentInArray(cellPositions, cellIndex)){
                cellPositions.Add(cellIndex);
            }
        }

        endpoints.Dispose();//dispose unused arrays

        //list of neighbors
        NativeList<BoidAgentMovement> neighbors = new NativeList<BoidAgentMovement>(Allocator.Temp);

        //get neighbors by considering all boids within the cubes that overlaps with the volume of the boid
        int count = 0;
        for(int i = 0; i < cellPositions.Length; i++){
            NativeParallelMultiHashMap<int,BoidAgentMovement>.Enumerator enumerator = boidMovementHashMap.GetValuesForKey(cellPositions[i]);
            while(enumerator.MoveNext()){
                if(count == 10) break;

                //select only boids that is within the designated radius centered at the boid
                if(math.lengthsq(center -enumerator.Current.currentPos) < math.square(visualRange)){
                    neighbors.Add(enumerator.Current); 
                    count++;
                }
            }
        }

        cellPositions.Dispose();
        float3 seperation = 0;
        float3 alignment = 0;
        float3 cohesion = 0;

        if(count < 2){ //this instance is included
            return;
        }

        //calculate boid vectors
        for(int i = 0; i < count; i++){
            if(math.lengthsq(center - neighbors[i].currentPos) <= math.square(criticalRange)){

                float3 offset = center - neighbors[i].currentPos;
                float dist = math.length(offset);

                //add to the separation the weighted direction from the boid to the neighbor
                //the nearer the neighbor the higher the weight
                if (dist < criticalRange)
                {
                    float weight = math.smoothstep(0f, criticalRange, criticalRange - dist);

                    seperation += offset * weight;
                }


            }
            alignment +=  neighbors[i].direction;
            cohesion +=  neighbors[i].currentPos;
        }
        float3 direction = float3.zero;

        direction += math.normalize(seperation * avoidingFactor);
        direction += math.normalize(alignment / count * matchingFactor);
        direction += math.normalize(((cohesion / count) - center) * centeringFactor);

        //add the new direction with the previous direction 
        //normalize the new direction as the former direction is already normalized
        
        boidMovement.futureDirection = math.normalize(math.lerp(boidMovement.direction, direction, 0.1f));

        neighbors.Dispose();

    }
    static bool isPresentInArray (NativeList<int> arr, int val){
        for (int i = 0; i < arr.Length; i++) 
        {
            if (arr[i] == val)
            {
                return true;
            }
        }
        return false;
    }
    
    

}