using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Plain object representing the player's state from the server
public class PlayerState 
{
    public string id;
    public string name;
    public Vector2 normalizedPos;
    public float angle;
    public int score;

    public PlayerState(string id, string name, Vector2 normalizedPos, float angle, int score)
    {
        this.id = id;
        this.name = name;
        this.normalizedPos = normalizedPos;
        this.angle = angle;
        this.score = score;
    }
}
