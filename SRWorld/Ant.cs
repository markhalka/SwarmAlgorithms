using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;



// Ant 
//  this class contains all functions relating to the individual agents in the simulation. They collect minerals
//  in the environment, and sort them into groups. If the groups are a certain size, they may become factories
//  which process minerals in an area, and produce more ants.
//  the ants are based on a threshold model, for dynamic task allocation, and together represent a swarm intelligent system
public class Ant
{
    public World world;
    public Factory factory;
    
    public Vector2 position;
    public int id;
    public Dictionary<MINERAL_TYPE, Vector2> cachePoints;

    int THRESH_DEGREE = 2; // the degree of the threshold function, a higher degree makes the threshold function less steep
    float THRESH_PICKUP = 1; // threshold constant for picking up an item. The higher the constant the more stimulus is needed for a response
    float THRESH_DROP = 8; // threshold constant for dropping an item
    float THRESH_CREATE_FACTORY = 1f; // threshold constant for creating a factory
    float THRESH_LEAVE_FACTORY = 2f; // threshold function for leaving a factory

    Vector2 goalPos;
    public int goalMineralId;
    public int goalFactoryId;
    
    Vector2 cachePos;
    int cacheSize;

    public enum ANT_STATE { PICKUP, CARRY, DROPOFF, NEUTRAL, FACTORY, JOIN };
    public ANT_STATE state;
    float speed = 10;

    public int sensorRange; // the distance that a single ant can see
    public int groupRange; // the distance that qualifies two minerals as being in a group
    public int factoryTime = 0; // the amount of time spent in a factory

    bool isHome = false; // true if the ant is at its cache position
    int timeLost = 0; // the amount of time an ant spends lost, (when it cannot find a mineral to pickup)


    // Ant(position, id, world, sensorRange) creates a new ant with the given parameters
    public Ant(Vector2 position, int id, World world, int sensorRange = 150)
    {
        this.position = position;
        this.id = id;
        this.sensorRange = sensorRange;
        this.world = world;
        cachePoints = new Dictionary<MINERAL_TYPE, Vector2>();  
        state = ANT_STATE.NEUTRAL;
        goalPos = position;
        goalMineralId = -1;
        groupRange = 50;
        cachePos = Vector2.zero;
        cacheSize = 0;
    }


    // updateParameters(pickup, drop, create, leave) updates the ants parameters
    public void updateParameters(float pickup, float drop, float create, float leave)
    {
        THRESH_PICKUP = pickup;
        THRESH_DROP = drop;
        THRESH_CREATE_FACTORY = create;
        THRESH_LEAVE_FACTORY = leave;
    }


    // update() updates the ants state and position
    public void update()
    {
        Vector2 pos; 
        if (state == ANT_STATE.NEUTRAL)
        {         
            if(decideGroup(true, out pos)){ 
                // if the ant finds a group of minerals, and decides to go, it enters the pickup state
                state = ANT_STATE.PICKUP;
                goalPos = pos;
            }    
        }
        else if (state == ANT_STATE.CARRY)
        {
            if (decideGroup(false, out pos))
            {
                // if an ant is carrying a mineral, finds a place to deposit, and decides to go, it enters the dropoff state
                state = ANT_STATE.DROPOFF;
                goalPos = pos;
            }
        }

        if (state == ANT_STATE.DROPOFF) 
        {
            if (updateMoving())
            {
                //after an ant drops off a mineral, it may choose to become a factory, depending on the amount of unprocessed minerals
                Factory factory;
                if(decideFactory(out factory))
                {
                    goalFactoryId = factory.id;
                    state = ANT_STATE.JOIN;
                }
            }
        } else if (state == ANT_STATE.PICKUP)
        {
            updateMoving();
        } else if(state == ANT_STATE.CARRY  || state == ANT_STATE.NEUTRAL)
        {
            if (updateMoving())
            {
                // keep moving until a mineral is found
                goalPos = wander();
            }
        } else if(state == ANT_STATE.FACTORY)
        {
            // if an ant is in a factory, it may choose to leave the factory
            decideAnt();
        } else if(state == ANT_STATE.JOIN)
        {
            // if the ant reaches the factory they are moving to, their state becomes factory
            if (updateMoving())
            {
                state = ANT_STATE.FACTORY;
            }
        }
    }


