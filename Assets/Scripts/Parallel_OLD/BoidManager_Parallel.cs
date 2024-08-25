using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

[BurstCompile]
public class BoidManager_Parallel : MonoBehaviour
{
    [SerializeField] private Config config;
    [SerializeField] private GameObject boidPrefab;
    [SerializeField] private Material lineMaterial;

    private List<Boid_Parallel> boids = new List<Boid_Parallel>();            // used for object reference
    private NativeArray<Boid_Parallel.Data> boidsData;               // used for parallelization
    private TransformAccessArray transformAccessArray;      // used to update transforms

    private bool shouldDraw = false;
    private Unity.Mathematics.Random random;

    private void Awake()
    {
        // initialize nativearray and transformaccessarray
        transformAccessArray = new TransformAccessArray(config.boidsCount);
        boidsData = new NativeArray<Boid_Parallel.Data>(config.boidsCount, Allocator.Persistent);

        // initialize the random generator with a seed
        random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, int.MaxValue));

        // spawn in boids
        for (int i = 0; i < config.boidsCount; i++)
        {
            GameObject obj = Instantiate(
                boidPrefab,
                UnityEngine.Random.insideUnitSphere * (config.spawnRadius / 2f),
                Quaternion.identity
            );
            obj.gameObject.SetActive(true);
            boids.Add(obj.GetComponent<Boid_Parallel>());

            // setup nativearray and transformaccessarray
            transformAccessArray.Add(boids[i].transform);
            boidsData[i] = boids[i].GetComponent<Boid_Parallel>().data;
        }
    }

    private void Update()
    {
        BoidJob_Parallel job = new BoidJob_Parallel
        {
            // raw data parameters to pass into the job
            boids = boidsData,

            deltaTime = Time.deltaTime,
            boundSize = config.boundSize,

            maxVelocity = config.boidMaxVelocity,
            maxAcceleration = config.boidMaxAcceleration,
            wanderWeight = config.wanderWeight,
            wanderRadius = config.wanderRadius,

            random = random
        };

        JobHandle jobHandle = job.Schedule(transformAccessArray);
        jobHandle.Complete();

        for (int i = 0; i < boids.Count; i++)
        {
            boids[i].GetComponent<Boid_Parallel>().data = boidsData[i];
            boids[i].transform.position = boidsData[i].position;
        }
        shouldDraw = true;
    }

    private void OnRenderObject()
    {
        // ensure draw calls are synced with update
        if (!shouldDraw) return;

        foreach (Boid_Parallel boid in boids)
        {
            GL.PushMatrix();
            GL.Begin(GL.LINES);
            lineMaterial.SetPass(0);

            GL.Color(Color.red);
            GL.Vertex(boid.transform.position);
            GL.Vertex(boid.transform.position + (boid.data.velocity / 2));

            GL.End();
            GL.PopMatrix();
        }

        shouldDraw = false;
    }

    private void OnDestroy()
    {
        transformAccessArray.Dispose();
        boidsData.Dispose();
    }
}
