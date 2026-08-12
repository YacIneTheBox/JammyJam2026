using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public enum GameState
{
    MainMenu,
    LevelSelect,
    Settings,
    Playing,
    Paused,
    GameOver,
    LevelComplete
}

public enum LossReason
{
    CameraCaught,
    ScannerColorMismatch,
    ScannerEmptySlot,
    SlotReachedEndEmpty,
    Electrocuted,
    LeftBehind,
    External
}

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance != null)
                return instance;

            instance = FindAnyObjectByType<GameManager>();

            if (instance == null)
            {
                GameObject go = new GameObject("GameManager");
                instance = go.AddComponent<GameManager>();
            }

            return instance;
        }
    }

    [Header("Scene Names")]
    public string menuSceneName = "MenuScene";
    public string gameSceneName = "GameScene";

    [Tooltip("If false, the game will not load scenes. Useful for testing all menus inside one scene.")]
    public bool useSceneLoading = true;

    [Header("Debug Read Only")]
    [SerializeField] private GameState currentState = GameState.MainMenu;
    [SerializeField] private int currentLevel = 1;

    public GameState CurrentState => currentState;
    public int CurrentLevel => currentLevel;

    public event Action<GameState> OnGameStateChanged;

    private GameState stateBeforeSettings = GameState.MainMenu;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        if (transform.parent != null)
            transform.SetParent(null);

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current == null)
            return;

        if (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (currentState == GameState.Playing)
            {
                PauseGame();
            }
            else if (currentState == GameState.Paused)
            {
                ResumeGame();
            }
            else if (currentState == GameState.Settings)
            {
                CloseSettings();
            }
        }
    }

    public void GoToMainMenu()
    {
        currentLevel = 0;
        SetState(GameState.MainMenu);
        LoadMenuSceneIfDifferent();
    }

    public void OpenLevelSelect()
    {
        SetState(GameState.LevelSelect);
        LoadMenuSceneIfDifferent();
    }

    public void OpenSettings()
    {
        if (currentState != GameState.Settings)
            stateBeforeSettings = currentState;

        SetState(GameState.Settings);
        LoadMenuSceneIfDifferent();
    }

    public void CloseSettings()
    {
        SetState(stateBeforeSettings);
    }

    public void StartLevel(int levelIndex)
    {
        int totalLevels = ProgressManager.Instance != null ? ProgressManager.Instance.TotalLevels : 4;
        currentLevel = Mathf.Clamp(levelIndex, 1, totalLevels);

        SetState(GameState.Playing);
        LoadGameSceneIfDifferent();
    }

    public void RestartLevel()
    {
        SetState(GameState.Playing);
        ReloadGameScene();
    }

    public void PauseGame()
    {
        if (currentState != GameState.Playing)
            return;

        SetState(GameState.Paused);
    }

    public void ResumeGame()
    {
        if (currentState != GameState.Paused)
            return;

        SetState(GameState.Playing);
    }

    public void QuitToMenu()
    {
        GoToMainMenu();
    }

    public void TriggerLoss(LossReason reason)
    {
        if (currentState != GameState.Playing)
            return;

        Debug.LogWarning($"[GameManager] Loss triggered: {reason}");
        SetState(GameState.GameOver);
    }

    // Add an optional parameter for stars (default 1 for backward compatibility)
    public void TriggerWin(int earnedStars = 1)
    {
        if (currentState != GameState.Playing)
            return;

        // Use 0-based index internally
        ProgressManager.Instance.CompleteLevel(currentLevel - 1, earnedStars);

        SetState(GameState.LevelComplete);
    }

    public void ContinueAfterWin()
    {
        if (currentState != GameState.LevelComplete)
            return;

        int totalLevels = ProgressManager.Instance != null ? ProgressManager.Instance.TotalLevels : 4;

        if (currentLevel < totalLevels)
            StartLevel(currentLevel + 1);
        else
            GoToMainMenu();
    }

    public void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void NotifyGameSceneReady()
    {
        if (currentState == GameState.MainMenu ||
            currentState == GameState.LevelSelect ||
            currentState == GameState.Settings)
        {
            SetState(GameState.Playing);
        }

        EnsureTimeScale();
    }

    public void NotifyMenuSceneReady()
    {
        if (currentState == GameState.Playing ||
            currentState == GameState.Paused ||
            currentState == GameState.GameOver ||
            currentState == GameState.LevelComplete)
        {
            SetState(GameState.MainMenu);
        }
    }

    // Convenience methods for UnityEvents or teammate systems.
    public void TriggerLossCamera() => TriggerLoss(LossReason.CameraCaught);
    public void TriggerLossElectricity() => TriggerLoss(LossReason.Electrocuted);
    public void TriggerLossScannerMismatch() => TriggerLoss(LossReason.ScannerColorMismatch);
    public void TriggerLossScannerEmptySlot() => TriggerLoss(LossReason.ScannerEmptySlot);
    public void TriggerLossSlotEndEmpty() => TriggerLoss(LossReason.SlotReachedEndEmpty);
    public void TriggerLossLeftBehind() => TriggerLoss(LossReason.LeftBehind);
    public void TriggerLossExternal() => TriggerLoss(LossReason.External);

    private void SetState(GameState newState)
    {
        if (currentState == newState)
        {
            EnsureTimeScale();
            return;
        }

        currentState = newState;
        EnsureTimeScale();

        if (OnGameStateChanged != null)
            OnGameStateChanged.Invoke(currentState);
    }

    private void EnsureTimeScale()
    {
        if (currentState == GameState.Paused || currentState == GameState.GameOver)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == gameSceneName)
        {
            EnsureTimeScale();
        }
    }

    private void LoadMenuSceneIfDifferent()
    {
        if (!useSceneLoading)
            return;

        if (SceneManager.GetActiveScene().name == menuSceneName)
            return;

        TryLoadScene(menuSceneName);
    }

    private void LoadGameSceneIfDifferent()
    {
        if (!useSceneLoading)
            return;

        if (SceneManager.GetActiveScene().name == gameSceneName)
            return;

        TryLoadScene(gameSceneName);
    }

    private void ReloadGameScene()
    {
        if (!useSceneLoading)
            return;

        TryLoadScene(gameSceneName);
    }

    private bool TryLoadScene(string sceneName)
    {
        // Check if scene exists in Build Settings by name
        bool sceneExists = false;
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName)
            {
                sceneExists = true;
                break;
            }
        }

        if (!sceneExists)
        {
            Debug.LogError($"[GameManager] Scene '{sceneName}' is not in Build Settings. Add it to File > Build Settings.");
            return false;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        return true;
    }
}
