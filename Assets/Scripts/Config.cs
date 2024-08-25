using UnityEngine;

[CreateAssetMenu(fileName = "Config", menuName = "Config", order = 0)]
public class Config : ScriptableObject
{
    [Header("Environment Variables")]
    public float boundSize;
    public float spawnRadius;

    [Header("Boids")]
    public int boidsCount;
    public int maxNeighbors;

    public float boidViewFOV;
    public float boidViewDistance;
    public float boidMaxAcceleration;
    public float boidMaxVelocity;

    public float wanderRadius;

    public float wanderWeight;
    public float cohesionWeight;
    public float alignmentWeight;
    public float separationWeight;

    // user interface
    public bool wanderEnabled = true;
    public bool cohesionEnabled = true;
    public bool alignmentEnabled = true;
    public bool separationEnabled = true;

    public void Awake()
    {
        // uncap framerate
        Application.targetFrameRate = int.MaxValue;
    }
}
