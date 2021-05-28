using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class LocalTrilateration : Trilateration
{

  
    // LocalTrilateration:
    //      this class allows a swarm of Robot objects to converge to a global coordinate system

    // two more things you need to add:
    // 1. estimate centroid, and use distance from that
    // 2. make sure that after invasions, regions are still relativly well-formed 

    public string file; // the file where simulation results are written to
    List<string> toWrite = new List<string>(); // what is written to the file

    int guessRange; // the size of the map
    int numRobots; // the number of robots in the simulation

    bool part1 = true;
    int doneCount = 0;

    // LocalTrilateration(numRobots, alpha, sensorRange, guessRange) creates a new instance of the LocalTrilateration class
    public LocalTrilateration(int numRobots, float alpha = 0.05f, int sensorRange = 10, int guessRange = 15, string file="default") : base(numRobots, alpha, sensorRange, guessRange)
    {
        this.sensorRange = sensorRange;
        this.guessRange = guessRange;
        this.numRobots = numRobots;
        this.file = file;
    }

    #region tests
    // smallerAreaTest() used to test the properties of the divide and conquer algorithm with only two areas
    public void smallerAreaTest()
    {
        int left = 1;
        int right = 2;
        for (int i = 0; i < robots.Count; i++)
        {
            robots[i].currentCoords.position = robots[i].globalPosition;
            robots[i].position = robots[i].globalPosition;
            if (robots[i].globalPosition.x <= 170)//Random.Range(0, 2) == 1)
            {
                robots[i].currentCoords.originId = left;
            } else
            {
                robots[i].currentCoords.originId = right;
                robots[i].currentCoords.position.x = guessRange - robots[i].currentCoords.position.x;
            }
        }

        collectMeasurments();
    }

    #endregion

    // findValidNeighbors(robot, isSeed, neighborhood) returns true if the given robot has a valid neighborhood 
    //      and sets neighborhoods to include all valid neighborhoods, otherwise it returns false and neighborhoods is
    //      undefeinded
    // requires: robot to be valid
    private bool findValidNeighbors(Robot robot, bool isSeed, out List<Neighborhood> neighborhoods)
    {
        List<Robot> robotOrigins = new List<Robot>();
        neighborhoods = new List<Neighborhood>();
        if (robot.measurments.Count < 2)
        {
            // if a robot has less than two measurements, it cannot have a valid neighborhood
            return false;
        }

        for (int i = 0; i < robot.measurments.Count; i++)
        {
            Robot other = robot.measurments[i].robot;
            if (isSeed && other.currentCoords.originId != -1)
            {
                // if the robot is a seed robot, it must find other robots with no coordinate system
                continue;
            }
            else if (!isSeed && (other.currentCoords.originId == -1 || other.currentCoords.originId == robot.currentCoords.originId))
            {
                continue;
            }
            else
            {
                robotOrigins.Add(other);
            }
        }

        // sort the valid robots by their coordinate system
        ILookup<int, Robot> robotLookup = robotOrigins.ToLookup(r => r.currentCoords.originId);
        bool hasValidNeighbours = false;

        foreach (var r in robotLookup)
        {
            List<Robot> curr = r.ToList<Robot>();

            if (curr.Count >= 3)
            {
                // a coordinate system is only valid if it has more than 2 robots (otherwise trilateration cannot be used)
                hasValidNeighbours = true;
                neighborhoods.Add(new Neighborhood(curr));
            }
        }
        return hasValidNeighbours;
    }

    // initialGuess(robots) creates 3 seed agents from the given list of robots.
    // requires: robots must be valid, with at least 3 members
    public bool initialGuess(List<Robot> robots)
    {

        float r0Dist = Vector3.Distance(robots[0].globalPosition, robots[2].globalPosition);
        float r1Dist = Vector3.Distance(robots[1].globalPosition, robots[2].globalPosition);

        Vector3 r2Guess1;
        Vector3 r2Guess2;
        if (CircleTrilateration.intersectionOf2Circles(Vector2.zero, r0Dist,
            new Vector2(Vector2.Distance(robots[0].globalPosition, robots[1].globalPosition), 0), r1Dist, out r2Guess1, out r2Guess2) == 0)
        {
            // trilateration failed
            return false;
        }

        robots[0].currentCoords.position = Vector3.zero;
        robots[0].currentCoords.originId = robots[0].id;

        robots[1].currentCoords.position = new Vector3(Vector3.Distance(robots[0].globalPosition, robots[1].globalPosition), 0, 0);
        robots[1].currentCoords.originId = robots[0].id;


        robots[2].currentCoords.position = r2Guess1; // we randomly take one guess, as either one will be valid
        robots[2].currentCoords.originId = robots[0].id;

        return true;
    }


    // findNewFromTrilat(robot, neighbors) finds new coordinates from the robots given neighbors using trilateration
    // requires: robot and neighbors to be valid
    private bool findNewFromTrilat(Robot robot, List<Robot> neighbors)
    {
        // find three random distinct robots from the given neighborhood
        int first = Random.Range(0, neighbors.Count);
        int second = first;
        while (second == first)
        {
            second = Random.Range(0, neighbors.Count);
        }

        int third = first;
        while (third == first || third == second)
        {
            third = Random.Range(0, neighbors.Count);
        }

        Vector3 newPos;
        float dist0 = Vector3.Distance(neighbors[first].globalPosition, robot.globalPosition);
        float dist1 = Vector3.Distance(neighbors[second].globalPosition, robot.globalPosition);
        float dist2 = Vector3.Distance(neighbors[third].globalPosition, robot.globalPosition);

        if (!CircleTrilateration.getUserLocation(neighbors[first].currentCoords.position, dist0, neighbors[second].currentCoords.position,
            dist1, neighbors[third].currentCoords.position, dist2, out newPos))
        {
            // trilateration failed
            return false;
        }

        Coordinates newCoords = new Coordinates(newPos, neighbors[first].currentCoords.originId);
        robot.currentCoords = newCoords;

        return true;
    }


    // checkNeighbor(robot) attempts to create or update the coordinate system of the given robot.
    //      if the robot has no coordinate system, it may initialize it as a seed robot. Otherwise, 
    //      it will attempt to use its neighbors to initialize itself in a coordinate system
    // requires: robot to be valid
    private void checkNeighbor(Robot robot)
    {

        List<Neighborhood> neighborhoods = new List<Neighborhood>();
        if (robot.currentCoords.originId == -1)
        {
            int seedChance = Random.Range(0, 400);

            if (seedChance == 1)
            {
                if (findValidNeighbors(robot, true, out neighborhoods))
                {
                    if (initialGuess(neighborhoods[0].neighbours))
                    {
                        // the robot has become a seed
                        return;
                    }
                }
            }
        }

        neighborhoods.Clear();

        if (!findValidNeighbors(robot, false, out neighborhoods))
        {
            // if the robot has no valid neighbors, there is nothing to do
            return;
        }

        int minOriginId = -1;
        List<Robot> neighbors = new List<Robot>();

        // find the neighborhood with the most robots
        foreach (var n in neighborhoods)
        {
            if (n.neighbours.Count > minOriginId)
            {
                minOriginId = n.neighbours.Count;
                neighbors = n.neighbours;
            }
        }

        if (getCloseNeighborhood(robot) < 0.25f)
        {
            // if the local density of the robots current coordinate system is less than 25%, update to the most popular coordinate system
            findNewFromTrilat(robot, neighbors);
        }
    }

    // you should move this to the robot class

    // checkInvadeby(Robot robot) returns false if an invasion was unsuccessful. Otherwise, if there is an invasion it will
    //      propogate the invasion method to its neighbors, and it will attempt to convert to the invading coordinate system
    // requires: robot to be valid
    private bool checkInvadedBy(Robot robot)
    {

        for (int i = 0; i < robot.measurments.Count; i++)
        {
            if (robot.measurments[i].robot.currentCoords.originId == robot.currentCoords.originId)
            {                
                if (robot.invadedById != -1 && robot.measurments[i].robot.invadedById == -1)
                {
                    if(Random.Range(0,2) == 1)
                    {
                        // the robot has a probability of propogating the invade signal within its area
                         invadeThing(robot.invadingRobot, robot.measurments[i].robot); 
                    }           
                }
            }
        }
        
        // if the robot is invading, then after a certain amount of time forget the invasion
        if (robot.toInvadeId != -1)
        {
            robot.toInvadeCount++;
            if (robot.toInvadeCount > 10)
            {
                Debug.LogError("RESETING");
                robot.resetInvading();
                return false;
            }
        }

        // if the robot is being invaded, after a certain amount of time forget the invasion
        if(robot.invadedById != -1)
        {
            robot.invadedByCount++;
            if (robot.invadedByCount > 10)
            {
                robot.resetInvadedBy();
                return false;
            }
        }

        if (robot.invadedById != -1)
        {
            // the robot will pass the invade message to its neighbors in the same area
            // the robot will attempt to convert to the invading coordinate system
            List<Neighborhood> neighborhoods;
            if (findValidNeighbors(robot, false, out neighborhoods))
            {
                foreach(var n in neighborhoods)
                {
                    if (n.neighbours[0].currentCoords.originId == robot.invadedById)
                    {
                        if (findNewFromTrilat(robot, n.neighbours))
                        {
                            // the robot was successfully invaded
                            robot.resetInvadedBy();
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
        }
        return false;
    }

    int invadeTick = 0;

    // TODO: CHANGE THIS METHOD SO THAT YOU CAN INVADE ROBOTS THAT ARE NOT APART OF A VALID NEIGHBORHOOD (LESS THAN 3 NEARBY)

    // checkToInvade(robot) checks to see if the robot can invade another area
    private void checkToInvade(Robot robot)
    {
        
        if(robot.invadedById != -1)
        {
            // a robot can only invade if it is not already being invaded
            return;
        }

        if (robot.currentCoords.originId == -1)
        {
            // a robot can only invade if it belongs to a coordinate system
            return;
        }

        List<Neighborhood> neighborhoods;
        if (!findValidNeighbors(robot, false, out neighborhoods))
        {
            // if a robot has no valid neighbors, there is nothing to do
            return;
        }

        // find the neighborhood with the least amount of robots
        List<Robot> neighbors = new List<Robot>();
        neighborhoods.Sort(delegate (Neighborhood a, Neighborhood b)
        {
            if (a.neighbours.Count < b.neighbours.Count)
            {
                return -1;
            } else if(a.neighbours.Count == b.neighbours.Count)
            {
                return 0;
            } else
            {
                return 1;
            }
                });

        neighbors = neighborhoods[0].neighbours;

        // sort the neighbors by distance from their origin
        neighbors.Sort(delegate (Robot a, Robot b)
        {
            float distA = Vector2.SqrMagnitude(a.currentCoords.position);
            float distB = Vector2.SqrMagnitude(b.currentCoords.position);
            if (distA > distB)
            {
                return 1;
            }
            else if (distA == distB)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        });

        // probabilistically invade the neighbors who are on the edge of their area
        Robot enemy = null;
        for(int i =0; i < neighbors.Count; i++)
        {
            if(Random.Range(0,4) == 1)
            {
                enemy = neighbors[i];
            }
        }

        invadeThing(robot, enemy);
    }

    // getCloseNeighborhood(robot) returns the percentage of robots near the given robot who have the same coordinate system
    float getCloseNeighborhood(Robot robot)
    {
        int count = 0;
        for (int i = 0; i < robot.measurments.Count; i++)
        {
            var curr = robot.measurments[i].robot;
            if (curr.currentCoords.originId == robot.currentCoords.originId)
            {
                if (Vector2.Distance(curr.currentCoords.position, robot.currentCoords.position) < sensorRange)
                {
                    count++;
                }
            }        
        }

        return (float)(count) / (float)(robot.measurments.Count);
    }


    // invadeThing(robot, enemy)
    void invadeThing(Robot robot, Robot enemy)
    {
        if (enemy.invadedById != -1 || enemy.toInvadeId == robot.currentCoords.originId)
        {
            return;
        }
        float invadingNeighborhood = getCloseNeighborhood(robot);
        float enemyNeighborhood = getCloseNeighborhood(enemy);


        if (invadingNeighborhood < 0.25f || enemyNeighborhood > invadingNeighborhood)
        {
            return;
        }

        float chance = 0;
        float enemyDist = Vector2.SqrMagnitude(enemy.currentCoords.position);
        float currDist = Vector2.SqrMagnitude(robot.currentCoords.position);

        //   chance = currDist > enemyDist ? 0.5f : 0.05f;
        float thing = currDist / enemyDist - 1.0f;
        //  chance *= currDist / enemyDist;
        chance = 1 / (1 + Mathf.Exp(-thing));

        if (enemyNeighborhood < 0.25f)
        {
            chance = 0.8f;
        }


        chance = Mathf.Min(0.8f, chance);

        if (Random.Range(0, 1.0f) < chance)
        {
            enemy.getInvadedBy(robot);
            robot.toInvadeId = enemy.currentCoords.originId;
            robot.toInvadeCount = 0;
        }
    }

    int removed = 0;

    // asynchUpdate() to simulate a distributed, unsynchronized system, this method updates agents randomly, allowing all
    //      possible combination of update sequences to occur if given enough time.
    private void asyncUpdate()
    {
        collectMeasurments();
        for (int i = robots.Count - 1; i >= 0; i--)
        {
            int index = Random.Range(0, robots.Count);

            // for the first 10 iterations, different coordinate systems are created, then the coordinate systems will converge to a single one
            if (invadeTick > 10)
            {
                checkInvadedBy(robots[index]);
                checkToInvade(robots[index]);          
            }
            else
            {
                checkNeighbor(robots[index]);
            }
 
            robots[index].position = robots[index].currentCoords.position; // the position is used to display the robots on the screen
        }
    }


    // writeToFile(id) writes the given test results to the file. id represents the id of the test
    public void writeToFile(int id)
    {

        StreamWriter writer = new StreamWriter("Assets/Resources/" +file+".txt", true);
        writer.WriteLine("TEST " + id);
        // write the parameters to the test file
        string currParams = "guess_range: " + guessRange + " sensor_range: " + sensorRange + " num_robots: " + numRobots;
        writer.WriteLine(currParams);
        foreach(var l in toWrite)
        {          
            writer.WriteLine(l);
        }
        writer.WriteLine();
        writer.Close();
    }


    // findInaccurate() prints the number of robots who could not be globally localized
    void findInaccurate()
    {
        // create a dictionary that stores the amount of robots in each coordinate system
        Dictionary<int, int> maxOrigin = new Dictionary<int, int>();
        for (int i = 0; i < robots.Count; i++)
        {
            int id = robots[i].currentCoords.originId;
            if (id == -1)
            {
            }
            if (maxOrigin.ContainsKey(id))
            {
                maxOrigin[id]++;
            }
            else
            {
                maxOrigin.Add(id, 1);
            }
        }

        int maxOriginId = -2;
        int maxOriginCount = 0;

        // find the largest coordinate system
        foreach (var key in maxOrigin.Keys)
        {
            if (maxOrigin[key] > maxOriginCount)
            {
                maxOriginCount = maxOrigin[key];
                maxOriginId = key;
            }
        }

        // print the number of robots not in the largest coordinate system
        int notFound = 0;
        for (int i = 0; i < robots.Count; i++)
        {
            if (robots[i].currentCoords.originId != maxOriginId)
            {
                notFound++;
                robots[i].done = false;
            }
        }
        int found = robots.Count - notFound;

        string outputLine = "Removed: " + removed + " Not_found: " + notFound + " Found " + found;
        toWrite.Add(outputLine);
    }


    // testUpdate() runs the test and returns true when the test has terminated
    public bool testUpdate()
    {
        invadeTick++;
        asyncUpdate();
        doneCount++;
        if (doneCount >= 50)
        {
            part1 = false;
            findInaccurate();
            string outputLoss = "average_loss: " + printLoss();
            toWrite.Add(outputLoss);
            return true;
        } else 
        {
            return false;
        }
    }

    public override void update()
    {
        if (part1)
        {
            invadeTick++;
            asyncUpdate();
            doneCount++;
            if (doneCount >= 50)
            {
                part1 = false;
                findInaccurate();
                string outputLoss = "average_loss: " + printLoss();
                toWrite.Add(outputLoss);

            }
        }
        else
        {          
            base.update();
        } 
    }
}
