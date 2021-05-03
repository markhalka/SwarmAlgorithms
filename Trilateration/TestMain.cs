using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/*
Average error vs.iterations
Convergence time to< 95% error vs swarm size
Convergence time <95% vs.percentage of failures
How different parameters effect it (probability of seed, invasion)
Convergence time<95% vs.average noise
*/

// nvm, looks like it doesnt do too good with a larger number of agents
// just run it from 300 to 800, and that should be good
// keep the connectivity highish, but vary that as well (or just show the swarm density vs rate)

// so things to keep track of:
// 1. amount of iterations and error
// 2. initial parameters
// thats pretty much it 

// TestMain
// responsible for running the trilateration simulation
public class TestMain : MonoBehaviour
{

    public GameObject robot; // default robot object, that is copied and used to display robots on the screen
    LocalTrilateration tr; // current trilateration class
    GameObject[] localRobots; // array of gameobjects that show the robots estimated positions
    GameObject[] globalRobots; // array of gameobjects that show the robots actual positions

    Color[] colors = new Color[] { Color.white, Color.green, Color.black, Color.blue, Color.cyan, Color.magenta, Color.yellow };

    #region tests
    // test1() tests the
    private void test1()
    {
        for (int j = 300; j <= 800; j+=100)
        {
            for (int i = 0; i < 10; i++)
            {
                tr = new LocalTrilateration(300, 0.001f, 50, 300, "test"+j);
                while (!tr.testUpdate())
                {
                    //   printDistances();
                }

                tr.writeToFile(i);
            }
        }
    }

    // this test will run the divide and conquer with a swarm size of 800, test size of 400, comm distance of 30, and a drop out rate from
    // 10%-50% (probability of agent failing during the entire run) 
    private void test2()
    {

    }

    // this test will run the divide and conquer with a swarm size of 800, test size of 400, and comm distance of 30, with distance inaccuracy measurment
    // from 5% to 20%
    private void test3()
    {

    }

    // this test will run the divide and conquer with different seed probabilities from 1/n, 10/n, 100/n
    private void test4()
    {

    }

    // this test will run the divide and conquer with different invasion probabilities (make it bigger and smaller)
    private void test5()
    {

    }

    #endregion

    void Start()
    {

        test1();
       /* localRobots = null;
        globalRobots = null;
        tr = new LocalTrilateration(800, 0.0005f, 50, 400); //= new GlobalTrilateration(10);
  //      tr.smallerAreaTest();
        tr.update();
        printDistances(); */
    }

    float distance = 2f;


    // getNormPosition(n, out actual, out guess) returns the the first n robots actual and estimated positions, all with the same length
    public void getNormPosition(int n, out List<Vector3> actual, out List<Vector3> guess)
    {
        List<Robot> robots = tr.robots;

        actual = new List<Vector3>();
        guess = new List<Vector3>();


        for (int i = 0; i < n; i++)
        {
            actual.Add(robots[i].globalPosition.normalized * distance);
            guess.Add(robots[i].position.normalized * distance);
        }
    }

    // printDistances() displays the robots actual and estimated positions on the screen, as well as other information
    void printDistances()
    {

        List<Vector3> globalPos;
        List<Vector3> localPos;
        Vector3 offset = new Vector3(0, distance*500, 0); // used to display the estimated positions above the actual positions on the screen

        getNormPosition(tr.robots.Count, out globalPos, out localPos);
        if(localRobots == null)
        {
            // if this is the first time calling the method, create all gameobjects
            localRobots = new GameObject[globalPos.Count];
            globalRobots = new GameObject[globalPos.Count];

            for (int i = 0; i < globalPos.Count; i++)
            {
                GameObject globalRobot = Instantiate(robot.gameObject, robot.transform.parent);
                GameObject localRobot = Instantiate(robot.gameObject, robot.transform.parent);
                localRobots[i] = localRobot;
                globalRobots[i] = globalRobot;
            }

        }

        for (int i = 0; i < localPos.Count; i++)
        {
            globalRobots[i].transform.position = globalPos[i];
            localRobots[i].transform.position = localPos[i] + offset;

            // display the estimated robots with colors corresponding to their groups
            globalRobots[i].GetComponent<SpriteRenderer>().color = Color.red;
            localRobots[i].GetComponent<SpriteRenderer>().color = colors[(tr.robots[i].currentCoords.originId + 1) % colors.Length];

            globalRobots[i].GetComponentInChildren<TMP_Text>().text = i.ToString();
            localRobots[i].GetComponentInChildren<TMP_Text>().text = i.ToString();
        }
    }

    int count = 0; // count determines the speed of the simulation

    void Update()
    {

        /* if(count < 60)
         {
             count++;
             return;
         }
         count = 0; 
        
        tr.update();
        printDistances();  */
    } 
}
