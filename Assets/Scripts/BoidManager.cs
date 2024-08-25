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
    [SerializeField] private Material lineMaterial;

    private List<GameObject> boids = new List<GameObject>();
    //private List<Boid> boids = new List<Boid>();

    private bool shouldDraw = false;

    private void Awake()
    {
        // spawn in boids
        for (int i = 0; i < config.boidsCount; i++)
        {
            Vector3 spawnPosition = UnityEngine.Random.insideUnitSphere * (config.spawnRadius / 1.5f);
            GameObject obj = Instantiate(boidPrefab, spawnPosition, Quaternion.identity);
            obj.gameObject.SetActive(true);
            boids.Add(obj);

            Boid boid = obj.GetComponent<Boid>();
            boid.position = spawnPosition;
            boid.wanderTarget = Vector3.zero;
        }
    }

    private void Update()
    {
        foreach(GameObject boidObj in boids)
        {
            Boid boid = boidObj.GetComponent<Boid>();

            // update neighbors list
            boid.neighbors = GetNeighbors(boidObj);

            // calculate acceleration
            Vector3 acceleration = Vector3.ClampMagnitude(Combine(boid), config.boidMaxAcceleration);
            Vector3 velocity = Vector3.ClampMagnitude(boid.velocity + acceleration * Time.deltaTime, config.boidMaxVelocity);
            Vector3 position = boid.position + velocity * Time.deltaTime;

            // update boid component and transform
            boid.velocity = velocity;
            boid.acceleration = acceleration;
            boid.position = position;
            boidObj.transform.position = position;

            shouldDraw = true;
        }
    }

    private List<Boid> GetNeighbors(GameObject targetBoidObj)
    {
        Boid targetBoid = targetBoidObj.GetComponent<Boid>();
        List<Boid> neighbors = new List<Boid>();
        float radiusSquared = Mathf.Pow(config.boidViewDistance, 2); // use squared radius for distance comparison

        foreach (GameObject boidObj in boids)
        {
            if (boidObj != targetBoidObj) // Ignore the boid itself
            {
                Boid boid = boidObj.GetComponent<Boid>();
                float distanceSquared = (boid.position - targetBoid.position).sqrMagnitude;

                if (distanceSquared <= radiusSquared)
                {
                    neighbors.Add(boid);
                }
            }
        }

        return neighbors;
    }

    private Vector3 Combine(Boid boid)
    {
        return (Wander(boid) * config.wanderWeight + avoidBounds(boid)).normalized;
    }

    private Vector3 Wander(Boid boid)
    {
        boid.wanderTarget += new Vector3(
            UnityEngine.Random.Range(-1f, 1f),
            UnityEngine.Random.Range(-1f, 1f),
            UnityEngine.Random.Range(-1f, 1f)
        ).normalized;

        return (Vector3.ClampMagnitude(boid.wanderTarget, config.wanderRadius)).normalized;
    }

    private Vector3 avoidBounds(Boid boid)
    {
        Vector3 avoidanceVector = Vector3.zero;
        float edgeDistance = config.boundSize * 0.3f;

        avoidanceVector.x = boid.position.x < -edgeDistance ? 1 : boid.position.x > edgeDistance ? -1 : 0;
        avoidanceVector.y = boid.position.y < -edgeDistance ? 1 : boid.position.y > edgeDistance ? -1 : 0;
        avoidanceVector.z = boid.position.z < -edgeDistance ? 1 : boid.position.z > edgeDistance ? -1 : 0;

        return avoidanceVector * 100f; // yes
    }

    private void OnRenderObject()
    {
        // ensure draw calls are synced with update
        if (!shouldDraw) return;

        foreach(GameObject boidObj in boids)
        {
            GL.PushMatrix();
            GL.Begin(GL.LINES);
            lineMaterial.SetPass(0);

            GL.Color(Color.red);
            GL.Vertex(boidObj.transform.position);
            GL.Vertex(boidObj.transform.position + boidObj.GetComponent<Boid>().velocity);

            GL.End();
            GL.PopMatrix();
        }

        shouldDraw = false;
    }
}


[BurstCompile]
public struct BoidJob : IJobParallelForTransform
{
    public void Execute(int index, TransformAccess transform)
    {
        // boid vector calculations and job stuff here
    }
}
