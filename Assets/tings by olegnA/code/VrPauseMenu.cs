using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// VR Pause Menu — attach to a persistent GameObject in your game scene.
/// Pause is triggered by the Menu button on the left Quest controller.
/// The pause canvas appears in front of the player's head when opened.
/// </summary>
public class VRPauseMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject optionsPanel;

    [Header("Menu Canvas")]
    [SerializeField] private Transform menuCanvas;          // World Space canvas
    [SerializeField] private float distanceFromPlayer = 1.5f;
    [SerializeField] private float heightOffset = 0f;

    public static bool IsPaused { get; private set; } = false;

    // OpenXR / XR Input — Menu button on left Quest controller
    private UnityEngine.XR.InputDevice leftController;
    private bool previousMenuButtonState = false;

    private void Start()
    {
        // Start with menu hidden
        if (menuCanvas != null) menuCanvas.gameObject.SetActive(false);
    }

    private void Update()
    {
        PollMenuButton();
    }

    /// <summary>
    /// Polls the left controller Menu button (hamburger button on Quest).
    /// </summary>
    private void PollMenuButton()
    {
        if (!leftController.isValid)
        {
            leftController = GetLeftController();
            return;
        }

        bool menuButtonPressed = false;
        leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out menuButtonPressed);

        // Only trigger on button down (not held)
        if (menuButtonPressed && !previousMenuButtonState)
        {
            if (IsPaused) ResumeGame();
            else PauseGame();
        }

        previousMenuButtonState = menuButtonPressed;
    }

    private UnityEngine.XR.InputDevice GetLeftController()
    {
        var devices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(
            UnityEngine.XR.InputDeviceCharacteristics.Left |
            UnityEngine.XR.InputDeviceCharacteristics.Controller,
            devices);

        return devices.Count > 0 ? devices[0] : default;
    }

    // --- Core Actions ---

    public void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0f;

        PositionMenuInFrontOfPlayer();
        if (menuCanvas != null) menuCanvas.gameObject.SetActive(true);
        ShowPauseMenu();
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        if (menuCanvas != null) menuCanvas.gameObject.SetActive(false);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Replace "MainMenu" with the name of your main menu scene.
    /// </summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // --- Navigation ---

    public void ShowPauseMenu()
    {
        SetActivePanel(pauseMenuPanel);
    }

    public void ShowOptions()
    {
        SetActivePanel(optionsPanel);
    }

    public void BackFromOptions()
    {
        ShowPauseMenu();
    }

    // --- Helpers ---

    /// <summary>
    /// Snaps the menu canvas in front of wherever the player's head is pointing.
    /// </summary>
    private void PositionMenuInFrontOfPlayer()
    {
        if (menuCanvas == null) return;

        Camera headCamera = Camera.main;
        if (headCamera == null) return;

        Vector3 forward = headCamera.transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 pos = headCamera.transform.position + forward * distanceFromPlayer;
        pos.y += heightOffset;

        menuCanvas.position = pos;
        menuCanvas.rotation = Quaternion.LookRotation(forward);
    }

    private void SetActivePanel(GameObject panelToShow)
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);

        if (panelToShow != null) panelToShow.SetActive(true);
    }
}