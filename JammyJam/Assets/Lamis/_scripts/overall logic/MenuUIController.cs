using UnityEngine;
using UnityEngine.UI;

public class MenuUIController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject levelSelectPanel;
    public GameObject settingsPanel;

    [Header("Main Menu Buttons")]
    public Button playButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("Level Select Buttons")]
    public Button levelBackButton;

    [Header("Settings Buttons")]
    public Button settingsBackButton;

    private GameManager cachedGameManager;

    private void Start()
    {
        // Safe during normal startup.
        if (GameManager.Instance != null)
            GameManager.Instance.NotifyMenuSceneReady();

        SubscribeToGameManager();
    }

    private void OnEnable()
    {
        SubscribeToGameManager();
        AddListeners();

        if (cachedGameManager != null)
            HandleStateChanged(cachedGameManager.CurrentState);
    }

    private void OnDisable()
    {
        UnsubscribeFromGameManager();
        RemoveListeners();
    }

    private void SubscribeToGameManager()
    {
        if (cachedGameManager != null)
            return;

        // IMPORTANT: InstanceOrNull does NOT create a new GameManager.
        cachedGameManager = GameManager.InstanceOrNull;

        if (cachedGameManager != null)
            cachedGameManager.OnGameStateChanged += HandleStateChanged;
    }

    private void UnsubscribeFromGameManager()
    {
        if (cachedGameManager != null)
            cachedGameManager.OnGameStateChanged -= HandleStateChanged;

        cachedGameManager = null;
    }

    private void AddListeners()
    {
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        if (levelBackButton != null)
            levelBackButton.onClick.AddListener(OnLevelBackClicked);

        if (settingsBackButton != null)
            settingsBackButton.onClick.AddListener(OnSettingsBackClicked);
    }

    private void RemoveListeners()
    {
        if (playButton != null)
            playButton.onClick.RemoveListener(OnPlayClicked);

        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(OnSettingsClicked);

        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuitClicked);

        if (levelBackButton != null)
            levelBackButton.onClick.RemoveListener(OnLevelBackClicked);

        if (settingsBackButton != null)
            settingsBackButton.onClick.RemoveListener(OnSettingsBackClicked);
    }

    public void OnPlayClicked()
    {
        GameManager.Instance.OpenLevelSelect();
    }

    public void OnSettingsClicked()
    {
        GameManager.Instance.OpenSettings();
    }

    public void OnQuitClicked()
    {
        GameManager.Instance.QuitApplication();
    }

    public void OnLevelBackClicked()
    {
        GameManager.Instance.GoToMainMenu();
    }

    public void OnSettingsBackClicked()
    {
        GameManager.Instance.CloseSettings();
    }

    private void HandleStateChanged(GameState state)
    {
        SetPanel(mainMenuPanel, state == GameState.MainMenu);
        SetPanel(levelSelectPanel, state == GameState.LevelSelect);
        SetPanel(settingsPanel, state == GameState.Settings);
    }

    private void SetPanel(GameObject panel, bool active)
    {
        if (panel != null && panel.activeSelf != active)
            panel.SetActive(active);
    }
}