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
    [Header("Video Control")]
   public VirtualMonitor targetMonitor;
    public Action OnWebSocketConnected;
    public ARNoteController noteController;

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
            if (message.StartsWith("GESTURE:"))
            {
                
                string action = message.Substring(8); // Remove "GESTURE:"
                
                // Call the new function to control the monitor
                if (action == "NOTE") SpawnStickyNote();
                HandleMonitorControl(action);
            }
        };

        await websocket.Connect();
    }
    void SpawnStickyNote()
    {
        Debug.Log("🔍 Attempting to Spawn Note...");

        // 1. If we don't have the controller reference, HUNT FOR IT.
        if (noteController == null)
        {
            // Try to find it on this object first
            noteController = GetComponent<ARNoteController>();

            // If not found, search the WHOLE SCENE (The "Ghost Object" fix)
            if (noteController == null)
            {
                noteController = FindFirstObjectByType<ARNoteController>();
            }
        }

        // 2. Now try to execute
        if (noteController != null)
        {
            Debug.Log("✅ Found Controller on: " + noteController.gameObject.name);
            noteController.SpawnNote();
        }
        else
        {
            Debug.LogError("❌ CRITICAL: 'ARNoteController' script is missing from the scene! Add it to the AR Session Origin.");
        }
    }    void HandleMonitorControl(string action)
    {
        // If not assigned in Inspector, try to find it in the scene automatically
        if (targetMonitor == null)
        {
            targetMonitor = FindFirstObjectByType<VirtualMonitor>();
        }

        if (targetMonitor == null) 
        {
            Debug.LogWarning("⚠️ No VirtualMonitor found in scene to control!");
            return;
        }

        if (action == "PAUSE")
        {
            targetMonitor.PauseVideo();
        }
        else if (action == "PLAY")
        {
            targetMonitor.PlayVideo();
        }
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
