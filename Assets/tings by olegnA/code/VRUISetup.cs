using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using UnityEngine.XR.Interaction.Toolkit.UI;

/// <summary>
/// Drop this on any persistent GameObject in your scene.
/// It will automatically configure all VR UI requirements at startup:
///   1. Swap StandaloneInputModule → XRUIInputModule on the EventSystem
///   2. Swap GraphicRaycaster → TrackedDeviceGraphicRaycaster on every World Space Canvas
///   3. Enable UI interaction on every XRRayInteractor in the scene
/// No Inspector wiring needed.
/// </summary>
public class VRUISetup : MonoBehaviour
{
    [Header("Optional overrides — leave empty to auto-find")]
    [Tooltip("Leave empty — all World Space Canvases are found automatically.")]
    [SerializeField] private Canvas[] manualCanvases;

    private void Awake()
    {
        FixEventSystem();
        FixCanvases();
        FixRayInteractors();
    }

    // ─── 1. EventSystem ───────────────────────────────────────────────────────

    private void FixEventSystem()
    {
        EventSystem es = FindObjectOfType<EventSystem>();

        if (es == null)
        {
            // Create one if missing
            GameObject esGO = new GameObject("EventSystem");
            es = esGO.AddComponent<EventSystem>();
            Debug.Log("[VRUISetup] Created missing EventSystem.");
        }

        // Remove flat input module if present
        StandaloneInputModule flat = es.GetComponent<StandaloneInputModule>();
        if (flat != null)
        {
            Destroy(flat);
            Debug.Log("[VRUISetup] Removed StandaloneInputModule.");
        }

        // Add XR module if missing
        if (es.GetComponent<XRUIInputModule>() == null)
        {
            es.gameObject.AddComponent<XRUIInputModule>();
            Debug.Log("[VRUISetup] Added XRUIInputModule.");
        }
    }

    // ─── 2. Canvases ─────────────────────────────────────────────────────────

    private void FixCanvases()
    {
        Canvas[] targets = (manualCanvases != null && manualCanvases.Length > 0)
            ? manualCanvases
            : FindObjectsOfType<Canvas>();

        foreach (Canvas canvas in targets)
        {
            // Only World Space canvases need this
            if (canvas.renderMode != RenderMode.WorldSpace) continue;

            // Remove flat raycaster
            var flatRC = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (flatRC != null)
            {
                Destroy(flatRC);
                Debug.Log($"[VRUISetup] Removed GraphicRaycaster from '{canvas.name}'.");
            }

            // Add tracked raycaster if missing
            if (canvas.GetComponent<TrackedDeviceGraphicRaycaster>() == null)
            {
                canvas.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
                Debug.Log($"[VRUISetup] Added TrackedDeviceGraphicRaycaster to '{canvas.name}'.");
            }
        }
    }

    // ─── 3. Ray Interactors ───────────────────────────────────────────────────

    private void FixRayInteractors()
    {
        UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor[] interactors = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>();

        if (interactors.Length == 0)
        {
            Debug.LogWarning("[VRUISetup] No XRRayInteractors found in the scene! " +
                             "Make sure your XR Origin has Left/Right Ray Interactor children.");
            return;
        }

        foreach (UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor ri in interactors)
        {
            if (!ri.enableUIInteraction)
            {
                ri.enableUIInteraction = true;
                Debug.Log($"[VRUISetup] Enabled UI interaction on '{ri.gameObject.name}'.");
            }
        }
    }
}