    // dfs(visited, iron, index) updates visited to be true for all minerals that are apart of the 
    //  given minerals group, and are valid.
    private void dfs(ref bool[] visited, List<Mineral> iron, int index)
    {
        for(int i = 0; i < iron.Count; i++)
        {
            if (i == index)
                continue;

            if (!visited[i] && Vector2.Distance(iron[i].position, iron[index].position) < groupRange && isValidMineral(iron[i]))
            {
                visited[i] = true;
                dfs(ref visited, iron, i);
            }
        }
    }


    // isValidMineral(mineral) returns true if the mineral is valid, false otherwise
    private bool isValidMineral(Mineral mineral)
    {
        return mineral.isAvailable && mineral.isActive && Vector2.Distance(mineral.position, position) < sensorRange;
    }

    
    // scanNeighborhood() returns a list of mineral groups that the ant can see
    private List<MineralGroup> scanNeighborhood()
    {
        List<MineralGroup> mineralGroups = new List<MineralGroup>();
        int len = world.iron.Count;
        bool[] globalVisited = new bool[len]; // this will hold information about all the minerals an ant can see
        bool[] visited = new bool[len]; // this will hold information about the current mineral group

        for (int i = 0; i < len; i++)
        {
            visited = new bool[len];
            List<Mineral> currMinerals = new List<Mineral>();

            if (!globalVisited[i] && isValidMineral(world.iron[i]))
            {
                // if the current mineral is not yet apart of a group, create a new group and find all minerals in the same group
                currMinerals.Add(world.iron[i]);
                dfs(ref visited, world.iron, i);
            } else
            {
                continue;
            }

            Vector2 mineralGroupCentroid = Vector2.zero;
            int count = 1;
            for (int j = 0; j < visited.Length; j++)
            {
                if (visited[j])
                {
                    currMinerals.Add(world.iron[j]);
                    globalVisited[j] = true;
                    mineralGroupCentroid += world.iron[j].position;
                    count++;
                }
            }

            // estimate the center of the group
            mineralGroupCentroid /= count;
            
            mineralGroups.Add(new MineralGroup(mineralGroupCentroid, currMinerals));

        }
        return mineralGroups;
    }


    // updateMoving() depending on the ant state, the method will update the ants position, and may update the state, it returns true if
    //      the ant has reached its goal destination
    bool updateMoving()
    {
        
        if(state == ANT_STATE.PICKUP && !world.iron[goalMineralId].isAvailable) 
        {
            // if another ant picked up the mineral, return to neutral to find a new mineral
            state = ANT_STATE.NEUTRAL;
            return false;
        } else if(state == ANT_STATE.JOIN)
        {
            // if the factory disbanded, return to neutral
            bool disbanded = true;
            foreach(var f in world.factories)
            {
                if(f.id == goalFactoryId)
                {
                    disbanded = false;
                    break;
                }
            }
            if (disbanded)
            {
                state = ANT_STATE.NEUTRAL;
                return false;
            }
        }

        int deltaX = goalPos.x >= position.x ? (goalPos.x > position.x ? 1 : 0) : -1;
        int deltaY = goalPos.y >= position.y ? (goalPos.y > position.y ? 1 : 0) : -1;
       
        position.x += deltaX * speed;
        position.y += deltaY * speed;

        if(Vector2.Distance(position, goalPos) < groupRange/2)
        {
           // if the ant is sufficiently close to the goal position

            if (state == ANT_STATE.PICKUP)
            {
                // grab the mineral
                world.iron[goalMineralId].isAvailable = false;
                state = ANT_STATE.CARRY;
            } else if(state == ANT_STATE.DROPOFF)
            {
                // drop the mineral
                world.iron[goalMineralId].isAvailable = true;
                world.iron[goalMineralId].position = position;
                state = ANT_STATE.NEUTRAL;
            }

            if(Vector2.Distance(goalPos, cachePos) < groupRange / 2)
            {
                // the ant is at its cache position, this ensures that it does not take minerals from its own cache
                isHome = true;
            }

            return true;
        }
        return false;
    }

