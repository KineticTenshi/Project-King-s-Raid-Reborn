using UnityEngine;
using System;
using UnityEngine.UIElements;

public class Teleportation : MonoBehaviour
{
    public float position_x;
    public float position_z;

    public float teleportationInterval = 5f;
    public float teleportationTime;


    public System.Random rand = new System.Random();

    public void Update()
    {
        teleportationTime += Time.deltaTime;
        if (teleportationTime > teleportationInterval)
        {
            position_x = rand.Next(-10, 10);
            position_z = rand.Next(-10, 10);
            transform.position = new Vector3(position_x, 0.5f, position_z);
            teleportationTime = 0f;
        }
    }
}
