using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using UnityEngine;
using System.Threading;
using UnityEngine.UIElements;

public class BoidManager : MonoBehaviour
{
    [SerializeField] private Config config;
    [SerializeField] private GameObject boidPrefab;
    [SerializeField] private Material lineMaterial;

    private List<Boid> boids = new List<Boid>();
    private bool shouldDraw = false; // line draw flag

    private static readonly System.Random random = new System.Random();
    private static readonly object randomLock = new object();

    private void Awake()
    {
        // spawn in boids
        for (int i = 0; i < config.boidsCount; i++)
        {
            GameObject obj = Instantiate(
                boidPrefab,
                UnityEngine.Random.insideUnitSphere * (config.spawnRadius / 2f),
                Quaternion.identity
            );
            obj.gameObject.SetActive( true );
            boids.Add(obj.GetComponent<Boid>());
        }
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        // manage each boid script
        Parallel.ForEach(boids, boid =>
        {
            //UnityEngine.Debug.Log($"Boid processing on thread {Thread.CurrentThread.ManagedThreadId}");
            
            // update neighbor list of boid
            boid.neighbors = GetNeighbors(boid, config.boidViewDistance);

            // compute velocity and acceleration, and update
            // no transformations here
            Vector3 velocity = boid.velocity + boid.acceleration * deltaTime;
            boid.velocity = Vector3.ClampMagnitude(velocity, config.boidMaxVelocity);

            Vector3 acceleration = Combine(boid);
            boid.acceleration = Vector3.ClampMagnitude(acceleration, config.boidMaxAcceleration);
        });

        shouldDraw = true;
    }

    private List<Boid> GetNeighbors(Boid boid, float radius)
    {
        List<Boid> neighbors = new List<Boid>();

        // return list of boids in a radius
        // use the boids list to iterate
        foreach(Boid otherBoid in boids)
        {
            float distance = Vector3.Distance(boid.position, otherBoid.position);
            if(distance < radius)
            {
                neighbors.Add(otherBoid);
            }
        }

        // remove self
        neighbors.Remove(boid);

        return neighbors;
    }

    // boid vector stuff
    private Vector3 Combine(Boid boid)
    {
        Vector3 result = Vector3.zero;

        if (config.wanderEnabled)
            result += Wander(boid) * config.wanderWeight;
        if (config.cohesionEnabled)
            result += Cohesion(boid) * config.cohesionWeight;
        if (config.alignmentEnabled)
            result += Alignment(boid) * config.alignmentWeight;
        if (config.separationEnabled)
            result += Separation(boid) * config.separationWeight;
        result += AvoidBounds(boid);

        //result +=
        //    AvoidBounds(boid) + // temporary disable
        //    Wander(boid) * config.wanderWeight;

        return result.normalized;
    }

    private Vector3 Wander(Boid boid)
    {
        float x, y, z;
        lock (randomLock)
        {
            x = (float)(random.NextDouble() * 2.0 - 1.0);
            y = (float)(random.NextDouble() * 2.0 - 1.0);
            z = (float)(random.NextDouble() * 2.0 - 1.0);
        }

        return Vector3.ClampMagnitude(boid.wanderTarget += new Vector3(x, y, z), config.wanderRadius).normalized;
    }

    private Vector3 AvoidBounds(Boid boid)
    {
        Vector3 avoidanceVector = Vector3.zero;
        float edgeDistance = config.boundSize * 0.3f;

        avoidanceVector.x = boid.position.x < -edgeDistance ? 1 : boid.position.x > edgeDistance ? -1 : 0;
        avoidanceVector.y = boid.position.y < -edgeDistance ? 1 : boid.position.y > edgeDistance ? -1 : 0;
        avoidanceVector.z = boid.position.z < -edgeDistance ? 1 : boid.position.z > edgeDistance ? -1 : 0;

        return avoidanceVector.normalized * 100f; // adjust multiplier
    }

    private Vector3 Cohesion(Boid boid)
    {
        Vector3 cohesionVector = Vector3.zero;

        if (boid.neighbors == null || boid.neighbors.Count == 0)
            return cohesionVector;

        foreach(Boid neighbor in boid.neighbors)
        {
            // sum positions
            cohesionVector += boid.position;
        }

        cohesionVector /= boid.neighbors.Count; // calculate average position of neighbors
        cohesionVector -= boid.position;        // get direction towards said position
        return cohesionVector.normalized;       // normalize and return
    }

    private Vector3 Alignment(Boid boid)
    {
        Vector3 alignmentVector = Vector3.zero;

        if (boid.neighbors == null || boid.neighbors.Count == 0)
            return alignmentVector;

        foreach(Boid neighbor in boid.neighbors)
        {
            // sum velocities of neighbors
            alignmentVector += neighbor.velocity;
        }

        alignmentVector /= boid.neighbors.Count;
        return alignmentVector.normalized;
    }

    private Vector3 Separation(Boid boid)
    {
        Vector3 separationVector = Vector3.zero;

        if(boid.neighbors == null || boid.neighbors.Count == 0)
            return separationVector;

        foreach(Boid neighbor in boid.neighbors)
        {
            Vector3 target = boid.position - neighbor.position;
            if (target.magnitude > 0)
                separationVector += target.normalized / target.sqrMagnitude;
        }

        return separationVector.normalized;
    }

    private void OnRenderObject()
    {
        // ensure all positions have been processed first
        if (!shouldDraw)
            return;

        foreach (Boid boid in boids)
        {
            GL.PushMatrix();
            GL.Begin(GL.LINES);
            lineMaterial.SetPass(0);

            GL.Color(Color.red);
            GL.Vertex(boid.transform.position);
            GL.Vertex(boid.transform.position + (boid.velocity / 2));

            GL.End();
            GL.PopMatrix();
        }

        shouldDraw = false;
    }
}
