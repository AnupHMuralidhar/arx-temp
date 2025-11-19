using UnityEngine;

public class ARFloatingButtonController : MonoBehaviour
{
    public Camera arCamera;
    public float distanceFromCamera = 0.5f;

    void Start()
    {
        if (arCamera == null)
            arCamera = Camera.main;
    }

    void Update()
    {
        if (arCamera != null)
        {
            // Keep button in front of camera
            transform.position = arCamera.transform.position + arCamera.transform.forward * distanceFromCamera;
            transform.rotation = Quaternion.LookRotation(transform.position - arCamera.transform.position);
        }
    }

    public void OnButtonPressed()
    {
        Debug.Log("🌟 Floating AR Button Pressed!");
        var planeController = Object.FindFirstObjectByType<ARPlaneController>();
        if (planeController != null)
            planeController.TogglePlane();
    }
}
