using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


// Neighborhood
// this class contains a group of robots that all share the same coordinate system
public class Neighborhood
{
    public List<Robot> neighbours;

    // Neighborhood(neighbours) creates a new neighborhood object with the givne parameters
    public Neighborhood(List<Robot> neighbours)
    {
        this.neighbours = neighbours;
    }
}
