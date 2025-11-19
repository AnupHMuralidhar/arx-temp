using UnityEngine;

public class ARPlaneController : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject planePrefab;                     // Assign prefab in Inspector
    public Vector3 planeScale = new Vector3(1.2f, 0.6f, 0.02f); // Bigger rectangle (1.2m x 0.6m)
    public Vector3 visualOffset = new Vector3(0.04f, 0f, 0.1f); // 4cm right, 10cm forward

    private GameObject currentPlane;
    private Vector3 lastDetectedPosition;
    private bool planeSpawned = false;

    public void SetDetectedPosition(Vector3 localCameraPos)
    {
        lastDetectedPosition = Camera.main.transform.TransformPoint(localCameraPos);
        Debug.Log("📍 Detected position (world-space): " + lastDetectedPosition);

        SpawnAtDetectedPosition();
    }

    public void SpawnAtDetectedPosition()
    {
        if (planePrefab == null)
        {
            Debug.LogWarning("⚠️ Plane prefab not assigned!");
            return;
        }

        Vector3 offsetWorld = Camera.main.transform.TransformVector(visualOffset);
        Vector3 spawnPosition = lastDetectedPosition + offsetWorld;

        if (currentPlane != null)
            Destroy(currentPlane);

        currentPlane = Instantiate(planePrefab, spawnPosition, Quaternion.identity);

        // ✅ Apply scale to the prefab root AND all children (handles nested meshes)
        currentPlane.transform.localScale = planeScale;
        foreach (Transform child in currentPlane.GetComponentsInChildren<Transform>())
        {
            child.localScale = Vector3.one; // reset child scale so parent scale works
        }

        // Face camera
        currentPlane.transform.LookAt(Camera.main.transform);
        currentPlane.transform.Rotate(0f, 180f, 0f);

        currentPlane.SetActive(true);
        planeSpawned = true;

        Debug.Log($"✅ Plane spawned at {spawnPosition} with scale {planeScale}");
    }

    public void SpawnPlane()
    {
        if (!planeSpawned)
            SpawnAtDetectedPosition();
    }

    public void HidePlane()
    {
        if (!planeSpawned)
            return;

        if (currentPlane != null)
            Destroy(currentPlane);

        currentPlane = null;
        planeSpawned = false;

        Debug.Log("🛑 Plane hidden/destroyed");
    }

    public void TogglePlane()
    {
        if (planeSpawned)
            HidePlane();
        else
            SpawnPlane();
    }
}
