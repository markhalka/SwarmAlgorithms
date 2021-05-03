using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// World: 
// this class is responsible for creating, and keeping track of all information on objects in the current simulation
public class World
{
    public List<Mineral> iron;
    public List<Mineral> zinc;
    public List<Mineral> copper;
    public List<Ant> ants;
    public List<Factory> factories;
    int mapRange; // map range represents the size of the map

    // generateSingleWorld(n, mapRange): returns a world initialized with n amount of iron, and with the given mapRange
    public static World genrateSingleWorld(int n, int mapRange = 50)
    {
        World world = new World();
        world.iron = new List<Mineral>();
        world.mapRange = mapRange;
        world.factories = new List<Factory>();
        for(int i = 0; i < n; i++)
        {
            world.iron.Add(new Mineral(MINERAL_TYPE.IRON, VectorUtil.generateRandomVector(mapRange), i));
        }
        return world;
    }

    // generateAnts(n): creates n amount of ants
    public void generateAnts(int n)
    {
        ants = new List<Ant>();
        for(int i = 0; i < n; i++)
        {
            ants.Add(new Ant(VectorUtil.generateRandomVector(mapRange), i, this));
        }

    }
}