    // wander() picks a random location, and makes the ant move towards it
    Vector2 wander()
    {
        if (!isHome)
        {
            return cachePos;
        }

        float radius = Random.Range(200, 600);
        Vector2 goalVec = Random.onUnitSphere * radius;
        isHome = false;
        return position + goalVec;
    }


    // chooseClosestMineral(group, pos) returns the closest position of the closest mineral
    //  in the group to the ant
    bool chooseClosestMineral(MineralGroup group, out Vector2 pos)
    {
        // sort based on distance to the ant
        group.items.Sort(delegate (Mineral a, Mineral b)
        {
            float da = Vector2.Distance(a.position, position);
            float db = Vector2.Distance(b.position, position);
            if (da > db)
            {
                return 1;
            }
            return da == db ? 0 : -1;
        });

        // only choose minerals that are available
        int index = 0;
        for (; index < group.items.Count; index++)
        {
            if (world.iron[group.items[index].id].isAvailable)
            {
                break;
            }
        }
        goalMineralId = group.items[index].id;

        
        if (index >= group.items.Count)
        {
            // could not find a mineral that is available
            pos = Vector2.zero;
            return false;
        }

        pos = group.items[index].position;
        return true;
    }


    // shareInfo() share cache points and factory locations with nearby ants
    private void shareInfo()
    {
        List<Ant> nearbyAnts = findAnts();
        for (int i = 0; i < nearbyAnts.Count; i++)
        {
            if (cacheSize < nearbyAnts[i].cacheSize)
            {
                // then output cachepos
            }
        }
    }


    // findAnts() returns a list of ants that are near the current ant
    private List<Ant> findAnts()
    {
        List<Ant> outputAnts = new List<Ant>();
        for(int i = 0; i < world.ants.Count; i++)
        {
            if(Vector2.Distance(position, world.ants[i].position) < sensorRange)
            {
                outputAnts.Add(world.ants[i]);
            }
        }
        return outputAnts;
    }

    // findFactories() returns a list of factories that are near the ant
    private List<Factory> findFactories()
    {
        List<Factory> outputFactories = new List<Factory>();
        for (int i = 0; i < world.factories.Count; i++)
        {
            if (Vector2.Distance(world.factories[i].position, position) < sensorRange)
            {
                outputFactories.Add(world.factories[i]);
            }
        }
        return outputFactories;
    }


    // decideGroup(pickup, out pos) returns the position of 
    private bool decideGroup(bool pickup, out Vector2 pos)
    {
        List<MineralGroup> mineralGroups = scanNeighborhood();
        List<Factory> factoryGroups = findFactories();


        if (factoryGroups.Count > 0)
        {
            // sort factoryGroups by the amount of unprocessed material they have
            factoryGroups.Sort(delegate (Factory a, Factory b)
            {
                if (a.materialSize < b.materialSize)
                {
                    return -1;
                }
                else if (a.materialSize == b.materialSize)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }

            });         
        }

        // sort mineralGroups by the amount of minerals they have
        if (mineralGroups.Count > 0)
        {
            mineralGroups.Sort(delegate (MineralGroup a, MineralGroup b)
            {
                if (a.items.Count > b.items.Count)
                {
                    return 1;
                }
                else if (a.items.Count == b.items.Count)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            });
        }

