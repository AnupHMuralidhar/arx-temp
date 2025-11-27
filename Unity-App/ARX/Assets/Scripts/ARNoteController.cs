using UnityEngine;

public class ARNoteController : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject notePrefab;                       
    public Vector3 noteScale = new Vector3(0.15f, 0.15f, 1f); 
    public float spawnDistance = 0.5f;                  

    [Header("Settings")]
    public float toggleCooldown = 2.0f; // Wait 2 seconds before allowing another toggle

    // 🔒 STATIC: Shared by ALL instances of this script in the scene
    private static GameObject currentNoteInstance;
    private static float lastToggleTime = 0f;

    // 🟢 CALLED BY WEBSOCKET CLIENT
    public void ToggleNote()
    {
        // 1. Cooldown Check: Prevent "Machine Gun" toggling
        if (Time.time - lastToggleTime < toggleCooldown)
        {
            return; // Too soon! Ignore this gesture.
        }

        // Update the timer
        lastToggleTime = Time.time;

        // 2. Logic: Create or Toggle
        if (currentNoteInstance == null)
        {
            SpawnNewNote();
        }
        else
        {
            if (currentNoteInstance.activeSelf)
            {
                // Visible -> Hide
                currentNoteInstance.SetActive(false);
                Debug.Log("🙈 Note Hidden");
            }
            else
            {
                // Hidden -> Show & Move to Face
                TeleportToCamera(currentNoteInstance);
                currentNoteInstance.SetActive(true);
                Debug.Log("👀 Note Resummoned");
            }
        }
    }

    private void SpawnNewNote()
    {
        if (notePrefab == null) 
        {
            Debug.LogError("❌ ARNoteController: Prefab missing on " + gameObject.name);
            return;
        }

        Vector3 spawnPos = Camera.main.transform.position + (Camera.main.transform.forward * spawnDistance);
        currentNoteInstance = Instantiate(notePrefab, spawnPos, Quaternion.identity);

        currentNoteInstance.transform.localScale = noteScale;
        
        // Fix children scale
        foreach (Transform child in currentNoteInstance.GetComponentsInChildren<Transform>())
        {
            if (child != currentNoteInstance.transform) child.localScale = Vector3.one; 
        }

        FaceUser(currentNoteInstance);
        
        Debug.Log("📝 New Note Created");
    }
    // 🟢 CALLED BY WEBSOCKET CLIENT
    public void ReceiveKeystroke(string key)
    {
        Debug.Log($"🎹 Processing Keystroke: '{key}'");

        if (currentNoteInstance == null)
        {
            Debug.LogError("❌ FAIL: Note Instance is NULL. (Did you spawn it first?)");
            return;
        }

        if (!currentNoteInstance.activeSelf)
        {
            Debug.LogWarning("⚠️ Note exists but is hidden. Ignoring key.");
            return;
        }

        // Try to find InputField
        var inputField = currentNoteInstance.GetComponentInChildren<TMPro.TMP_InputField>();
        
        // Fallback: Try finding just a Text Mesh Pro object (if you didn't use InputField)
        var simpleText = currentNoteInstance.GetComponentInChildren<TMPro.TextMeshProUGUI>();

        if (inputField != null)
        {
            if (key == "BACKSPACE")
            {
                if (inputField.text.Length > 0)
                    inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
            }
            else
            {
                inputField.text += key;
            }
            
            // Force Unity to redraw the text immediately
            inputField.ForceLabelUpdate(); 
            Debug.Log($"✅ Text Updated! Current Content: '{inputField.text}'");
        }
        else if (simpleText != null)
        {
            // Fallback logic for normal text
            if (key == "BACKSPACE")
                simpleText.text = simpleText.text.Substring(0, simpleText.text.Length - 1);
            else
                simpleText.text += key;
                
            Debug.Log($"✅ Simple Text Updated! Content: '{simpleText.text}'");
        }
        else
        {
            Debug.LogError("❌ CRITICAL: Could not find 'TMP_InputField' OR 'TextMeshProUGUI' on the note!");
            Debug.LogError("👉 check your Prefab structure.");
        }
    }
    private void TeleportToCamera(GameObject obj)
    {
        obj.transform.position = Camera.main.transform.position + (Camera.main.transform.forward * spawnDistance);
        FaceUser(obj);
    }

    private void FaceUser(GameObject obj)
    {
        obj.transform.LookAt(Camera.main.transform);
        obj.transform.Rotate(0f, 180f, 0f); 
    }
}