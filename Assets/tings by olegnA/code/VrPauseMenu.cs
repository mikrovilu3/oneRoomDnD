using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// VR Pause & Death Menu — Configurable input mapping via Inspector dropdowns.
/// </summary>
public class VRPauseMenu : MonoBehaviour
{
    public enum ControllerHand { Left, Right }
    public enum VRButtonChoice { MenuButton, PrimaryButton, SecondaryButton, ThumbstickClick }

    [Header("Input Configuration")]
    [SerializeField] private ControllerHand targetHand = ControllerHand.Left;
    [SerializeField] private VRButtonChoice pauseButton = VRButtonChoice.PrimaryButton; // Default to X/A button

    [Header("Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject deathMenuPanel;

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

    [Header("Death Menu Buttons")]
    [SerializeField] private Button deathRestartButton;
    [SerializeField] private Button deathMainMenuButton;
    [SerializeField] private Button deathQuitButton;

    public static bool IsPaused { get; private set; } = false;
    public static bool IsDead { get; private set; } = false;

    private UnityEngine.XR.InputDevice targetDevice;
    private bool previousButtonState = false;

    private void Start()
    {
        WireButtons();
        if (menuCanvas != null) menuCanvas.gameObject.SetActive(false);
    }

    private void WireButtons()
    {
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (optionsButton != null) optionsButton.onClick.AddListener(ShowOptions);
        if (restartButton != null) restartButton.onClick.AddListener(RestartLevel);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);

        if (optionsBackButton != null) optionsBackButton.onClick.AddListener(ShowPauseMenu);

        if (deathRestartButton != null) deathRestartButton.onClick.AddListener(RestartLevel);
        if (deathMainMenuButton != null) deathMainMenuButton.onClick.AddListener(GoToMainMenu);
        if (deathQuitButton != null) deathQuitButton.onClick.AddListener(QuitGame);
    }

    private void Update()
    {
        PollPauseInput();
    }

    private void PollPauseInput()
    {
        if (IsDead) return;

        if (!targetDevice.isValid)
        {
            targetDevice = GetTargetController();
            return;
        }

        // Figure out which hardware feature usage context to query
        UnityEngine.XR.InputFeatureUsage<bool> buttonFeature = UnityEngine.XR.CommonUsages.primaryButton;

        switch (pauseButton)
        {
            case VRButtonChoice.MenuButton:
                buttonFeature = UnityEngine.XR.CommonUsages.menuButton;
                break;
            case VRButtonChoice.PrimaryButton:
                buttonFeature = UnityEngine.XR.CommonUsages.primaryButton; // X on Left, A on Right
                break;
            case VRButtonChoice.SecondaryButton:
                buttonFeature = UnityEngine.XR.CommonUsages.secondaryButton; // Y on Left, B on Right
                break;
            case VRButtonChoice.ThumbstickClick:
                buttonFeature = UnityEngine.XR.CommonUsages.primary2DAxisClick; // Joystick click
                break;
        }

        targetDevice.TryGetFeatureValue(buttonFeature, out bool buttonPressed);

        // Check for a clean press frame (down transition)
        if (buttonPressed && !previousButtonState)
        {
            if (IsPaused) ResumeGame();
            else PauseGame();
        }

        previousButtonState = buttonPressed;
    }

    private UnityEngine.XR.InputDevice GetTargetController()
    {
        var characteristics = UnityEngine.XR.InputDeviceCharacteristics.Controller;

        if (targetHand == ControllerHand.Left)
            characteristics |= UnityEngine.XR.InputDeviceCharacteristics.Left;
        else
            characteristics |= UnityEngine.XR.InputDeviceCharacteristics.Right;

        var devices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(characteristics, devices);

        return devices.Count > 0 ? devices[0] : default;
    }

    // --- Core Actions ---

    public void PauseGame()
    {
        if (IsDead) return;

        IsPaused = true;
        Time.timeScale = 0f;
        PositionMenuInFrontOfPlayer();
        if (menuCanvas != null) menuCanvas.gameObject.SetActive(true);
        ShowPauseMenu();
    }

    public void ResumeGame()
    {
        if (IsDead) return;

        IsPaused = false;
        Time.timeScale = 1f;
        if (menuCanvas != null) menuCanvas.gameObject.SetActive(false);
    }

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
        if (deathMenuPanel != null) deathMenuPanel.SetActive(false);

        if (panelToShow != null) panelToShow.SetActive(true);
    }
}