using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaf : MonoBehaviour
{
    public LeafState leafState;

    public void Suck() {
        NetworkController.instance.ReportSuck(this.leafState.id);    
    }

    public void SetLeafState(LeafState leafState)
    {
        this.leafState = leafState;
        transform.position = GameState.instance.NormalizedToWorldPos(leafState.normalizedPos);
    }
}
