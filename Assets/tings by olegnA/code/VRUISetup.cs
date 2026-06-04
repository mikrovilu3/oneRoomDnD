using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Master VR UI Auto-Setup
/// 1. Fixes Event System for VR
/// 2. Fixes Canvases (Adds Tracked Raycaster & Auto-Assigns VR Headset Camera for tracking)
/// 3. Enables UI Interaction on all Ray Interactors
/// 4. (NEW) Automatically wires Hover & Click audio to every button.
/// </summary>
public class VRUISetup : MonoBehaviour
{
    [Header("UI Audio Cues (Optional)")]
    [Tooltip("Leave empty if you don't want auto-audio.")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private float audioVolume = 0.5f;

    private AudioSource uiAudioSource;

    private void Awake()
    {
        SetupAudioSource();

        FixEventSystem();
        FixCanvases();
        FixRayInteractors();

        if (hoverSound != null || clickSound != null)
            AutoWireAudioToButtons();
    }

    // ─── 1. Audio Setup ───────────────────────────────────────────────────────

    private void SetupAudioSource()
    {
        if (hoverSound == null && clickSound == null) return;

        // Create a dedicated 2D audio source just for UI sounds
        GameObject audioObj = new GameObject("UI_Audio_Source");
        audioObj.transform.SetParent(transform);
        uiAudioSource = audioObj.AddComponent<AudioSource>();
        uiAudioSource.spatialBlend = 0f; // Keep it 2D so it sounds like UI
        uiAudioSource.volume = audioVolume;
        uiAudioSource.playOnAwake = false;
    }

    private void AutoWireAudioToButtons()
    {
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);

        foreach (Button btn in allButtons)
        {
            // Add click sound
            if (clickSound != null)
            {
                btn.onClick.AddListener(() => uiAudioSource.PlayOneShot(clickSound));
            }

            // Add hover sound via EventTrigger
            if (hoverSound != null)
            {
                EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>();
                if (trigger == null) trigger = btn.gameObject.AddComponent<EventTrigger>();

                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerEnter;
                entry.callback.AddListener((data) => { uiAudioSource.PlayOneShot(hoverSound); });

                trigger.triggers.Add(entry);
            }
        }
        Debug.Log($"[VRUISetup] Wired audio to {allButtons.Length} buttons.");
    }

    // ─── 2. EventSystem ───────────────────────────────────────────────────────

    private void FixEventSystem()
    {
        EventSystem es = FindFirstObjectByType<EventSystem>();

        if (es == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            es = esGO.AddComponent<EventSystem>();
        }

        StandaloneInputModule flat = es.GetComponent<StandaloneInputModule>();
        if (flat != null) Destroy(flat);

        if (es.GetComponent<XRUIInputModule>() == null)
            es.gameObject.AddComponent<XRUIInputModule>();
    }

    // ─── 3. Canvases & Camera Tracking ────────────────────────────────────────

    private void FixCanvases()
    {
        Canvas[] targets = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Camera vrCamera = Camera.main;

        if (vrCamera == null)
        {
            Debug.LogError("[VRUISetup] No Main Camera found! UI tracking will fail. Please tag your VR Headset camera as 'MainCamera'.");
        }

        foreach (Canvas canvas in targets)
        {
            if (canvas.renderMode != RenderMode.WorldSpace) continue;

            // FIX: Assign the VR camera so controllers know where to point
            if (canvas.worldCamera == null && vrCamera != null)
            {
                canvas.worldCamera = vrCamera;
                Debug.Log($"[VRUISetup] Assigned VR Camera to '{canvas.name}'.");
            }

            var flatRC = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (flatRC != null) Destroy(flatRC);

            if (canvas.GetComponent<TrackedDeviceGraphicRaycaster>() == null)
                canvas.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
        }
    }

    // ─── 4. Ray Interactors ───────────────────────────────────────────────────

    private void FixRayInteractors()
    {
        XRRayInteractor[] interactors = FindObjectsByType<XRRayInteractor>(FindObjectsSortMode.None);

        foreach (XRRayInteractor ri in interactors)
        {
            if (!ri.enableUIInteraction)
            {
                ri.enableUIInteraction = true;
                Debug.Log($"[VRUISetup] Enabled UI interaction on '{ri.gameObject.name}'.");
            }
        }
    }
}