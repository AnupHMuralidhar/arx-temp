using UnityEngine;

public class ARPlaneController : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject planePrefab;                     // Assign prefab in Inspector
    public Vector3 planeScale = new Vector3(1.2f, 0.6f, 0.02f); 
    public Vector3 visualOffset = new Vector3(0.04f, 0f, 0.1f); 

    private GameObject currentPlane;
    private bool planeSpawned = false;

    // 🟢 CALLED BY WEBSOCKET: Accepts World Position directly
    public void SetDetectedPosition(Vector3 worldPosition)
    {
        // 🟢 LOCK: If we already spawned it, ignore new updates from the server.
        // This prevents the object from jumping around or following your face.
        if (planeSpawned) return; 

        // Apply the visual offset relative to where you are looking
        Vector3 offsetWorld = Camera.main.transform.TransformVector(visualOffset);
        Vector3 finalPosition = worldPosition + offsetWorld;

        Debug.Log("📍 Spawning Plane at World Pos: " + finalPosition);
        SpawnPlane(finalPosition);
    }

    private void SpawnPlane(Vector3 position)
    {
        if (planePrefab == null) return;

        // Instantiate only once
        if (currentPlane != null) Destroy(currentPlane);

        currentPlane = Instantiate(planePrefab, position, Quaternion.identity);

        // Apply Scaling to parent and reset children (prevents distortion)
        currentPlane.transform.localScale = planeScale;
        foreach (Transform child in currentPlane.GetComponentsInChildren<Transform>())
        {
            child.localScale = Vector3.one; 
        }

        // Face the user initially
        currentPlane.transform.LookAt(Camera.main.transform);
        currentPlane.transform.Rotate(0f, 180f, 0f); 

        currentPlane.SetActive(true);
        planeSpawned = true;
    }

    // --- UI HELPER METHODS (Kept from your original script) ---

    public void HidePlane()
    {
        if (currentPlane != null) Destroy(currentPlane);
        planeSpawned = false;
    }

    public void TogglePlane()
    {
        if (planeSpawned) HidePlane();
        // Note: We can't "Show" it again without a position from the AI, 
        // so Toggle only really works to hide it.
    }

    // Call this to force the AI to scan again (e.g. "Reset" button)
    public void ResetPlane()
    {
        HidePlane();
        Debug.Log("🔄 Plane reset, waiting for AI detection...");
    }
}