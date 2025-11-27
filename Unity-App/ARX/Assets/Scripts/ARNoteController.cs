using UnityEngine;

public class ARNoteController : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject notePrefab;                       // Drag Blue StickyNote Prefab here
    public Vector3 noteScale = new Vector3(0.15f, 0.15f, 1f); // 15cm size
    public float spawnDistance = 0.5f;                  // 50cm in front of face

    // 🟢 CALLED BY WEBSOCKET CLIENT (On Gesture)
    public void SpawnNote()
    {
        Debug.Log($"🚀 ARNoteController: SpawnNote() TRIGGERED on GameObject '{gameObject.name}'");

        // --- CHECK PREFAB ---
        if (notePrefab == null)
        {
            Debug.LogError("❌ CRITICAL FAIL: 'notePrefab' is NULL! Did you forget to drag the Blue Prefab into the slot?");
            return;
        }
        Debug.Log($"✅ Prefab found: {notePrefab.name}");

        // --- CHECK CAMERA ---
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("❌ CRITICAL FAIL: No Main Camera found in scene (Tag 'MainCamera' missing?)");
            return;
        }
        Debug.Log($"📷 Camera found at: {mainCam.transform.position}");

        // --- 1. CALCULATE POSITION ---
        Vector3 camPos = mainCam.transform.position;
        Vector3 camFwd = mainCam.transform.forward;
        Vector3 spawnPos = camPos + (camFwd * spawnDistance);

        Debug.Log($"🧮 Math: Camera {camPos} + (Forward {camFwd} * {spawnDistance}) = {spawnPos}");

        // --- 2. INSTANTIATE ---
        GameObject currentNote = Instantiate(notePrefab, spawnPos, Quaternion.identity);

        if (currentNote == null)
        {
            Debug.LogError("❌ INSTANTIATE FAILED: The object was not created.");
            return;
        }
        Debug.Log($"✨ Object Instantiated! Name: {currentNote.name}");

        // --- 3. APPLY SCALING ---
        Debug.Log($"📏 Scaling parent to: {noteScale}");
        currentNote.transform.localScale = noteScale;
        
        int childCount = 0;
        foreach (Transform child in currentNote.GetComponentsInChildren<Transform>())
        {
            if (child != currentNote.transform)
            {
                child.localScale = Vector3.one; 
                childCount++;
            }
        }
        Debug.Log($"👶 Reset scale for {childCount} children.");

        // --- 4. ROTATE ---
        Debug.Log("👀 Rotating to face camera...");
        currentNote.transform.LookAt(mainCam.transform);
        currentNote.transform.Rotate(0f, 180f, 0f); // Fix backward quad
        Debug.Log($"🔄 Final Rotation: {currentNote.transform.rotation.eulerAngles}");

        // --- 5. ACTIVATE ---
        currentNote.SetActive(true);
        Debug.Log("✅ Note Active. SUCCESS.");
    }
}