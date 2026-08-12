using UnityEngine;
using UnityEngine.UI;

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

    private void Start()
    {
        GameManager.Instance.NotifyGameSceneReady();
    }

    private void OnEnable()
    {
        GameManager.Instance.OnGameStateChanged += HandleStateChanged;
        AddListeners();
        HandleStateChanged(GameManager.Instance.CurrentState);
    }

    private void OnDisable()
    {
        GameManager.Instance.OnGameStateChanged -= HandleStateChanged;
        RemoveListeners();
    }

    private void AddListeners()
    {
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
            quitFromGameOverButton.onClick.RemoveListener(OnQuitToMenuClicked);

        if (nextLevelButton != null)
            nextLevelButton.onClick.RemoveListener(OnNextLevelClicked);

        if (menuFromLevelCompleteButton != null)
            menuFromLevelCompleteButton.onClick.RemoveListener(OnQuitToMenuClicked);
    }

    public void OnResumeClicked()
    {
        GameManager.Instance.ResumeGame();
    }

    public void OnRestartClicked()
    {
        GameManager.Instance.RestartLevel();
    }

    public void OnQuitToMenuClicked()
    {
        GameManager.Instance.QuitToMenu();
    }

    public void OnNextLevelClicked()
    {
        GameManager.Instance.ContinueAfterWin();
    }

    private void HandleStateChanged(GameState state)
    {
        SetPanel(hudPanel, state == GameState.Playing || state == GameState.Paused);
        SetPanel(pausePanel, state == GameState.Paused);
        SetPanel(gameOverPanel, state == GameState.GameOver);
        SetPanel(levelCompletePanel, state == GameState.LevelComplete);
    }

    private void SetPanel(GameObject panel, bool active)
    {
        if (panel != null && panel.activeSelf != active)
            panel.SetActive(active);
    }
}