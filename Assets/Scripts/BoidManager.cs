using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

[BurstCompile]
public class BoidManager : MonoBehaviour
{
    [SerializeField] private Config config;
    [SerializeField] private GameObject boidPrefab;
    [SerializeField] private Material lineMaterial;

    private List<GameObject> boidObjects = new List<GameObject>();
    private NativeArray<Boid.Data> boidsData;
    private NativeArray<Vector3> randomValues;
    private TransformAccessArray transformAccessArray;

    private bool shouldDraw = false;

    private void Awake()
    {
        // spawn in boids
        for (int i = 0; i < config.boidsCount; i++)
        {
            Vector3 spawnPosition = UnityEngine.Random.insideUnitSphere * (config.spawnRadius / 1.5f);
            GameObject obj = Instantiate(boidPrefab, spawnPosition, Quaternion.identity);
            obj.gameObject.SetActive(true);
            boidObjects.Add(obj);
        }

        // init nativearray and transformaccessarray
        boidsData = new NativeArray<Boid.Data>(boidObjects.Count, Allocator.Persistent);
        transformAccessArray = new TransformAccessArray(boidObjects.Count);
        randomValues = new NativeArray<Vector3>(boidObjects.Count, Allocator.Persistent);

        for (int i = 0;i < boidObjects.Count;i++)
        {
            var transform = boidObjects[i].transform;
            transformAccessArray.Add(transform);
            var boid = boidObjects[i].GetComponent<Boid>();
            boidsData[i] = boid.data;

            randomValues[i] = new Vector3(
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f)
            );
        }
    }

    private void Update()
    {
        BoidJob job = new BoidJob
        {
            // raw data parameters to pass into the job
            boids = boidsData,
            wanderWeight = config.wanderWeight,
            wanderRadius = config.wanderRadius,
            boundSize = config.boundSize,
            boidMaxVelocity = config.boidMaxVelocity,
            boidMaxAcceleration = config.boidMaxAcceleration,
            randomValues = randomValues,
            deltaTime = Time.deltaTime
        };

        JobHandle jobHandle = job.Schedule(transformAccessArray);
        jobHandle.Complete();

        // update boid data in gameobjects
        for (int i = 0; i < boidObjects.Count; i++)
        {
            var boid = boidObjects[i].GetComponent<Boid>();
            boid.data = boidsData[i];
            boidObjects[i].transform.position = boid.data.position;
            boidObjects[i].GetComponent<Boid>().data = boid.data;
        }

        shouldDraw = true;
    }

    private void OnRenderObject()
    {
        // ensure draw calls are synced with update
        if (!shouldDraw) return;

        foreach(GameObject boidObj in boidObjects)
        {
            GL.PushMatrix();
            GL.Begin(GL.LINES);
            lineMaterial.SetPass(0);

            GL.Color(Color.red);
            GL.Vertex(boidObj.transform.position);
            GL.Vertex(boidObj.transform.position + boidObj.GetComponent<Boid>().data.velocity);

            GL.End();
            GL.PopMatrix();
        }

        shouldDraw = false;
    }

    private void OnDestroy()
    {
        transformAccessArray.Dispose();
        boidsData.Dispose();
        randomValues.Dispose();
    }
}


[BurstCompile]
public struct BoidJob : IJobParallelForTransform
{
    public NativeArray<Boid.Data> boids;
    public NativeArray<Vector3> randomValues;
    public float wanderWeight;
    public float wanderRadius;
    public float boundSize;
    public float boidMaxVelocity;
    public float boidMaxAcceleration;
    public float deltaTime;

    public void Execute(int index, TransformAccess transform)
    {
        Boid.Data boid = boids[index];

        // Calculate wander and avoid bounds
        Vector3 wander = Wander(ref boid, randomValues[index]);
        Vector3 avoidBounds = AvoidBounds(ref boid);

        // Combine forces and update position
        Vector3 acceleration = Vector3.ClampMagnitude(wander * wanderWeight + avoidBounds, boidMaxAcceleration);
        Vector3 velocity = Vector3.ClampMagnitude(boid.velocity + acceleration * deltaTime, boidMaxVelocity);
        boid.position += velocity * deltaTime;

        // Apply updates
        boid.velocity = velocity;
        boid.acceleration = acceleration;

        // Set the transform position
        transform.position = boid.position;
        boids[index] = boid;
    }

    private Vector3 Wander(ref Boid.Data boid, Vector3 randomValue)
    {
        // Apply wander force using precomputed random value
        boid.wanderTarget += randomValue.normalized;
        return Vector3.ClampMagnitude(boid.wanderTarget, wanderRadius).normalized;
    }

    private Vector3 AvoidBounds(ref Boid.Data boid)
    {
        // Calculate avoidance vector
        Vector3 avoidanceVector = Vector3.zero;
        float edgeDistance = boundSize * 0.3f;

        avoidanceVector.x = boid.position.x < -edgeDistance ? 1 : boid.position.x > edgeDistance ? -1 : 0;
        avoidanceVector.y = boid.position.y < -edgeDistance ? 1 : boid.position.y > edgeDistance ? -1 : 0;
        avoidanceVector.z = boid.position.z < -edgeDistance ? 1 : boid.position.z > edgeDistance ? -1 : 0;

        return avoidanceVector * 100f;
    }
}
