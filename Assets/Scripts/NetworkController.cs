using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class NetworkController : MonoBehaviour
{
    public static NetworkController instance;


    WebSocket websocket;

    void Start()
    {
        if (instance == null)
        {
            instance = this; // this is the first instance of the class
        }
        else
        {
            Destroy(gameObject); // this must be a duplicate from a scene reload - DESTROY!
            return;
        }

        // websocket = new WebSocket("wss://opvih5x3uask4clcxgozpxglhi.srv.us");
        websocket = new WebSocket("wss://leafsucker.mnt.dev/ws");

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            // TODO: show error message to user
            Debug.Log("Connection closed!");
        };

        websocket.OnMessage += (bytes) =>
        {
            // Debug.Log("OnMessage!");
            // Debug.Log(bytes);

            // getting the message as a string
            var payload = System.Text.Encoding.UTF8.GetString(bytes);
            // Debug.Log("OnMessage! " + message);

            JObject data = JObject.Parse(payload);
            // Debug.Log("data: " + data);
            JToken typeRaw = data.GetValue("type");
            // Debug.Log("typeRaw:" + typeRaw);
            if (typeRaw == null)
            {
                Debug.Log("typeRaw is null");
                return;
            }
            string type = typeRaw.ToObject<string>();
            if (type == "update")
            {
                // update game 
                JToken playersRaw = data.GetValue("players");
                JToken leavesRaw = data.GetValue("leaves");

                // Debug.Log("Players type: " + playersRaw.GetType());
                // Debug.Log("Leaves type: " + leavesRaw.GetType());

                Dictionary<string, PlayerState> players = new Dictionary<string, PlayerState>();
                Dictionary<string, LeafState> leaves = new Dictionary<string, LeafState>();
                foreach (JProperty playerPair in playersRaw) {
                    string uuid = playerPair.Name;
                    JToken rest = playerPair.Value;
                    string name = rest["name"].ToObject<string>();
                    float angle = rest["angle"].ToObject<float>();
                    int score = rest["score"].ToObject<int>();
                    JToken position = rest["position"];
                    float x = position["x"].ToObject<float>();
                    float y = position["y"].ToObject<float>();
                    PlayerState playerState = new PlayerState(uuid, name, new Vector2(x, y), angle, score);
                    players.Add(uuid, playerState);
                }

                foreach (JProperty leafPair in leavesRaw) {
                    string uuid = leafPair.Name;
                    JToken rest = leafPair.Value;
                    JToken position = rest["position"];
                    float x = position["x"].ToObject<float>();
                    float y = position["y"].ToObject<float>();
                    LeafState leafState = new LeafState(uuid, new Vector2(x, y));
                    leaves.Add(uuid, leafState);
                }

                HandleGameUpdate(players, leaves);
            }
            else if (type == "assign")
            {
                // set our UUID
                JToken uuidRaw = data.GetValue("uuid");
                string uuid = uuidRaw.ToObject<string>();
                // Debug.Log("uuid type: " + uuidRaw.GetType());
                Debug.Log("assign player uuid: " + uuid);
                JToken nameRaw = data.GetValue("name");
                string name = nameRaw.ToObject<string>();
                JToken positionRaw = data.GetValue("position");
                float x = positionRaw["x"].ToObject<float>();
                float y = positionRaw["y"].ToObject<float>();

                // we have our uuid as a string

                PlayerState playerState = new PlayerState(uuid, name, new Vector2(x, y), 0, 0);
                GameState.instance.CreateActivePlayer(playerState);
            }
            else
            {
                // something fucky happened
                Debug.Log("Got other message");
            }
        };

        // Keep sending messages at every 0.3s
        // InvokeRepeating("SendWebSocketMessage", 0.0f, 0.3f);

        // waiting for messages
        websocket.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif
    }

    async void SendWebSocketMessage()
    {
        if (websocket == null)
        {
            Debug.LogError("Bad socket, can't send message");
            return;
        }

        if (websocket.State == WebSocketState.Open)
        {
            // Sending bytes
            // await websocket.Send(new byte[] { 10, 20, 30 });

            // Sending plain text
            await websocket.SendText("{\"name\":\"brennan\"}");
        }
    }

    private async void OnApplicationQuit()
    {
        if (websocket == null) return;
        await websocket.Close();
    }

    public void ReportSuck(string uuid) {
        ClaimMessage msg = new ClaimMessage(uuid);
        // Debug.Log(msg.ToString());
        websocket.SendText(msg.ToString());
    }

    public void ReportMove(Vector2 pos, float angle) {
        MoveMessage msg = new MoveMessage(pos.x, pos.y, angle);
        // Debug.Log(msg.ToString());
        websocket.SendText(msg.ToString());
    }

    // Create (active) player
    void HandleAssign(string uuid, string name, Vector2 normalizedPos)
    {
        PlayerState playerState = new PlayerState(uuid, name, normalizedPos, 0, 0);
        GameState.instance.CreateActivePlayer(playerState);
    }

    // Update board state: player and leaves info 
    void HandleGameUpdate(Dictionary<string, PlayerState> players, Dictionary<string, LeafState> leaves)
    {
        GameState.instance.HandleGameUpdate(players, leaves);
    }
    
    class ClaimMessage {
        public string uuid;
        public string type;

        public ClaimMessage(string uuid) {
            this.uuid = uuid;
            this.type = "claim";
        }

        public string ToString() {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }

        // public 
    }

    class MoveMessage {
        public float x;
        public float y;
        public float angle;
        public string type;

        public MoveMessage(float x, float y, float angle) {
            this.x = x;
            this.y = y;
            this.angle = angle;
            this.type = "move";
        }

        public string ToString() {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }
    }
}