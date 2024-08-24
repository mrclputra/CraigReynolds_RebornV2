using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    [SerializeField] private Config config;
    [SerializeField] private GameObject boidPrefab;
    private List<GameObject> boids = new List<GameObject>();

    private void Awake()
    {
        // spawn in boids
        for (int i = 0; i < config.boidsCount; i++)
        {
            Vector3 spawnPosition = Random.insideUnitSphere * (config.spawnRadius / 2);
            GameObject obj = Instantiate(boidPrefab, spawnPosition, Quaternion.identity);
            obj.gameObject.SetActive(true);
            boids.Add(obj);
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
