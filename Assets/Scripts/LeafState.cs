using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafState
{
    public string id;
    public Vector2 normalizedPos;   // from server; 0 to 1

    public LeafState(string id, Vector2 normalizedPos)
    {
        this.id = id;
        this.normalizedPos = normalizedPos;
    }
}
