using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    public Vector3 position;
    public Vector3 velocity;
    public Vector3 acceleration;

    [SerializeField] WorldManager world;
    [SerializeField] Config config;

    [SerializeField] Material lineMaterial;
    [SerializeField] Material trailMaterial;

    private Vector3 wanderTarget = Vector3.zero;

    private bool shouldDraw = false;

    private void Awake()
    {
        // setup components here
        GetComponent<TrailRenderer>().material = trailMaterial;
    }

    private void Start()
    {
        // init position (important)
        position = transform.position;
    }

    private void Update()
    {
        acceleration = Combine();
        acceleration = Vector3.ClampMagnitude(acceleration, config.boidMaxAcceleration);

        // apply acceleration
        velocity = Vector3.ClampMagnitude(velocity + acceleration * Time.deltaTime, config.boidMaxVelocity);

        // update position
        position += velocity * Time.deltaTime;
        transform.position = position;

        shouldDraw = true;
    }

    private Vector3 Combine()
    {
        Vector3 result = Wander() * config.wanderWeight + avoidBounds();
        return result.normalized;
    }

    private Vector3 Wander()
    {
        wanderTarget += new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;

        wanderTarget = Vector3.ClampMagnitude(wanderTarget, config.wanderRadius);

        return wanderTarget.normalized;
    }

    private Vector3 avoidBounds()
    {
        Vector3 avoidanceVector = Vector3.zero;
        float edgeDistance = config.boundSize * 0.3f;

        avoidanceVector.x = position.x < -edgeDistance ? 1 : position.x > edgeDistance ? -1 : 0;
        avoidanceVector.y = position.y < -edgeDistance ? 1 : position.y > edgeDistance ? -1 : 0;
        avoidanceVector.z = position.z < -edgeDistance ? 1 : position.z > edgeDistance ? -1 : 0;

        return avoidanceVector * 100f; // yes
    }

    private void OnRenderObject()
    {
        // ensure draw calls synced with update()
        if (shouldDraw)
        {
            GL.PushMatrix();
            GL.Begin(GL.LINES);
            lineMaterial.SetPass(0);

            GL.Color(Color.red); // change line color here
            GL.Vertex(position);
            GL.Vertex(position + velocity);

            GL.End();
            GL.PopMatrix();

            shouldDraw = false; // reset flag
        }
    }
}
