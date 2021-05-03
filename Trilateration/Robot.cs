using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Robot
// responsible for each individual agent in the trilateration simulation
//  robots only have access to local information, and a single sensor that gives the
//  range measurments to nearby robots, as well as limited communication between robots
public class Robot
{

    public Vector3 position; // the robots estimated position
    public Vector3 globalPosition; // the robots actual position
    public List<Measurement> measurments; // list of nearby robots
    private float alpha; // the learning rate for gradient descent
    public bool done; // if the robot has finalized their coordinate system
    public int id;

    public Coordinates currentCoords; // the current coordinate system of the robot

    public int toInvadeId = -1; // the coordinate system id to invade
    public int invadedById = -1; // the coordinate system id of the invading system
    public int toInvadeCount = 0; // amount of time since no system was invaded
    public int invadedByCount = 0; // amount of time since uninvaded
    public Robot invadingRobot = null; // the robot that is invading

    // Robot(guessRange, alpha, id) creates a new robot object with the given parameters
    public Robot(int guessRange, float alpha, int id = 0)
    {
        position = VectorUtil.generateRandomVector(guessRange);
        globalPosition = VectorUtil.generateRandomVector(guessRange);
        currentCoords = new Coordinates(position, -1);
        measurments = new List<Measurement>();
        this.alpha = alpha;
        done = false;
        this.id = id;
        done = true;
        toInvadeId = -1;
        invadedById = -1;
        toInvadeCount = 0;
        invadedByCount = 0;
    }


    // getInvadedBy(robot) if the robot is not currently invading the robot, that it gets invaded by the given robot
    public void getInvadedBy(Robot robot)
    {
        if(id == toInvadeId)
        {
            return;
        }

        invadingRobot = robot;
        invadedByCount = 0;
        invadedById = robot.currentCoords.originId;
    }

    // resetInvadedBy() reset all invaded information, the robot is no longer being invaded by anyone
    public void resetInvadedBy()
    {
        invadedById = -1;
        invadedByCount = 0;
        invadingRobot = null;
    }


    // resetInvading() the robot is no longer invading anyone
    public void resetInvading()
    {
        toInvadeCount = 0;
        toInvadeId = -1;
    }


    // gradientDescent() returns a new position after being updated with gradient descent
    public Vector3 gradientDescent()
    {
        Vector3 errors = Vector3.zero;
        for (int i = measurments.Count - 1; i >= 0; i--)
        {
            if(measurments[i].dist == 0)
            {
                continue;
            }

            Vector3 curr_error = Vector3.zero;
            for (int j = 0; j < 3; j++)
            {

                curr_error[j] = -2.0f * (position[j] - measurments[i].pos_guess[j]) * (measurments[i].globalDist - measurments[i].dist) / measurments[i].dist;

            }
            errors += curr_error;
        }
        return position - alpha * errors;
    }

    // addMeasurment(pos, dist, globalDist, other) adds a new measurment to the robots stack, based on the given information
    public void addMeasurment(Vector3 pos, float dist, float globalDist, Robot other)
    {
        measurments.Add(new Measurement(pos, dist, globalDist, other));
    }

}
