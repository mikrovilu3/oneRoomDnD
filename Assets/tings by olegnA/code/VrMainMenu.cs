using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// VR Main Menu — attach to a GameObject in your menu scene.
/// Assign buttons in the Inspector; they are wired up automatically in Start().
/// </summary>
public class VRMainMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Menu Canvas")]
    [SerializeField] private Transform menuCanvas;
    [SerializeField] private Transform xrOrigin;
    [SerializeField] private float distanceFromPlayer = 2f;
    [SerializeField] private float heightOffset = 0f;

    [Header("Main Menu Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;

    [Header("Options Panel Buttons")]
    [SerializeField] private Button optionsBackButton;

    [Header("Credits Panel Buttons")]
    [SerializeField] private Button creditsBackButton;

    [Header("Scene Name")]
    [SerializeField] private string gameSceneName = "GameScene";

    private void Start()
    {
        WireButtons();
        PositionMenuInFrontOfPlayer();
        ShowMainMenu();
    }

    private void WireButtons()
    {
        if (playButton != null) playButton.onClick.AddListener(PlayGame);
        if (optionsButton != null) optionsButton.onClick.AddListener(ShowOptions);
        if (creditsButton != null) creditsButton.onClick.AddListener(ShowCredits);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
        if (optionsBackButton != null) optionsBackButton.onClick.AddListener(ShowMainMenu);
        if (creditsBackButton != null) creditsBackButton.onClick.AddListener(ShowMainMenu);
    }

    private void PositionMenuInFrontOfPlayer()
    {
        if (menuCanvas == null || xrOrigin == null) return;

        Camera headCamera = Camera.main;
        if (headCamera == null) return;

        Vector3 forward = headCamera.transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 menuPosition = xrOrigin.position + forward * distanceFromPlayer;
        menuPosition.y = xrOrigin.position.y + heightOffset;

        menuCanvas.position = menuPosition;
        menuCanvas.rotation = Quaternion.LookRotation(forward);
    }

    // --- Navigation ---

    public void ShowMainMenu() => SetActivePanel(mainMenuPanel);
    public void ShowOptions() => SetActivePanel(optionsPanel);
    public void ShowCredits() => SetActivePanel(creditsPanel);

    // --- Core Actions ---

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
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