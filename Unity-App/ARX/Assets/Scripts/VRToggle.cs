using UnityEngine;
using UnityEngine.UI;

public class VRToggle : MonoBehaviour
{
    [Header("Camera Setup")]
    public Camera arCamera;
    public RenderTexture vrRenderTexture; // Drag your AR_RenderTexture here

    [Header("UI Containers")]
    public GameObject standardCanvas; // Drag 'Canvas_Standard' here
    public GameObject vrCanvas;       // Drag 'Canvas_VR' here

    private bool isVRMode = false;

    void Start()
    {
        // Ensure we start in Standard Mode
        SetMode(false);
    }

    public void ToggleVRMode()
    {
        isVRMode = !isVRMode;
        SetMode(isVRMode);
    }

    void SetMode(bool vrEnabled)
    {
        if (vrEnabled)
        {
            // --- ENABLE VR MODE ---
            // 1. Redirect Camera to Texture
            arCamera.targetTexture = vrRenderTexture;
            
            // 2. Show Split Screen UI
            vrCanvas.SetActive(true);
            
            // 3. Hide Standard UI
            standardCanvas.SetActive(false);
            
            Debug.Log("🥽 VR Mode Enabled");
        }
        else
        {
            // --- ENABLE STANDARD MODE ---
            // 1. Redirect Camera back to Screen (null means screen)
            arCamera.targetTexture = null;
            
            // 2. Hide Split Screen UI
            vrCanvas.SetActive(false);
            
            // 3. Show Standard UI
            standardCanvas.SetActive(true);
            
            Debug.Log("📱 Standard Mode Enabled");
        }
    }
}