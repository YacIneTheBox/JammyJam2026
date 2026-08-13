using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

public class LevelCollectionManager : MonoBehaviour
{
    public static LevelCollectionManager Instance;

    [System.Serializable]
    public struct LevelInfo
    {
        [Tooltip("1-based level number, matches GameManager.CurrentLevel")]
        public int levelIndex;
        public int totalItems;
    }

    [Header("Setup")]
    public List<LevelInfo> levels = new List<LevelInfo>();

    [Header("Debug Read Only")]
    [SerializeField] private int collected = 0;
    [SerializeField] private int totalInLevel = 0;

    public int Collected => collected;

    public int TotalInLevel => totalInLevel;

    public event Action<int, int> OnCollectionChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Only reset when entering the gameplay scene
        if (GameManager.Instance.gameSceneName != scene.name)
            return;

        collected = 0;

        int currentLevel = GameManager.Instance.CurrentLevel;

        LevelInfo match = levels.Find(l => l.levelIndex == currentLevel);
        totalInLevel = match.totalItems > 0 ? match.totalItems : 0;

        // Enforce "at least one paper" win rule only if the level has papers
        if (totalInLevel > 0)
            GameManager.Instance.customWinRequirement = () => collected >= 1;
        else
            GameManager.Instance.customWinRequirement = null;

        NotifyCollectionChanged();
    }

    public void CollectItem()
    {
        collected++;

        NotifyCollectionChanged();

        Debug.Log($"[Collection] Level {GameManager.Instance.CurrentLevel} | Collected: {collected} / {totalInLevel}");

        GameProgress.SaveLevelProgress("Level_" + GameManager.Instance.CurrentLevel, collected, totalInLevel);
    }

    private void NotifyCollectionChanged()
    {
        if (OnCollectionChanged != null)
            OnCollectionChanged.Invoke(collected, totalInLevel);
    }
}