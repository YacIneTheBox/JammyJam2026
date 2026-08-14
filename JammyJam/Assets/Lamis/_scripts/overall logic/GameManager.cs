using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public enum GameState
{
    MainMenu,
    LevelSelect,
    Settings,
    Playing,
    Paused,
    GameOver,
    LevelComplete,
    Cutscene
}

public enum LossReason
{
    None,
    CameraCaught,
    ScannerColorMismatch,
    ScannerEmptySlot,
    SlotReachedEndEmpty,
    Electrocuted,
    LeftBehind,
    NotEnoughPaper,
    External,
    OutOfCameraSight
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

    public static bool HasInstance => instance != null;

    public static GameManager InstanceOrNull => instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticReferences()
    {
        instance = null;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    [Header("Scene Names")]
    public string menuSceneName = "MenuScene";
    public string gameSceneName = "GameScene";

    [Tooltip("If false, menus and gameplay are expected to be inside the same scene.")]
    public bool useSceneLoading = true;

    [Header("Level Scenes")]
    [Tooltip("Scene name for each level, in order. Element 0 = Level 1. Leave empty to use Game Scene Name for all levels.")]
    public List<string> levelSceneNames = new List<string>();

    [Header("Cutscenes")]
    [Tooltip("Optional cutscene scene name for each level. Element 0 = Level 1. Leave empty for no cutscene.")]
    public List<string> levelCutsceneNames = new List<string>();

    [SerializeField] private int pendingLevelAfterCutscene = 0;

    [SerializeField] private int lastEarnedStars = 0;
    public int LastEarnedStars => lastEarnedStars;

    [Header("Debug Read Only")]
    [SerializeField] private GameState currentState = GameState.MainMenu;
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private LossReason lastLossReason = LossReason.None;

    public GameState CurrentState => currentState;
    public int CurrentLevel => currentLevel;
    public LossReason LastLossReason => lastLossReason;

    public event Action<GameState> OnGameStateChanged;
    public event Action<LossReason> OnLossTriggered;

    // This will be assigned later by the collection system.
    // For now, if null, winning is allowed.
    public Func<bool> customWinRequirement;

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
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return;

        if (keyboard.escapeKey.wasPressedThisFrame)
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

    public string GetSceneNameForLevel(int levelIndex)
    {
        int i = levelIndex - 1;

        if (levelSceneNames != null && i >= 0 && i < levelSceneNames.Count && !string.IsNullOrEmpty(levelSceneNames[i]))
            return levelSceneNames[i];

        return gameSceneName;
    }

    public string GetCutsceneNameForLevel(int levelIndex)
    {
        int i = levelIndex - 1;

        if (levelCutsceneNames != null && i >= 0 && i < levelCutsceneNames.Count && !string.IsNullOrEmpty(levelCutsceneNames[i]))
            return levelCutsceneNames[i];

        return null;
    }

    public void FinishCutscene()
    {
        if (currentState != GameState.Cutscene)
            return;

        int level = pendingLevelAfterCutscene > 0 ? pendingLevelAfterCutscene : 1;
        pendingLevelAfterCutscene = 0;

        currentLevel = level;

        SetState(GameState.Playing);
        LoadLevelSceneIfDifferent();
    }

    public bool IsGameplayScene(string sceneName)
    {
        if (sceneName == gameSceneName)
            return true;

        return levelSceneNames != null && levelSceneNames.Contains(sceneName);
    }

    public void GoToMainMenu()
    {
        currentLevel = 0;
        customWinRequirement = null;
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

        customWinRequirement = null;

        string cutscene = GetCutsceneNameForLevel(currentLevel);

        if (!string.IsNullOrEmpty(cutscene))
        {
            pendingLevelAfterCutscene = currentLevel;
            SetState(GameState.Cutscene);
            TryLoadScene(cutscene);
        }
        else
        {
            SetState(GameState.Playing);
            LoadLevelSceneIfDifferent();
        }
    }

    public void RestartLevel()
    {
        customWinRequirement = null;
        // On SUPPRIME SetState(GameState.Playing); pour garder le jeu en pause
        ReloadLevelScene();
    }

    private void LoadLevelSceneIfDifferent()
    {
        if (!useSceneLoading)
            return;

        string target = GetSceneNameForLevel(currentLevel);

        if (SceneManager.GetActiveScene().name == target)
            return;

        TryLoadScene(target);
    }

    private void ReloadLevelScene()
    {
        if (!useSceneLoading)
            return;

        TryLoadScene(GetSceneNameForLevel(currentLevel));
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

    public void TriggerLoss(LossReason reason)
    {
        if (currentState != GameState.Playing)
            return;

        lastLossReason = reason;

        Debug.LogWarning($"[GameManager] Loss triggered: {reason}");

        if (OnLossTriggered != null)
            OnLossTriggered.Invoke(reason);

        SetState(GameState.GameOver);


    }

    public void TriggerWin()
    {
        if (currentState != GameState.Playing)
            return;

        int stars = 1;

        if (LevelCollectionManager.Instance != null)
            stars = LevelCollectionManager.Instance.CalculateStars();

        lastEarnedStars = stars;

        // Nothing collected -> no stars -> not a win
        if (stars <= 0)
        {
            TriggerLoss(LossReason.NotEnoughPaper);
            return;
        }

        if (ProgressManager.Instance != null)
            ProgressManager.Instance.CompleteLevel(currentLevel - 1, stars);

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

    public bool CanWin()
    {
        if (customWinRequirement == null)
            return true;

        return customWinRequirement.Invoke();
    }

    public string GetCurrentLossText()
    {
        return GetLossText(lastLossReason);
    }

    public static string GetLossText(LossReason reason)
    {
        switch (reason)
        {
            case LossReason.CameraCaught:
                return "You were caught by a security camera.";

            case LossReason.ScannerColorMismatch:
                return "You entered the scanner with the wrong color.";

            case LossReason.OutOfCameraSight: // <-- AJOUTE CECI
                return "stays in camera sight";

            case LossReason.ScannerEmptySlot:
                return "Your slot passed the scanner empty.";

            case LossReason.SlotReachedEndEmpty:
                return "Your slot reached the end without you.";

            case LossReason.Electrocuted:
                return "You were electrocuted.";

            case LossReason.LeftBehind:
                return "You were left behind.";

            case LossReason.NotEnoughPaper:
                return "You need to collect at least one paper.";

            case LossReason.External:
                return "You failed.";

            default:
                return "You failed.";
        }
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

    public void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

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
        if (currentState == GameState.Paused ||
            currentState == GameState.GameOver ||
            currentState == GameState.LevelComplete)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsGameplayScene(scene.name))
        {
            // On ajoute GameOver et LevelComplete ici pour forcer le passage en Playing une fois la scène chargée
            if (currentState == GameState.MainMenu ||
                currentState == GameState.LevelSelect ||
                currentState == GameState.Settings ||
                currentState == GameState.GameOver || 
                currentState == GameState.LevelComplete)
            {
                SetState(GameState.Playing);
            }

            EnsureTimeScale();
        }
        else if (scene.name == menuSceneName)
        {
            if (currentState == GameState.Playing ||
                currentState == GameState.Paused ||
                currentState == GameState.GameOver ||
                currentState == GameState.LevelComplete)
            {
                SetState(GameState.MainMenu);
            }
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
        int sceneCount = SceneManager.sceneCountInBuildSettings;

        for (int i = 0; i < sceneCount; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string nameOnly = Path.GetFileNameWithoutExtension(scenePath);

            if (nameOnly == sceneName)
            {
                SceneManager.LoadScene(sceneName);
                return true;
            }
        }

        Debug.LogError($"[GameManager] Scene '{sceneName}' is not in Build Settings. Add it to File > Build Settings.");
        return false;
    }
}
