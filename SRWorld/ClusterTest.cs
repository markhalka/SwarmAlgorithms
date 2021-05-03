using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ClusterTest
// Responsible for creating a world object, updating all objects in the world, and displaying them on the screen

public class ClusterTest : MonoBehaviour
{

    public GameObject baseParticle;
    List<GameObject> particles;
    List<GameObject> ants;
    List<GameObject> factories;

    [SerializeField]
    public float pickup = 1.0f; // parameter influencing the probability of an ant pickuping up an item

    [SerializeField]
    public float drop = 8.0f; // parameter influencing the probability of an ant dipositing an item

    [SerializeField]
    public float create = 1.0f; // parameter influencing the probability of an ant becoming a factory

    [SerializeField]
    public float leave = 1.0f; //parameter influencing the probability of an ant leaving a factory

    public Button resetBtn;

    World world;

    int TICK_THRESH = 5; // used to control the speed of the simulation
    int count = 0;
    void Start()
    {
        createWorld();
        resetBtn.onClick.AddListener(delegate { reset(); });
    }

    // createWorld() creates a new world and updates all objects in the world
    void createWorld()
    {
        world = World.genrateSingleWorld(50, 400);
        updateParticles();
        world.generateAnts(1);
        updateAnts();
        updateParameters();
    }

    // reset() restarts the simulation with default parameters
    public void reset()
    {
        // just delete old world, and create new one
        createWorld();
    }

    void Update()
    {
        // speed of the simulation is controlled by TICK_THRESH
        if(count < TICK_THRESH)
        {
           count++;
            return;
        }
        count = 0;

        updateAnts();
        updateParticles();
        updateFactories();
    }

    // updateFactories() updates all factories in the current world
    void updateFactories()
    {
        for(int i = 0; i < world.factories.Count; i++)
        {
            world.factories[i].update();
        }
    }


    int paramCount = 0;
    // updateParameters() dynamically updates parameters for all ants the world. 
    //      notes: when running the simulation, the parameters can be adjusted while the simulation is running
    //      this method ensures that after approximatly one second, the changed parameters are updated for all ants
    void updateParameters()
    {
        if(paramCount < 60)
        {
            paramCount++;
            return;
        }
        paramCount = 0;
        for(int i = 0; i < world.ants.Count; i++)
        {
            world.ants[i].updateParameters(pickup, drop, create, leave);
        }
    }

    // createAntObjects(n) creates n ant gameObjects objects that are displayed on the screen
    void createAntObjects(int n)
    {
        for (int i = 0; i < world.ants.Count; i++)
        {
            GameObject curr = Instantiate(baseParticle, baseParticle.transform.parent);
            curr.GetComponent<SpriteRenderer>().color = Color.blue;           
            ants.Add(curr);
        }
    }

    // updateAnts() displays all ants on the screen.
    //      notes: it may create or destroy ant gameObjects,
    void updateAnts()
    {
        if(ants == null)
        {
            ants = new List<GameObject>();
            createAntObjects(world.ants.Count);           
        }

        if (world.ants.Count > ants.Count)
        {
            createAntObjects(world.ants.Count - ants.Count);
        }
   
        for (int i = 0; i < world.ants.Count; i++)
        {
            world.ants[i].update();
            ants[i].transform.position = world.ants[i].position;

            UnityEngine.Color color = Color.blue;

            Ant.ANT_STATE state = world.ants[i].state;       
            if (state == Ant.ANT_STATE.CARRY)
            {
                color = Color.red;
            } else if(state == Ant.ANT_STATE.DROPOFF)
            {
                color = Color.green;
            } else if(state == Ant.ANT_STATE.PICKUP)
            {
                color = Color.yellow;
            } else if(state == Ant.ANT_STATE.FACTORY)
            {
                color = Color.grey;
            }
            ants[i].transform.GetComponentInChildren<SpriteRenderer>().color = color;
        }
    }

    // createParticleObjects(n) creates n particle gameObjects, and displays them on the screen
    void createParticleObjects(int n)
    {
        for (int i = 0; i < n; i++)
        {
            GameObject curr = Instantiate(baseParticle, baseParticle.transform.parent);
            curr.GetComponent<SpriteRenderer>().color = Color.red;
            particles.Add(curr);
        }
    }

    // updateParticles() displays all mineral positions on the screen
    //      notes: may add or remove mineral gameObjects
    void updateParticles()
    {
        if (particles == null)
        {


            particles = new List<GameObject>();
            if (world.iron != null)
            {
                createParticleObjects(world.iron.Count);
            }
        }

             if (world.iron.Count > particles.Count)
             {
            createParticleObjects(world.iron.Count - factories.Count);
             }
             else if (world.iron.Count < particles.Count)
             {
                 for (int i = particles.Count; i > world.iron.Count; i--)
                 {
                     particles.RemoveAt(i);
                 }
             } 


        for (int i = 0; i < world.iron.Count; i++)
        {

            if (!world.iron[i].isActive)
            {
                particles[i].SetActive(false);
                continue;
            }

            particles[i].transform.position = world.iron[i].position;
            if (!world.iron[i].isAvailable)
            {
                particles[i].GetComponent<SpriteRenderer>().color = Color.cyan;
            }
            else
            {
                particles[i].GetComponent<SpriteRenderer>().color = Color.red;
            }   
        }
    }
}
