using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

[BurstCompile]
public struct BoidJob : IJobParallelForTransform
{
    public NativeArray<Boid.Data> boids;
    public NativeArray<Vector3> wanderVectors;

    // environment variables
    public float deltaTime;
    public float boundSize;

    // boid variables
    public float maxVelocity;
    public float maxAcceleration;
    public float wanderWeight;
    public float wanderRadius;
    // add more variables here in the future

    public void Execute(int index, TransformAccess transform)
    {
        // select a boid from data array to process
        Boid.Data boid = boids[index];

        // calculate movement
        Vector3 acceleration = Vector3.ClampMagnitude(Combine(ref boid), maxAcceleration);
        Vector3 velocity = Vector3.ClampMagnitude(boid.velocity + acceleration * deltaTime, maxVelocity);
        boid.position = transform.position;
        boid.position += velocity * deltaTime;

        // apply updates
        boid.velocity = velocity;
        boid.acceleration = acceleration;
        transform.position = boid.position;
        boid.wanderVector = wanderVectors[index];
        boids[index] = boid; // reassign, is this necessary?
    }

    // implement vector operations below
    private Vector3 Combine(ref Boid.Data boid)
    {
        // combines all vector operations
        Vector3 combineVector  = Vector3.zero;

        combineVector +=
            AvoidBounds(ref boid) +
            Wander(ref boid) * wanderWeight;

        return combineVector.normalized;
    }

    private Vector3 Wander(ref Boid.Data boid)
    {
        // calculate wander vector
        boid.wanderTarget += boid.wanderVector;
        return Vector3.ClampMagnitude(boid.wanderTarget, wanderRadius).normalized;
    }

    private Vector3 AvoidBounds(ref Boid.Data boid)
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
