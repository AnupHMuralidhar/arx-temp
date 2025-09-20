using System;
using System.Text;
using UnityEngine;
using NativeWebSocket;

[Serializable]
public class PositionData
{
    public float x;
    public float y;
    public float z;
}

public class WebSocketClient : MonoBehaviour
{
    WebSocket websocket;
    public int backendPort = 8765;

    public ARPlaneController planeController;
    public Action OnWebSocketConnected;

    // 🔗 Called by UDPDiscoveryClient once backend IP is found
    public async void Connect(string backendIP)   // 👈 renamed to "Connect"
    {
        string uri = $"ws://{backendIP}:{backendPort}";
        Debug.Log($"🌐 Trying WebSocket connection: {uri}");

        websocket = new WebSocket(uri);

        websocket.OnOpen += () =>
        {
            Debug.Log("✅ WebSocket connected!");
            OnWebSocketConnected?.Invoke();
        };

        websocket.OnError += (e) =>
        {
            Debug.LogWarning("❌ WebSocket error: " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("🔌 WebSocket closed!");
        };

        websocket.OnMessage += (bytes) =>
        {
            string message = Encoding.UTF8.GetString(bytes);
            Debug.Log("📩 Received from backend: " + message);

            if (message.StartsWith("POSE:"))
            {
                string json = message.Substring(5);
                PositionData data = JsonUtility.FromJson<PositionData>(json);
                Vector3 offset = new Vector3(data.x, data.y, data.z);
                Vector3 worldPosition = Camera.main.transform.TransformPoint(offset);

                if (planeController != null)
                {
                    planeController.SpawnOrMovePlane(worldPosition);
                }
                else
                {
                    Debug.LogWarning("⚠️ No ARPlaneController assigned!");
                }
            }
        };

        await websocket.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null)
        {
            await websocket.Close();
        }
    }

    // 🔹 Optional: helper so other scripts can send messages
    public async void SendToBackend(string msg)
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.SendText(msg);
        }
    }
}
