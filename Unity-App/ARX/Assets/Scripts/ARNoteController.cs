using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ARNoteController : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject notePrefab;
    public Vector3 noteScale = new Vector3(0.15f, 0.15f, 1f);
    public float spawnDistance = 0.5f;
    public float gestureCooldown = 1.5f;

    // Visible in Inspector for debugging
    [SerializeField] private List<GameObject> allNotes = new List<GameObject>();
    [SerializeField] private int currentIndex = -1;
    private float lastGestureTime = 0f;

    // ✌️ PEACE SIGN: Show/Hide the Stack
    public void ToggleStack()
    {
        Debug.Log($"⚡ [ToggleStack] Triggered. Time: {Time.time}");

        if (Time.time - lastGestureTime < gestureCooldown)
        {
            Debug.LogWarning($"⏳ Cooldown active. Wait {gestureCooldown - (Time.time - lastGestureTime)}s");
            return;
        }
        lastGestureTime = Time.time;

        // 1. Empty Stack?
        if (allNotes.Count == 0)
        {
            Debug.Log("📂 Stack is empty. Spawning the very first note...");
            SpawnNewNote();
            return;
        }

        GameObject currentNote = allNotes[currentIndex];

        if (currentNote.activeSelf)
        {
            // VISIBLE -> HIDE
            currentNote.SetActive(false);
            Debug.Log($"🙈 Stack Hidden. (Hiding Note #{currentIndex + 1})");
        }
        else
        {
            // HIDDEN -> SHOW
            TeleportToCamera(currentNote);
            currentNote.SetActive(true);
            Debug.Log($"👀 Stack Opened. Showing Note #{currentIndex + 1}");
        }
    }

    // 👍 THUMBS UP: Next Page or New Page
    public void TryGoNext()
    {
        Debug.Log($"⚡ [TryGoNext] Triggered.");

        if (Time.time - lastGestureTime < gestureCooldown) return;
        if (allNotes.Count == 0)
        {
            Debug.LogError("❌ No notes exist yet. Use Peace Sign first.");
            return;
        }

        lastGestureTime = Time.time;
        GameObject currentNote = allNotes[currentIndex];

        if (!currentNote.activeSelf)
        {
            Debug.Log("⚠️ Note was hidden. Opening stack instead.");
            ToggleStack();
            return;
        }

        // CASE A: Just flipping pages
        if (currentIndex < allNotes.Count - 1)
        {
            Debug.Log($"👉 Moving from Note {currentIndex + 1} to Note {currentIndex + 2}");
            SwitchToIndex(currentIndex + 1);
        }
        // CASE B: At the end. Need new note?
        else
        {
            // Check content
            var inputField = currentNote.GetComponentInChildren<TMP_InputField>();
            string content = inputField != null ? inputField.text : "";

            Debug.Log($"📝 Checking Note #{currentIndex + 1} content: '{content}'");

            if (!string.IsNullOrWhiteSpace(content) && content != $"Note #{currentIndex + 1}")
            {
                Debug.Log("✅ Note has text. Creating NEW note...");
                SpawnNewNote();
            }
            else
            {
                Debug.Log("⛔ Current note is empty (or default). Not spawning a new one.");
            }
        }
    }

    // 👎 THUMBS DOWN: Previous Page
    public void TryGoPrev()
    {
        Debug.Log($"⚡ [TryGoPrev] Triggered.");

        if (Time.time - lastGestureTime < gestureCooldown) return;
        if (allNotes.Count == 0) return;

        lastGestureTime = Time.time;

        if (!allNotes[currentIndex].activeSelf)
        {
            ToggleStack();
            return;
        }

        if (currentIndex > 0)
        {
            Debug.Log($"👈 Moving from Note {currentIndex + 1} back to Note {currentIndex}");
            SwitchToIndex(currentIndex - 1);
        }
        else
        {
            Debug.Log("⚠️ Already at Note #1. Cannot go back.");
        }
    }

    // --- HELPERS ---

    private void SpawnNewNote()
    {
        Debug.Log("🏗️ INSTANTIATING NEW NOTE PREFAB...");

        if (currentIndex >= 0 && currentIndex < allNotes.Count)
            allNotes[currentIndex].SetActive(false);

        Vector3 spawnPos = Camera.main.transform.position + (Camera.main.transform.forward * spawnDistance);
        GameObject newNote = Instantiate(notePrefab, spawnPos, Quaternion.identity);

        // Setup Scale
        newNote.transform.localScale = noteScale;
        foreach (Transform child in newNote.GetComponentsInChildren<Transform>())
        {
            if (child != newNote.transform) child.localScale = Vector3.one;
        }

        FaceUser(newNote);
        allNotes.Add(newNote);
        currentIndex = allNotes.Count - 1;

        // --- VISUAL DEBUG: AUTO-NAME THE NOTE ---
        var inputField = newNote.GetComponentInChildren<TMP_InputField>();
        if (inputField != null)
        {
            inputField.text = $"Note #{currentIndex + 1}"; // Set default text so you see it change
        }

        Debug.Log($"✅ SUCCESS: Created Note #{currentIndex + 1}. Total Notes: {allNotes.Count}");
    }

    private void SwitchToIndex(int newIndex)
    {
        allNotes[currentIndex].SetActive(false); // Hide Old
        currentIndex = newIndex;

        GameObject newNote = allNotes[currentIndex];
        TeleportToCamera(newNote);
        newNote.SetActive(true); // Show New
        Debug.Log($"🔄 Switched active note to Index {currentIndex}");
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

    public void ReceiveKeystroke(string key)
    {
        Debug.Log($"🎹 Key Received: {key} | Target: Note #{currentIndex + 1}");
        
        if (currentIndex < 0 || currentIndex >= allNotes.Count) return;
        GameObject activeNote = allNotes[currentIndex];
        if (!activeNote.activeSelf) return;

        var inputField = activeNote.GetComponentInChildren<TMP_InputField>();
        if (inputField != null)
        {
            if (key == "BACKSPACE")
            {
                if (inputField.text.Length > 0) inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
            }
            else inputField.text += key;

            inputField.ForceLabelUpdate();
            inputField.MoveTextEnd(false);
        }
    }
}