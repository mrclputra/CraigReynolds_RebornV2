using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    public Vector3 velocity;
    public Vector3 acceleration;
    public Vector3 position;

    public Vector3 wanderTarget; // used in Wander()

    public List<Boid> neighbors;

    private void Start()
    {
        // runs on object instantiate
        velocity = Vector3.zero;
        acceleration = Vector3.zero;
        position = transform.position;

        wanderTarget = Vector3.zero;
    }

    private void FixedUpdate()
    {
        position += velocity * Time.deltaTime;
        transform.position = position;

        // update rotation
        if(velocity != Vector3.zero )
        {
            Quaternion targetRotation = Quaternion.LookRotation(velocity);
            transform.rotation = targetRotation;
        }
    }
}