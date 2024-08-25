using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

[BurstCompile]
public struct BoidJob_Parallel : IJobParallelForTransform
{
    public NativeArray<Boid_Parallel.Data> boids;

    // environment variables
    public float deltaTime;
    public float boundSize;

    // boid variables
    public float maxVelocity;
    public float maxAcceleration;
    public float wanderWeight;
    public float wanderRadius;
    // add more variables here in the future

    public Unity.Mathematics.Random random;

    public void Execute(int index, TransformAccess transform)
    {
        // select a boid from data array to process
        Boid_Parallel.Data boid = boids[index];

        // calculate movement
        Vector3 acceleration = Vector3.ClampMagnitude(Combine(ref boid), maxAcceleration);
        Vector3 velocity = Vector3.ClampMagnitude(boid.velocity + acceleration * deltaTime, maxVelocity);

        // apply updates
        boid.velocity = velocity;
        boid.acceleration = acceleration;
        transform.position = boid.position;
        boids[index] = boid; // reassign, is this necessary?
    }

    // implement vector operations below
    private Vector3 Combine(ref Boid_Parallel.Data boid)
    {
        // combines all vector operations
        Vector3 combineVector  = Vector3.zero;

        combineVector +=
            AvoidBounds(ref boid) +
            Wander(ref boid) * wanderWeight;

        return combineVector.normalized;
    }

    private Vector3 Wander(ref Boid_Parallel.Data boid)
    {
        Vector3 randomVector = new Vector3(
            random.NextFloat(-1f, 1f),
            random.NextFloat(-1f, 1f),
            random.NextFloat(-1f, 1f)
        ).normalized;

        // calculate wander vector
        boid.wanderTarget += randomVector;
        return Vector3.ClampMagnitude(boid.wanderTarget, wanderRadius).normalized;
    }

    private Vector3 AvoidBounds(ref Boid_Parallel.Data boid)
    {
        // calculate avoidance0 vector
        Vector3 avoidanceVector = Vector3.zero;
        float edgeDistance = boundSize * 0.3f;

        avoidanceVector.x = boid.position.x < -edgeDistance ? 1 : boid.position.x > edgeDistance ? -1 : 0;
        avoidanceVector.y = boid.position.y < -edgeDistance ? 1 : boid.position.y > edgeDistance ? -1 : 0;
        avoidanceVector.z = boid.position.z < -edgeDistance ? 1 : boid.position.z > edgeDistance ? -1 : 0;

        return avoidanceVector.normalized * 100f; // adjust mutiplier
    }
}
