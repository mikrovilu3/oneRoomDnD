using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// VR Pause Menu — attach to a persistent GameObject in your game scene.
/// Pause is triggered by the Menu button on the left Quest controller.
/// Assign buttons in the Inspector; they are wired up automatically in Start().
/// </summary>
public class VRPauseMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject optionsPanel;

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

    public static bool IsPaused { get; private set; } = false;

    private UnityEngine.XR.InputDevice leftController;
    private bool previousMenuButtonState = false;

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
    }

    private void Update()
    {
        PollMenuButton();
    }

    private void PollMenuButton()
    {
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

    public void ShowPauseMenu() => SetActivePanel(pauseMenuPanel);
    public void ShowOptions() => SetActivePanel(optionsPanel);
    public void BackFromOptions() => ShowPauseMenu();

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
        if (panelToShow != null) panelToShow.SetActive(true);
    }
}