using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// VectorUtil
// impliments general useful methods manipulating vectors
public class VectorUtil
{
    // printVector(toPrint) prints the given vector in a nice format
    public static void printVector(Vector3 toPrint)
    {
        Debug.LogError(toPrint.x + " " + toPrint.y + " " + toPrint.z);
    }


    // generateRandomVector(range) generates a new 3d vector with x,y components in the range [0, range], and z = 0
    public static Vector3 generateRandomVector(int range)
    {
        return new Vector3(Random.Range(0, range), Random.Range(0, range), 0);
    }
}
