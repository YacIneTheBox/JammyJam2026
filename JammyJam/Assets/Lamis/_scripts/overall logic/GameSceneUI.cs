using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameSceneUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject hudPanel;
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject levelCompletePanel;

    [Header("Pause Buttons")]
    public Button resumeButton;
    public Button restartFromPauseButton;
    public Button quitFromPauseButton;

    [Header("Game Over Buttons")]
    public Button retryFromGameOverButton;
    public Button quitFromGameOverButton;

    [Header("Level Complete Buttons")]
    public Button nextLevelButton;
    public Button menuFromLevelCompleteButton;

    [Header("Text")]
    public TMP_Text lossReasonText;

    [Header("HUD")]
    public TMP_Text paperCounterText;
    public Slider suspicionMeter;

    [Header("Pattern Alert")]
    public GameObject patternAlertPanel;
    public TMP_Text patternAlertText;

    [Tooltip("How long the alert banner stays visible.")]
    public float alertDisplayTime = 2.5f;

    [Tooltip("Optional sound played when the alert fires.")]
    public AudioSource alertSound;

    private PatternManager cachedPatternManager;
    private Coroutine alertCoroutine;

    private GameManager cachedGameManager;

    private void Awake()
    {
        AddListeners();
    }

    private void Start()
    {
        ValidateReferences();

        // This can safely create/find the GameManager during normal startup.
        if (GameManager.Instance != null)
            GameManager.Instance.NotifyGameSceneReady();

        SubscribeToGameManager();
        UpdateNextLevelButton();

        cachedPatternManager = PatternManager.Instance;

        if (cachedPatternManager != null)
            cachedPatternManager.OnPatternAlert += HandlePatternAlert;
    }

    private void Update()
    {
        if (suspicionMeter != null && SuspicionManager.HasInstance)
            suspicionMeter.value = SuspicionManager.Instance.Suspicion;
    }

    private void OnEnable()
    {
        SubscribeToGameManager();
        if (LevelCollectionManager.Instance != null)
        {
            LevelCollectionManager.Instance.OnCollectionChanged += HandleCollectionChanged;
            HandleCollectionChanged(LevelCollectionManager.Instance.Collected, LevelCollectionManager.Instance.TotalInLevel);
        }
    }

    private void OnDisable()
    {
        UnsubscribeFromGameManager();

        if (LevelCollectionManager.Instance != null)
            LevelCollectionManager.Instance.OnCollectionChanged -= HandleCollectionChanged;
    }

    private void OnDestroy()
    {
        RemoveListeners();

        if (cachedPatternManager != null)
            cachedPatternManager.OnPatternAlert -= HandlePatternAlert;
    }

    private void SubscribeToGameManager()
    {
        if (cachedGameManager != null)
            return;

        // IMPORTANT: InstanceOrNull does NOT create a new GameManager.
        cachedGameManager = GameManager.InstanceOrNull;

        if (cachedGameManager != null)
        {
            cachedGameManager.OnGameStateChanged += HandleStateChanged;
            cachedGameManager.OnLossTriggered += HandleLossTriggered;
            HandleStateChanged(cachedGameManager.CurrentState);
        }
    }

    private void UnsubscribeFromGameManager()
    {
        if (cachedGameManager != null)
        {
            cachedGameManager.OnGameStateChanged -= HandleStateChanged;
            cachedGameManager.OnLossTriggered -= HandleLossTriggered;
        }

        cachedGameManager = null;
    }

    private void ValidateReferences()
    {
        if (resumeButton == null)
            Debug.LogWarning("[GameSceneUI] Resume Button is not assigned.");

        if (restartFromPauseButton == null)
            Debug.LogWarning("[GameSceneUI] Restart From Pause Button is not assigned.");

        if (quitFromPauseButton == null)
            Debug.LogWarning("[GameSceneUI] Quit From Pause Button is not assigned.");

        if (retryFromGameOverButton == null)
            Debug.LogWarning("[GameSceneUI] Retry From Game Over Button is not assigned.");

        if (quitFromGameOverButton == null)
            Debug.LogWarning("[GameSceneUI] Quit From Game Over Button is not assigned.");

        if (nextLevelButton == null)
            Debug.LogWarning("[GameSceneUI] Next Level Button is not assigned.");

        if (menuFromLevelCompleteButton == null)
            Debug.LogWarning("[GameSceneUI] Menu From Level Complete Button is not assigned.");

        if (lossReasonText == null)
            Debug.LogWarning("[GameSceneUI] Loss Reason Text is not assigned.");
    }

    private void AddListeners()
    {
        Debug.Log("[GameSceneUI] Adding button listeners.");

        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);

        if (restartFromPauseButton != null)
            restartFromPauseButton.onClick.AddListener(OnRestartClicked);

        if (quitFromPauseButton != null)
            quitFromPauseButton.onClick.AddListener(OnQuitToMenuClicked);

        if (retryFromGameOverButton != null)
            retryFromGameOverButton.onClick.AddListener(OnRestartClicked);

        if (quitFromGameOverButton != null)
            quitFromGameOverButton.onClick.AddListener(OnQuitToMenuClicked);

        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);

        if (menuFromLevelCompleteButton != null)
            menuFromLevelCompleteButton.onClick.AddListener(OnQuitToMenuClicked);
    }

    private void RemoveListeners()
    {
        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(OnResumeClicked);

        if (restartFromPauseButton != null)
            restartFromPauseButton.onClick.RemoveListener(OnRestartClicked);

        if (quitFromPauseButton != null)
            quitFromPauseButton.onClick.RemoveListener(OnQuitToMenuClicked);

        if (retryFromGameOverButton != null)
            retryFromGameOverButton.onClick.RemoveListener(OnRestartClicked);

        if (quitFromGameOverButton != null)
            quitFromGameOverButton.onClick.RemoveListener(OnQuitFromGameOverClicked);

        if (nextLevelButton != null)
            nextLevelButton.onClick.RemoveListener(OnNextLevelClicked);

        if (menuFromLevelCompleteButton != null)
            menuFromLevelCompleteButton.onClick.RemoveListener(OnQuitToMenuClicked);
    }

    public void OnResumeClicked()
    {
        Debug.Log($"[GameSceneUI] Resume clicked. Current state: {GameManager.Instance.CurrentState}");
        GameManager.Instance.ResumeGame();
    }

    public void OnRestartClicked()
    {
        Debug.Log($"[GameSceneUI] Restart clicked. Current state: {GameManager.Instance.CurrentState}");
        GameManager.Instance.RestartLevel();
    }

    public void OnQuitToMenuClicked()
    {
        Debug.Log($"[GameSceneUI] Quit to menu clicked. Current state: {GameManager.Instance.CurrentState}");
        GameManager.Instance.GoToMainMenu();
    }

    public void OnQuitFromGameOverClicked()
    {
        Debug.Log($"[GameSceneUI] Quit from Game Over clicked. Current state: {GameManager.Instance.CurrentState}");
        GameManager.Instance.GoToMainMenu();
    }

    public void OnNextLevelClicked()
    {
        Debug.Log($"[GameSceneUI] Next Level clicked. Current state: {GameManager.Instance.CurrentState}");
        GameManager.Instance.ContinueAfterWin();
    }

    private void HandlePatternAlert()
    {
        if (patternAlertPanel != null)
            patternAlertPanel.SetActive(true);

        if (patternAlertText != null)
            patternAlertText.text = "PATTERN CHANGE INCOMING!";

        if (alertSound != null)
            alertSound.Play();

        if (alertCoroutine != null)
            StopCoroutine(alertCoroutine);

        alertCoroutine = StartCoroutine(HideAlertAfterDelay());
    }

    private IEnumerator HideAlertAfterDelay()
    {
        yield return new WaitForSeconds(alertDisplayTime);

        if (patternAlertPanel != null)
            patternAlertPanel.SetActive(false);

        alertCoroutine = null;
    }

    private void HandleLossTriggered(LossReason reason)
    {
        if (lossReasonText != null)
            lossReasonText.text = GameManager.GetLossText(reason);
    }

    private void HandleStateChanged(GameState state)
    {
        Debug.Log($"[GameSceneUI] Game state changed: {state}");

        SetPanel(hudPanel, state == GameState.Playing || state == GameState.Paused);
        SetPanel(pausePanel, state == GameState.Paused);
        SetPanel(gameOverPanel, state == GameState.GameOver);
        SetPanel(levelCompletePanel, state == GameState.LevelComplete);

        if (state == GameState.GameOver && lossReasonText != null)
        {
            if (cachedGameManager != null)
                lossReasonText.text = cachedGameManager.GetCurrentLossText();
            else
                lossReasonText.text = GameManager.GetLossText(LossReason.External);
        }

        if (state == GameState.LevelComplete)
            UpdateNextLevelButton();
    }

    private void HandleCollectionChanged(int collected, int total)
    {
        if (paperCounterText != null)
            paperCounterText.text = $"{collected} / {total}";
    }

    private void UpdateNextLevelButton()
    {
        if (nextLevelButton == null)
            return;

        int totalLevels = ProgressManager.HasInstance ? ProgressManager.Instance.TotalLevels : 4;

        int currentLevel;

        if (cachedGameManager != null)
            currentLevel = cachedGameManager.CurrentLevel;
        else if (GameManager.HasInstance)
            currentLevel = GameManager.Instance.CurrentLevel;
        else
            currentLevel = 1;

        bool hasNextLevel = currentLevel < totalLevels;

        nextLevelButton.gameObject.SetActive(hasNextLevel);
    }

    private void SetPanel(GameObject panel, bool active)
    {
        if (panel != null && panel.activeSelf != active)
            panel.SetActive(active);
    }
}