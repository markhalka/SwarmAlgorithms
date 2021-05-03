using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Mineral: 
// Responsible for storing all information relevant to minerals

public enum MINERAL_TYPE { IRON, COPPER, ZINC };
public class Mineral
{
    public Vector2 position;
    MINERAL_TYPE mineralType;
    public bool isAvailable; // a mineral is unavailable if it is being carried by an ant, otherwise it is available
    public bool isActive; // a mineral becomes inactive after it is processed by a factory.
    public int id;
    

    // Mineral(mineralType, position, id) produces a new mineral object with the given parameters
    public Mineral(MINERAL_TYPE mineralType, Vector2 position, int id)
    {
        this.mineralType = mineralType;
        this.position = position;
        isAvailable = true;
        this.id = id;
        isActive = true;
    }
}
