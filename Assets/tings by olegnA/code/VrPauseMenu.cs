using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// VR Pause & Death Menu — attach to a persistent GameObject in your game scene.
/// Pause is triggered by the Menu button on the left Quest controller.
/// Death is triggered via code by calling PlayerDied().
/// </summary>
public class VRPauseMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject deathMenuPanel; // NEW: Death Menu Panel

    [Header("Menu Canvas")]
    [SerializeField] private Transform menuCanvas;
    [SerializeField] private float distanceFromPlayer = 1.5f;
    [SerializeField] private float heightOffset = 0f;

    [Header("Pause Menu Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    [Header("Options Panel Buttons")]
    [SerializeField] private Button optionsBackButton;

    [Header("Death Menu Buttons")] // NEW: Buttons for when the player dies
    [SerializeField] private Button deathRestartButton;
    [SerializeField] private Button deathMainMenuButton;
    [SerializeField] private Button deathQuitButton;

    public static bool IsPaused { get; private set; } = false;
    public static bool IsDead { get; private set; } = false; // NEW: Tracks if player is dead

    private UnityEngine.XR.InputDevice leftController;
    private bool previousMenuButtonState = false;

    private void Start()
    {
        WireButtons();
        if (menuCanvas != null) menuCanvas.gameObject.SetActive(false);
    }

    private void WireButtons()
    {
        // Pause Menu
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (optionsButton != null) optionsButton.onClick.AddListener(ShowOptions);
        if (restartButton != null) restartButton.onClick.AddListener(RestartLevel);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);

        // Options
        if (optionsBackButton != null) optionsBackButton.onClick.AddListener(ShowPauseMenu);

        // Death Menu (Reuses the same core functions)
        if (deathRestartButton != null) deathRestartButton.onClick.AddListener(RestartLevel);
        if (deathMainMenuButton != null) deathMainMenuButton.onClick.AddListener(GoToMainMenu);
        if (deathQuitButton != null) deathQuitButton.onClick.AddListener(QuitGame);
    }

    private void Update()
    {
        PollMenuButton();
    }

    private void PollMenuButton()
    {
        // If the player is dead, completely disable the menu button so they can't unpause
        if (IsDead) return;

        if (!leftController.isValid)
        {
            leftController = GetLeftController();
            return;
        }

        leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out bool menuButtonPressed);

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
        if (IsDead) return; // Failsafe

        IsPaused = true;
        Time.timeScale = 0f;
        PositionMenuInFrontOfPlayer();
        if (menuCanvas != null) menuCanvas.gameObject.SetActive(true);
        ShowPauseMenu();
    }

    public void ResumeGame()
    {
        if (IsDead) return; // Cannot resume if dead

        IsPaused = false;
        Time.timeScale = 1f;
        if (menuCanvas != null) menuCanvas.gameObject.SetActive(false);
    }

    // NEW: Trigger this from your PlayerHealth script when health drops to 0
    public void PlayerDied()
    {
        IsDead = true;
        IsPaused = true;
        Time.timeScale = 0f;
        PositionMenuInFrontOfPlayer();
        if (menuCanvas != null) menuCanvas.gameObject.SetActive(true);

        SetActivePanel(deathMenuPanel);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        IsDead = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;
        IsDead = false;
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

    public void ShowPauseMenu() => SetActivePanel(pauseMenuPanel);
    public void ShowOptions() => SetActivePanel(optionsPanel);

    // --- Helpers ---

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
        if (deathMenuPanel != null) deathMenuPanel.SetActive(false); // Make sure this resets too

        if (panelToShow != null) panelToShow.SetActive(true);
    }
}