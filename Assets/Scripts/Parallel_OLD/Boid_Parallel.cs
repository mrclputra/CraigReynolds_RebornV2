using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Boid_Parallel : MonoBehaviour
{
    // this list is updated every frame used for vector calculations, to be implemented
    // public List<Boid> neighbors = new List<Boid>();

    public struct Data
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 acceleration;

        public Vector3 wanderTarget;    // this is consistent, changes a bit every frame

        // constructor
        public Data(Vector3 position, Vector3 velocity, Vector3 acceleration, Vector3 wanderTarget)
        {
            this.position = position;
            this.velocity = velocity;
            this.acceleration = acceleration;
            this.wanderTarget = wanderTarget;
        }
    }

    public Data data;
}