        if (pickup)
        {
            // if the ant is picking up, find the smallest group
            if (mineralGroups.Count <= 0)
            {
                pos = Vector2.zero;
                return false;
            }

            int index = 0;

            // check to make sure the mineral is not apart of the cache group 
            for (; index < mineralGroups.Count; index++)
            {
                if (Vector2.Distance(mineralGroups[index].position, cachePos) >= groupRange * 1.5f)
                    break;
            }

            // no mineral was found
            if (index >= mineralGroups.Count)
            {
                pos = Vector2.zero;
                return false;
            }

            if (thresholdFunction(mineralGroups[index].items.Count, THRESH_PICKUP))
            {
                // if the ant decides to pickup a mineral, return the closest one from the smallest group
                return chooseClosestMineral(mineralGroups[index], out pos);
            }

        } else
        {
            // if there are factories nearby, go to the closest one
            if(factoryGroups.Count > 0)
            {
                cachePos = factoryGroups[0].position;
                pos = cachePos;
                return true;
            }

            // if there are no mineral groups, just go to the cache position
            if(mineralGroups.Count <= 0)
            {
                pos = cachePos;
                return true;
            }

            // otherwise, find the largest mineral group, if it larger than the cache group, update the cache group
            var largest = mineralGroups[mineralGroups.Count - 1];
            if (cachePos == Vector2.zero)
            {
                cachePos = largest.position;
                cacheSize = largest.items.Count;
            }
            else if (largest.items.Count > cacheSize)
            {
                cachePos = largest.position;
                cacheSize = largest.items.Count;
            }

            pos = cachePos;
            return true; 
        }


        pos = Vector2.zero;
        return false;
    }


    // decideFactory(out outputFactory) returns true if the ant will join a factory. Otherwise, the ant may choose to become a factory
    //      or not
    private bool decideFactory(out Factory outputFactory)
    {
        // the largest group an ants knows of is its cache by definition
        int groupSize = cacheSize;

        // the decision to become a factory is based on the amount of unprocessed material
        if (thresholdFunction(groupSize, THRESH_CREATE_FACTORY))
        {
            List<Factory> outputFactories = findFactories();

            if (outputFactories.Count > 0)
            {
                // find closest factory 
                Factory goal = outputFactories[0];
                for (int i = 1; i < outputFactories.Count; i++)
                {
                    if (Vector2.Distance(goal.position, position) > Vector2.Distance(outputFactories[i].position, position))
                    {
                        goal = outputFactories[i];
                    }
                }

                outputFactory = goal;
                return true;
            }
            else
            {
                // no nearby factories, so create one
                outputFactory = new Factory(this);
                factory = outputFactory;
                state = ANT_STATE.FACTORY;

                return false;
            }
        }

        outputFactory = null;
        return false;
    }


    // decideAnt() returns true if the ant decides to leave the factory, and become an ant again
    private bool decideAnt()
    {
        // after a certain amount of time, if the factory process no material the ant will leave
        if(factory.processedMaterial == 0)
        {
            factoryTime++;
        }
        
        if(factoryTime > 100)
        {
            factoryTime = 0;
            state = ANT_STATE.NEUTRAL;
            factory.removeAnt(this);
            return true;
        } 

        if (factory.processedMaterial > 0 && !thresholdFunction(factory.materialSize, THRESH_LEAVE_FACTORY)) 
        {
            // otherwise, the ant has a chance of leaving inversely related to the amount of material processed by the factory
            state = ANT_STATE.NEUTRAL;
            factory.removeAnt(this);
            return true;
        }
        return false;
    }
    

    // thresholdFunction(stimulus, threshold) returns true if the ant decides the act based on their precieved stimulus and threshold
    private bool thresholdFunction(float stimulus, float threshold)
    {

        float chance = Mathf.Pow(stimulus, THRESH_DEGREE) / (Mathf.Pow(stimulus, THRESH_DEGREE) + Mathf.Pow(threshold, THRESH_DEGREE));
        if(chance > Random.Range(0, 1.0f))
        {
            return true;
        } else
        {
            return false;
        }
    }
}
