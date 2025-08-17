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
    public string backendIP = "192.168.0.174";
    public int backendPort = 8765;
    public bool autoConnectOnStart = true;

    public ARPlaneController planeController;  // ✅ Reference to Plane Controller

    public Action OnWebSocketConnected;

    async void Start()
    {
        if (autoConnectOnStart)
        {
            string uri = $"ws://{backendIP}:{backendPort}";
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

                // ✅ Check for pose command
                if (message.StartsWith("POSE:"))
                {
                    string json = message.Substring(5); // Remove "POSE:"
                    PositionData data = JsonUtility.FromJson<PositionData>(json);

                    // Convert from local camera space to world space
                    Vector3 offset = new Vector3(data.x, data.y, data.z);
                    Vector3 worldPosition = Camera.main.transform.TransformPoint(offset);

                    planeController.SpawnOrMovePlane(worldPosition);
                }
            };

            await websocket.Connect();
        }
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (websocket != null)
        {
            websocket.DispatchMessageQueue();
        }
#endif
    }

    public async void SendToBackend(string msg)
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.SendText(msg);
        }
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null)
        {
            await websocket.Close();
        }
    }
}
