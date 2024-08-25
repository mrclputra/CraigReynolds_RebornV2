using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

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
        // calculate every boid position in boids
        //foreach (Boid boid in boids)
        //{
        //    // calculate movement
        //    boid.acceleration = Vector3.ClampMagnitude(Combine(boid), config.boidMaxAcceleration);
        //    boid.velocity = Vector3.ClampMagnitude(boid.velocity + boid.acceleration * Time.deltaTime, config.boidMaxVelocity);
        //}

        float deltaTime = Time.deltaTime;

        Parallel.ForEach(boids, boid =>
        {
            boid.acceleration = Vector3.ClampMagnitude(Combine(boid), config.boidMaxAcceleration);
            boid.velocity = Vector3.ClampMagnitude(boid.velocity + boid.acceleration * deltaTime, config.boidMaxVelocity);
        });

        shouldDraw = true;
    }

    // boid vector stuff
    private Vector3 Combine(Boid boid)
    {
        Vector3 result = Vector3.zero;

        result +=
            AvoidBounds(boid) + // temporary disable
            Wander(boid) * config.wanderWeight;

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
