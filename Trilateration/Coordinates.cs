using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Coordinates
// represents the coordinate system of a robot
public class Coordinates
{
    public Vector3 position; // the robots position in the current coordinate system
    public int originId; // the id of the robot that represents the origin of the coordinate system

    // Coordinates(position, originId) creates a new coordinates object from the given parameters
    public Coordinates(Vector3 position, int originId)
    {
        this.position = position;
        this.originId = originId;
    }

}
