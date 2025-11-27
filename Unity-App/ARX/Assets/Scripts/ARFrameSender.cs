using UnityEngine;

public class ARFrameSender : MonoBehaviour
{
    public RenderTexture arTexture; // Assign your AR_RenderTexture here
    public WebSocketClient wsClient; // Assign your WebSocketClient
    
    private Texture2D sendTexture;
    private float timer = 0;
    public float scanInterval = 0.5f; // Send 2 frames per second (save bandwidth)

    void Start()
    {
        sendTexture = new Texture2D(arTexture.width, arTexture.height, TextureFormat.RGB24, false);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= scanInterval)
        {
            SendFrame();
            timer = 0;
        }
    }

    void SendFrame()
    {
        // Remember the currently active render texture
        RenderTexture.active = arTexture;
        
        // Read pixels from the Render Texture
        sendTexture.ReadPixels(new Rect(0, 0, arTexture.width, arTexture.height), 0, 0);
        sendTexture.Apply();
        
        // Restore active render texture
        RenderTexture.active = null;

        // Encode and send
        byte[] bytes = sendTexture.EncodeToJPG(50); // Quality 50 is fast
        string base64 = System.Convert.ToBase64String(bytes);
        
        if(wsClient != null)
            wsClient.SendToBackend("IMG:" + base64);
    }
}