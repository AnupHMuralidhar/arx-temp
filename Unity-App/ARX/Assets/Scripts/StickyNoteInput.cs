using UnityEngine;
using TMPro;

public class StickyNoteInput : MonoBehaviour
{
    public TMP_InputField inputField;

    void OnEnable()
    {
        // When the note appears, focus the keyboard immediately
        if (inputField != null)
        {
            inputField.Select();
            inputField.ActivateInputField();
        }
    }
}