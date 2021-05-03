using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// MineralGroup
//  represents a group of minerals close to each other
public class MineralGroup
{
    public Vector2 position; // the position of the centroid of the group
    public List<Mineral> items; // the individual minerals in the group

    // MineralGroup(position, items) creates a new mineralGroup object with the given parameters
    public MineralGroup(Vector2 position, List<Mineral> items)
    {
        this.position = position;
        this.items = items;
    }
}
