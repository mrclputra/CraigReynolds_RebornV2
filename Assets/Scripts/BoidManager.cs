using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Burst;

[BurstCompile]
public class BoidManager : MonoBehaviour
{
    [SerializeField] private Config config;
    [SerializeField] private GameObject boidPrefab;
    private List<Boid> boids = new List<Boid>();

    private void Awake()
    {
        // spawn in boids
        for (int i = 0; i < config.boidsCount; i++)
        {
            Vector3 spawnPosition = UnityEngine.Random.insideUnitSphere * (config.spawnRadius / 2);
            GameObject obj = Instantiate(boidPrefab, spawnPosition, Quaternion.identity);
            obj.gameObject.SetActive(true);
            boids.Add(obj.GetComponent<Boid>());
        }
    }

    private void Update()
    {
        for(int i = 0; i < boids.Count; i++)
        {
            // implement individual boid behaviour here
            // directly manipulate the vectors
        }
    }
}


[BurstCompile]
public struct BoidJob : IJobParallelForTransform
{
    public void Execute(int index, TransformAccess transform)
    {
        throw new System.NotImplementedException();
    }
}
