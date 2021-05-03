using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// GlobalTrilateration
// this class contains the implementation of trilateration using three global beacon nodes

public class GlobalTrilateration : Trilateration
{

    // Emitter
    // this class represents a beacon node, which has a known position, and a powerful signal to reach all robots
    class Emitter
    {
        public Vector3 position;

        public Emitter(Vector3 position)
        {
            this.position = position;
        }
    }

    private int numEmitters;
    private List<Emitter> emitters;

    // GlobalTrilateration(numRobots, alpha, sensorRange, guessRange) creates a new globalTrilateration object with three emitters
    // from the given parameters
    public GlobalTrilateration(int numRobots, float alpha = 0.05f, int sensorRange = 10, int guessRange = 15) : base(numRobots, alpha, sensorRange, guessRange)
    {
        numEmitters = 3;
        emitters = new List<Emitter>();
        for (int i = 0; i < numEmitters; i++)
        {
            emitters.Add(new Emitter(VectorUtil.generateRandomVector(guessRange)));
            VectorUtil.printVector(emitters[i].position);
        }
    }

    // initRobots() determines the position of each robot using the emitters
    public override void initRobots()
    {
        for (int i = 0; i < robots.Count; i++)
        {
            Vector3 distances = Vector3.zero; // the distances between the robot and emitters
            for (int j = 0; j < numEmitters; j++)
            {
                distances[j] = Vector3.Distance(emitters[j].position, robots[i].globalPosition);
            }

            Vector3 robotLocation;
            if (!CircleTrilateration.getUserLocation(emitters[0].position, distances[0], emitters[1].position, distances[1], emitters[2].position, distances[2], out robotLocation))
            {
                // the robots location could not be determined from the emitters
                return;
            }
            robots[i].position = robotLocation;
        }
    }

    public override void update()
    {
        base.update();
    }
}
