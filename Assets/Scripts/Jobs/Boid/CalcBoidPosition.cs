using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public partial struct CalcBoidPosition : IJobEntity {
    [ReadOnly] public float td;
    [ReadOnly] public float leftLimit;
    [ReadOnly] public float rightLimit;
    [ReadOnly] public float frontLimit;
    [ReadOnly] public float backLimit;
    [ReadOnly] public float topLimit;
    [ReadOnly] public float bottomLimit;
    [ReadOnly] public float turnFactor;

    public Random RandomGenerator;

    public void Execute (ref BoidAgentMovement boidAgentMovement, ref LocalTransform localTransform, in CommodityBioInfo bioInfo){
        float3 pos = localTransform.Position;

        float3 futureDir = boidAgentMovement.futureDirection;
        
        if(math.any(math.isnan(futureDir))){
            futureDir = math.mul(localTransform.Rotation, new float3(0, 0, 0.5f));
        }
        futureDir = math.normalize(futureDir);
        if(pos.x <= leftLimit){
            futureDir.x += turnFactor;
        }
        if(pos.x >= rightLimit){
            futureDir.x -= turnFactor;
        }
        if(pos.y >= topLimit){
            futureDir.y -= turnFactor;
        }
        if(pos.y <= bottomLimit) {
            futureDir.y += turnFactor ;
        }
        if(pos.z >= frontLimit){
            futureDir.z -= turnFactor;
        }
        if(pos.z <= backLimit){
            futureDir.z += turnFactor;
        }
        futureDir = math.normalize(futureDir);

        futureDir.y *= 0.8f;

        boidAgentMovement.direction = futureDir;
        // float3 newPosition =  (futureDir * td * CalcMovementSpeed(1, bioInfo.hungerLevel)) + pos;
        float3 newPosition =  (futureDir * td * 2) + pos;

        // Clamp the position to ensure the boid stays within the defined limits
        newPosition.x = math.clamp(newPosition.x, leftLimit - 1, rightLimit + 1);
        newPosition.y = math.clamp(newPosition.y, bottomLimit - 1 , topLimit + 1);
        newPosition.z = math.clamp(newPosition.z, backLimit - 1 , frontLimit + 1);
        
        localTransform.Position = newPosition;
        localTransform.Rotation = quaternion.LookRotationSafe(new float3(futureDir), math.up());
        boidAgentMovement.currentPos = newPosition;
        boidAgentMovement.futureDirection = futureDir;
    }

        float CalcMovementSpeed(float baseSpeed, float hungerLevel)
    {
        const float hungerMovementBoost = 0.5f; // 50% more movement when starving
        hungerLevel = math.clamp(hungerLevel, 0f, 1f);

        return baseSpeed * (1f + hungerLevel * hungerMovementBoost);
    }
}