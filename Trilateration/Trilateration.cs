using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



// Trilateration
// responsible for  
public class Trilateration
{
    public List<Robot> robots;
    protected int sensorRange; // the sensorRange for each robot


    // Trilateration(numRobots, alpha, sensorRange, guessRange) creates a new Trilateration object, with the given parameters
    // notes:   
    //  numRobots: the number of robots in the simulation
    //  alpha: the learning rate for gradient descent
    //  sensorRange: the sensor range of each robot
    //  guessRange: the size of the map
    public Trilateration(int numRobots, float alpha = 0.05f, int sensorRange = 10, int guessRange = 15)
    {
        this.sensorRange = sensorRange;
        robots = new List<Robot>();
        for(int i = 0; i < numRobots; i++)
        {
            robots.Add(new Robot(guessRange, alpha, i));
        }
    }

    
    // initRobots() a method to be overridden to initialize the robots appropriately
    public virtual void initRobots()
    {

    }

    
    // collectMeasurments() updates each robots measurements list
    protected void collectMeasurments()
    {
        // clear the previous measurments
        for(int i = 0; i < robots.Count; i++)
        {
            robots[i].measurments.Clear();
        }

        for (int i = 0; i < robots.Count; i++)
        {       
            for (int j = i + 1; j < robots.Count; j++)
            {
                // only robots within range are added to each-other's measurements
                float dist = Vector3.Distance(robots[i].position, robots[j].position);
                float globalDist = Vector3.Distance(robots[i].globalPosition, robots[j].globalPosition);
                if (globalDist < sensorRange)
                {
                    robots[i].addMeasurment(robots[j].position, dist, globalDist, robots[j]);
                    robots[j].addMeasurment(robots[i].position, dist, globalDist, robots[i]);
                }
            }
        }
    }


    // updateGuess() applies gradient descent on valid robots to update their positions
    public void updateGuess()
    {
        for(int i = 0; i < robots.Count; i++)
        {
            if (!robots[i].done || robots[i].currentCoords.originId == -1)
            {
                continue;
            }
            Vector3 gd = robots[i].gradientDescent();
            robots[i].position = gd;
        }
    }


    // printLoss() prints the average distance error per robot
    public string printLoss()
    {
        Debug.LogError("start loss: ");
        int notFound = 0;
        float totalLoss = 0;
        for(int i = 0; i < robots.Count; i++)
        {
            if (!robots[i].done)
            {
                notFound++;
                continue;
            }

            for(int j = i+1; j < robots.Count; j++)
            {
                if (!robots[j].done)
                {               
                    continue;
                }

                float estimatedDistance = Vector3.Distance(robots[i].position, robots[j].position);
                float actualDistance = Vector3.Distance(robots[i].globalPosition, robots[j].globalPosition);
                totalLoss += Mathf.Abs(estimatedDistance - actualDistance);
            }
        }
        float avgLoss = totalLoss / (robots.Count - notFound);
        return avgLoss.ToString();
    }

    // update()
    public virtual void update()
    {
        collectMeasurments();
        updateGuess();
        printLoss();
    }
}
