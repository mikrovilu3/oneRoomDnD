using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// VR Keybinding Reminder — shows Quest controller button mappings in the pause menu.
/// Defaults are Quest controller buttons; edit in the Inspector to match your game.
/// Attach to a GameObject inside your pause menu canvas.
/// </summary>
public class VRKeybindingReminder : MonoBehaviour
{
    [System.Serializable]
    public class KeybindEntry
    {
        public string actionName;   // e.g. "Jump"
        public string keyLabel;     // e.g. "A Button"
    }

    [Header("UI")]
    [SerializeField] private Transform contentParent;       // Vertical Layout Group parent
    [SerializeField] private GameObject bindingRowPrefab;   // Prefab: two TMP_Text children

    [Header("Controller Bindings")]
    [SerializeField]
    private List<KeybindEntry> controllerBindings = new List<KeybindEntry>
    {
        new KeybindEntry { actionName = "Move",           keyLabel = "Left Thumbstick" },
        new KeybindEntry { actionName = "Turn",           keyLabel = "Right Thumbstick" },
        new KeybindEntry { actionName = "Jump",           keyLabel = "A Button (Right)" },
        new KeybindEntry { actionName = "Interact",       keyLabel = "Right Trigger" },
        new KeybindEntry { actionName = "Grab",           keyLabel = "Right Grip" },
        new KeybindEntry { actionName = "Sprint",         keyLabel = "Left Thumbstick Click" },
        new KeybindEntry { actionName = "Pause / Menu",   keyLabel = "Menu Button (Left)" },
        new KeybindEntry { actionName = "Secondary",      keyLabel = "B Button (Right)" },
    };

    private void Start()
    {
        PopulateBindings(controllerBindings);
    }

    public void PopulateBindings(List<KeybindEntry> entries)
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        foreach (var entry in entries)
            AddBinding(entry.actionName, entry.keyLabel);
    }

    public void AddBinding(string actionName, string keyLabel)
    {
        if (bindingRowPrefab == null || contentParent == null) return;

        GameObject row = Instantiate(bindingRowPrefab, contentParent);

        TMP_Text[] labels = row.GetComponentsInChildren<TMP_Text>();
        if (labels.Length >= 2)
        {
            labels[0].text = actionName;
            labels[1].text = keyLabel;
        }
        else
        {
            Debug.LogWarning("[VRKeybindingReminder] Row prefab needs at least 2 TMP_Text children.");
        }
    }
}