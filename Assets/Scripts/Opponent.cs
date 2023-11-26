using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Opponent : MonoBehaviour
{
    PlayerState playerState;

    public void SetPlayerState(PlayerState playerState)
    {
        this.playerState = playerState;
        transform.position = GameState.instance.NormalizedToWorldPos(playerState.normalizedPos);
        transform.rotation = Quaternion.Euler(0, playerState.angle, 0);
    }
}
