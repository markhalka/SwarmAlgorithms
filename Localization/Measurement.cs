using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Measurement
// this class contains a robots measurment of another robot
public class Measurement
{
    public Vector3 pos_guess; // 
    public float dist; // the estimated distance between the robot and measured robot
    public float globalDist; // the actual distance between the robot and measured robot
    public Robot robot; // the measured robot


    // Measurement(pos_guess, dist, globalDist) creates a new measurement class from the given parameters
    public Measurement(Vector3 pos_guess, float dist, float globalDist)
    {
        this.pos_guess = pos_guess;
        this.dist = dist;
        this.globalDist = globalDist;
    }

    // Measurement(pos_guess, dist, globalDist) creates a new measurement class from the given parameters
    public Measurement(Vector3 pos_guess, float dist, float globalDist, Robot robot) : this(pos_guess, dist, globalDist)
    {
        this.robot = robot;
    }
}

