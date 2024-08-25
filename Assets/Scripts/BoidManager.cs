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

    private List<Boid> boids = new List<Boid>();            // used for object reference
    private NativeArray<Boid.Data> boidsData;               // used for parallelization
    private NativeArray<Vector3> wanderVectors;             // used in Wander()
    private TransformAccessArray transformAccessArray;      // used to update transforms

    private bool shouldDraw = false;

    private void Awake()
    {
        // initialize nativearray and transformaccessarray
        transformAccessArray = new TransformAccessArray(config.boidsCount);
        boidsData = new NativeArray<Boid.Data>(config.boidsCount, Allocator.Persistent);
        wanderVectors = new NativeArray<Vector3>(config.boidsCount, Allocator.Persistent);

        // spawn in boids
        for (int i = 0; i < config.boidsCount; i++)
        {
            GameObject obj = Instantiate(
                boidPrefab,
                UnityEngine.Random.insideUnitSphere * (config.spawnRadius / 2f),
                Quaternion.identity
            );
            obj.gameObject.SetActive(true);
            boids.Add(obj.GetComponent<Boid>());

            // setup nativearray and transformaccessarray
            transformAccessArray.Add(boids[i].transform);
            boidsData[i] = boids[i].GetComponent<Boid>().data;
            wanderVectors[i] = new Vector3(
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f)
            ).normalized; // normalize the vector for consistent wandering behavior
        }
    }

    private void Update()
    {
        for (int i = 0; i < wanderVectors.Length; i++)
        {
            wanderVectors[i] = new Vector3(
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f)
            ).normalized;
        }

        BoidJob job = new BoidJob
        {
            // raw data parameters to pass into the job
            boids = boidsData,
            wanderVectors = wanderVectors,

            deltaTime = Time.deltaTime,
            boundSize = config.boundSize,

            maxVelocity = config.boidMaxVelocity,
            maxAcceleration = config.boidMaxAcceleration,
            wanderWeight = config.wanderWeight,
            wanderRadius = config.wanderRadius
        };

        JobHandle jobHandle = job.Schedule(transformAccessArray);
        jobHandle.Complete();

        for (int i = 0; i < boids.Count; i++)
        {
            boids[i].GetComponent<Boid>().data = boidsData[i];
            boids[i].transform.position = boidsData[i].position;
        }
        shouldDraw = true;
    }

    private void OnRenderObject()
    {
        // ensure draw calls are synced with update
        if (!shouldDraw) return;

        foreach(Boid boid in boids)
        {
            GL.PushMatrix();
            GL.Begin(GL.LINES);
            lineMaterial.SetPass(0);

            GL.Color(Color.red);
            GL.Vertex(boid.transform.position);
            GL.Vertex(boid.transform.position + boid.data.velocity);

            GL.End();
            GL.PopMatrix();
        }

        shouldDraw = false;
    }

    private void OnDestroy()
    {
        transformAccessArray.Dispose();
        boidsData.Dispose();
        wanderVectors.Dispose();
    }
}