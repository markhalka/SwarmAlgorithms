using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Factory:
// Responsible for storing all information for a factory object

public class Factory
{

    int MAX_MATERIAL = 3; // constant for the maximum amount of material a factory can process per tick
    int ANT_COST = 6; // the default amount of material an ant costs
    int TIME_DELTA = 10; // the amount of system updates needed for one factory update, in Unity, system updates are approx. 60 hz

    public Vector2 position;

    List<Ant> ants;
    int[] productionValues;
    

    World world;

    static int factoryCount; // amount of factories created during the course of the simulation, used for assigning unique id's
    public int productionPerTick = 0; // represents the short-term history of production of a factory, used to determine its productivity
    public int materialSize; // amount of material within the radius of the factory, which is then communicated to passing ants
    public int processedMaterial = 0;  // current amount of processed material, used to create new ants
    public int id;
    int radius; // radius defines the range a factory has on processing particles
    int antCount; // amount of ants produced by the factory over the course of its lifetime
    int productionIndex;
 
    int currTime = 0;
    bool isFull = false;

    // Factory(world, position, radius) creates a new factory object with the given parameters
    private Factory(World world, Vector2 position, int radius = 2000)
    {
        ants = new List<Ant>();
        this.world = world;
        this.position = position;
        this.radius = radius;
        id = factoryCount++;
        world.factories.Add(this);
        productionValues = new int[10];
        productionIndex = 0;
    }

    // Factory(ant) creates a new factory which is composed of the given ant
    public Factory(Ant ant) : this(ant.world, ant.position)
    {
        ants.Add(ant);
    }

    // addAnt(ant) adds the given ant to the factory
    public void addAnt(Ant ant)
    {
        ants.Add(ant);
        ant.state = Ant.ANT_STATE.FACTORY;
    }

    // removeAnt(ant) removes the ant from the factory, and if there are no more ants, removes the factory
    public void removeAnt(Ant ant)
    {
        ants.Remove(ant);
        if(ants.Count == 0)
        {
            world.factories.Remove(this);
        }
    }

    // isValidMineral(mineral) returns true if the mineral is available, active, and within range of the factory, false otherwise
    private bool isValidMineral(Mineral mineral)
    {
        return mineral.isAvailable && mineral.isActive && Vector2.Distance(mineral.position, position) < radius;
    }

    // getMaterial(currentMax) produces a list of valid minerals of length less than or equal to currentMax
    // effects: also updates the materialSize count
    List<Mineral> getMaterial(int currentMax)
    {
        List<Mineral> output = new List<Mineral>();
        materialSize = 0;
        int count = 0;
        for(int i = 0; i < world.iron.Count; i++)
        {
            if(isValidMineral(world.iron[i]))
            {
                if (count < currentMax)
                {
                    output.Add(world.iron[i]);
                    count++;
                }
                materialSize++;
            }
        }
        return output;
    }


    // createAnt() creates a new ant from processed material
    // notes:
    //      the new ant has a 3 digit id format. The first digit is the factories id, the last two refer to the n'th ant the factory produced
    //      example: factory 6 produced its 15'th ant, the ant has id: 615
    private void createAnt()
    {
        int antId = id * 100 + antCount;
        Ant ant = new Ant(position, antId, world);
        world.ants.Add(ant); // there should probably be a better function for this
        antCount++;
    }

    // update() updates the factory with speed given by TIME_DELTA. On each iteration, the factory
    //  gather nearby material, processess it, and if it has sufficient material, produces a new ant
    public void update()
    {
        // update time based on TIME_DELTA
        currTime++;
        if(currTime < TIME_DELTA)
        {
            return;
        }
        currTime = 0;

        int value = 0;  
        // the amount of materials a factory can process is proportional to the amount of ants in the factory
        int currentMax = Mathf.Min(MAX_MATERIAL, ants.Count);
        List<Mineral> minerals = getMaterial(currentMax);

        foreach(var m in minerals)
        {
            // once a mineral is processed, it is no longer active
            m.isActive = false;
            processedMaterial++;
        }

        if (processedMaterial >= ANT_COST)
        {
            createAnt();
            processedMaterial -= ANT_COST;
            value = 1;
        }

// update a stack of of previous production values, with an array
// the array is composed of 1 or 0, corresponding to whether or not ants were produced during that time interval
        if (productionIndex >= 9)
        {
            productionIndex = 0;
            isFull = true;
        }
        else if (!isFull)
        {
            productionPerTick += value;
            productionValues[productionIndex++] = value;
            return;
        }

        productionPerTick += value - productionValues[productionIndex];
        productionValues[productionIndex++] = value;
    }
}

