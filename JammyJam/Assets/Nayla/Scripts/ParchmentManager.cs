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
        public string sceneName;
        public int totalItems;
    }

    [Header("Setup")]
    [Tooltip("Optional manual totals per scene. If a scene is missing or total is 0, parchments are counted automatically.")]
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
        // React to ANY gameplay scene (GameScene, Level1, Level2, ...), not just one name
        if (GameManager.HasInstance && !GameManager.Instance.IsGameplayScene(scene.name))
            return;

        // 1) Reset the count for the new level
        collected = 0;

        // 2) Find the total: manual entry first, otherwise count parchments in the scene
        LevelInfo match = levels.Find(l => l.sceneName == scene.name);

        if (match.totalItems > 0)
        {
            totalInLevel = match.totalItems;
        }
        else
        {
            totalInLevel = FindObjectsByType<Parchment>(FindObjectsSortMode.None).Length;
        }

        // 3) Enforce "at least one paper" win rule
        if (GameManager.HasInstance)
        {
            if (totalInLevel > 0)
                GameManager.Instance.customWinRequirement = () => collected >= 1;
            else
                GameManager.Instance.customWinRequirement = null;
        }

        // 4) Update the HUD (0 / 13, etc.)
        NotifyCollectionChanged();
    }

    public void CollectItem()
    {
        collected++;

        NotifyCollectionChanged();

        Debug.Log($"[Collection] Collected: {collected} / {totalInLevel}");
    }

    public int CalculateStars()
    {
        if (totalInLevel <= 0 || collected <= 0)
            return 0;

        if (collected >= totalInLevel)
            return 3;

        int third = Mathf.Max(1, Mathf.RoundToInt(totalInLevel / 3f));
        int twoThirds = third * 2;

        if (collected >= twoThirds)
            return 2;

        return 1;
    }

    private void NotifyCollectionChanged()
    {
        if (OnCollectionChanged != null)
            OnCollectionChanged.Invoke(collected, totalInLevel);
    }
}