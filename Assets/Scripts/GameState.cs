using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    [Header("Set in inspector")]
    [SerializeField] Vector2 worldSize;
    [SerializeField] GameObject opponentsContainer;
    [SerializeField] GameObject leavesContainer;

    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject opponentPrefab;
    [SerializeField] GameObject leafPrefab;

    PlayerController activePlayer;
    Dictionary<string, Opponent> opponents = new Dictionary<string, Opponent>();
    Dictionary<string, Leaf> leaves = new Dictionary<string, Leaf>();

    public static GameState instance;
    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
            instance = this; // this is the first instance of the class
        else
            Destroy(gameObject); // this must be a duplicate from a scene reload - DESTROY!
    }

    public Vector3 NormalizedToWorldPos(Vector2 normalizedPos)
    {
        return new Vector3(normalizedPos.x * worldSize.x - (0.5f * worldSize.x), 0, normalizedPos.y * worldSize.y - (0.5f * worldSize.y));
    }

    public Vector2 WorldToNormalizedPos(Vector3 worldPos)
    {
        return new Vector2((worldPos.x + (0.5f * worldSize.x)) / worldSize.x, (worldPos.z + (0.5f * worldSize.y)) / worldSize.y);
    }

// TODO: bug
    // public void ConnectionClosed(){
    //     opponents.Clear();
    //     leaves.Clear();
    // }

    public void CreateActivePlayer(PlayerState playerState)
    {
        // TODO: dont instantiate raw normalized pos
        GameObject player = Instantiate(playerPrefab, NormalizedToWorldPos(playerState.normalizedPos), Quaternion.identity);
        activePlayer = player.GetComponent<PlayerController>();
        activePlayer.SetPlayerState(playerState);

        Debug.Log("Created active player with id " + playerState.id);
    }

    public void HandleGameUpdate(Dictionary<string, PlayerState> updatedPlayers, Dictionary<string, LeafState> updatedLeaves)
    {
        HashSet<string> playersFromServer = new HashSet<string>(updatedPlayers.Keys);
        HashSet<string> localPlayers = new HashSet<string>(opponents.Keys);
        HashSet<string> leavesFromServer = new HashSet<string>(updatedLeaves.Keys);
        HashSet<string> localLeaves = new HashSet<string>(leaves.Keys);
        
        // Remove players that are no longer in the game (not in updatedPlayers)
        HashSet<string> playersToRemove = new HashSet<string>(localPlayers);
        playersToRemove.ExceptWith(playersFromServer);
        
        // Add players that are new to the game (not in localPlayers)
        HashSet<string> playersToAdd = new HashSet<string>(playersFromServer);
        playersToAdd.ExceptWith(localPlayers);

        HashSet<string> existingPlayers = new HashSet<string>(localPlayers);
        existingPlayers.IntersectWith(playersFromServer);

        // Remove leaves that are no longer in the game (not in updatedLeaves)
        HashSet<string> leavesToRemove = new HashSet<string>(localLeaves);
        leavesToRemove.ExceptWith(leavesFromServer);

        // Add leaves that are new to the game (not in localLeaves)
        HashSet<string> leavesToAdd = new HashSet<string>(leavesFromServer);
        leavesToAdd.ExceptWith(localLeaves);

        HashSet<string> existingLeaves = new HashSet<string>(localLeaves);
        existingLeaves.IntersectWith(leavesFromServer);

        
        foreach (string playerId in playersToRemove)
        {
            if (playerId == activePlayer.playerState.id) continue;

            Debug.Log("Player " + playerId + " left the game");
            Destroy(opponents[playerId].gameObject);
            opponents.Remove(playerId);
        }

        foreach (string playerId in playersToAdd)
        {
            if (playerId == activePlayer.playerState.id) continue;

            PlayerState playerState = updatedPlayers[playerId];
            Debug.Log("Player " + playerState.id + " joined the game");
            GameObject opponent = Instantiate(opponentPrefab, NormalizedToWorldPos(playerState.normalizedPos), Quaternion.identity, opponentsContainer.transform);
            Opponent opponentScript = opponent.GetComponent<Opponent>();
            opponentScript.SetPlayerState(playerState);
            opponents.Add(playerId, opponentScript);
        }

        foreach (string playerId in existingPlayers)
        {
            if (playerId == activePlayer.playerState.id) continue;

            // TODO: exclude active player
            PlayerState playerState = updatedPlayers[playerId];
            opponents[playerId].SetPlayerState(playerState);
        }

        foreach (string leafId in leavesToRemove)
        {
            // Debug.Log("Rip leaf " + leafId);
            Destroy(leaves[leafId].gameObject);
            leaves.Remove(leafId);
        }

        foreach (string leafId in leavesToAdd)
        {
            LeafState leaf = updatedLeaves[leafId];
            // Debug.Log("New leaf " + leafId);
            GameObject leafObject = Instantiate(leafPrefab, NormalizedToWorldPos(leaf.normalizedPos), Quaternion.identity, leavesContainer.transform);
            Leaf leafScript = leafObject.GetComponent<Leaf>();
            leafScript.SetLeafState(leaf);
            leaves.Add(leafId, leafScript);
        }

        foreach (string leafId in existingLeaves)
        {
            LeafState leaf = updatedLeaves[leafId];
            leaves[leafId].SetLeafState(leaf);
        }
    }
}
