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

    [Header("Plane & Button")]
    public ARPlaneController planeController;
    public GameObject floatingButtonPrefab;  // Assign ARFloatingButton prefab in Inspector
    public float buttonDistanceFromCamera = 0.5f; // Distance in meters

    private GameObject spawnedButton;
    private Camera arCamera;

    public Action OnWebSocketConnected;

    // 🔗 Connect to backend when UDP discovery finds IP
    public async void Connect(string backendIP)
    {
        string uri = $"ws://{backendIP}:{backendPort}";
        Debug.Log($"🌐 Trying WebSocket connection: {uri}");

        websocket = new WebSocket(uri);

        websocket.OnOpen += () =>
        {
            Debug.Log("✅ WebSocket connected!");
            OnWebSocketConnected?.Invoke();

            // Cache main AR camera
            if (arCamera == null)
                arCamera = Camera.main;

            // Spawn floating AR button only once
            if (floatingButtonPrefab != null && spawnedButton == null && arCamera != null)
            {
                spawnedButton = Instantiate(floatingButtonPrefab);

                // Position button in front of camera
                spawnedButton.transform.position = arCamera.transform.position + arCamera.transform.forward * buttonDistanceFromCamera;
                spawnedButton.transform.rotation = Quaternion.LookRotation(spawnedButton.transform.position - arCamera.transform.position);

                spawnedButton.SetActive(true);
            }
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
                Vector3 worldPosition = arCamera.transform.TransformPoint(offset);

                if (planeController != null)
                    planeController.SetDetectedPosition(worldPosition);
                else
                    Debug.LogWarning("⚠️ No ARPlaneController assigned!");
            }
        };

        await websocket.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();

        // Keep the floating button in front of the camera
        if (spawnedButton != null && arCamera != null)
        {
            spawnedButton.transform.position = arCamera.transform.position + arCamera.transform.forward * buttonDistanceFromCamera;
            spawnedButton.transform.rotation = Quaternion.LookRotation(spawnedButton.transform.position - arCamera.transform.position);
        }
#endif
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null)
            await websocket.Close();
    }

    // 🔹 Optional helper to send messages to backend
    public async void SendToBackend(string msg)
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
            await websocket.SendText(msg);
    }
}
