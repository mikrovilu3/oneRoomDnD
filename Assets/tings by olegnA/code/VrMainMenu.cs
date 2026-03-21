using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// VR Main Menu — attach to a GameObject in your menu scene.
/// The menu canvas should be World Space, positioned in front of the XR Origin spawn point.
/// </summary>
public class VRMainMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Menu Canvas")]
    [SerializeField] private Transform menuCanvas;       // The World Space canvas
    [SerializeField] private Transform xrOrigin;         // XR Origin transform
    [SerializeField] private float distanceFromPlayer = 2f;
    [SerializeField] private float heightOffset = 0f;    // Adjust if menu appears too high/low

    private void Start()
    {
        PositionMenuInFrontOfPlayer();
        ShowMainMenu();
    }

    /// <summary>
    /// Places the menu canvas in front of wherever the player is looking on start.
    /// </summary>
    private void PositionMenuInFrontOfPlayer()
    {
        if (menuCanvas == null || xrOrigin == null) return;

        Camera headCamera = Camera.main;
        if (headCamera == null) return;

        Vector3 forward = headCamera.transform.forward;
        forward.y = 0f; // Keep menu upright
        forward.Normalize();

        Vector3 menuPosition = xrOrigin.position + forward * distanceFromPlayer;
        menuPosition.y = xrOrigin.position.y + heightOffset;

        menuCanvas.position = menuPosition;
        menuCanvas.rotation = Quaternion.LookRotation(forward);
    }

    // --- Navigation ---

    public void ShowMainMenu()
    {
        SetActivePanel(mainMenuPanel);
    }

    public void ShowOptions()
    {
        SetActivePanel(optionsPanel);
    }

    public void ShowCredits()
    {
        SetActivePanel(creditsPanel);
    }

    // --- Core Actions ---

    /// <summary>
    /// Replace "GameScene" with the name of your actual game scene.
    /// </summary>
    public void PlayGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // --- Helpers ---

    private void SetActivePanel(GameObject panelToShow)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);

        if (panelToShow != null) panelToShow.SetActive(true);
    }
}