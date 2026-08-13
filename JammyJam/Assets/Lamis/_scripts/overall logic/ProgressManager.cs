using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LevelProgressData
{
    public bool isUnlocked;
    public int stars; // 0 to 3
}

public class ProgressManager : MonoBehaviour
{
    private static ProgressManager instance;
    public static ProgressManager Instance
    {
        get
        {
            if (instance != null) return instance;
            instance = FindAnyObjectByType<ProgressManager>();
            if (instance == null)
            {
                GameObject go = new GameObject("ProgressManager");
                instance = go.AddComponent<ProgressManager>();
            }
            return instance;
        }
    }

    public static bool HasInstance => instance != null;

    public static ProgressManager InstanceOrNull => instance;

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

    [SerializeField] private int totalLevels = 4;
    public int TotalLevels => totalLevels;

    // Internal save data
    private List<LevelProgressData> levelData = new List<LevelProgressData>();
    private const string SaveKey = "GameProgress_v1";

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    private void InitializeData()
    {
        levelData.Clear();
        for (int i = 0; i < totalLevels; i++)
        {
            levelData.Add(new LevelProgressData
            {
                isUnlocked = (i == 0), // Only level 1 unlocked by default
                stars = 0
            });
        }
    }

    public void Save()
    {
        string json = JsonUtility.ToJson(new SaveWrapper { data = levelData });
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        if (PlayerPrefs.HasKey(SaveKey))
        {
            try
            {
                string json = PlayerPrefs.GetString(SaveKey);
                var wrapper = JsonUtility.FromJson<SaveWrapper>(json);
                levelData = wrapper.data;

                // Validate count matches current totalLevels
                while (levelData.Count < totalLevels)
                    levelData.Add(new LevelProgressData { isUnlocked = false, stars = 0 });
            }
            catch
            {
                Debug.LogWarning("[ProgressManager] Save file corrupted. Resetting.");
                InitializeData();
            }
        }
        else
        {
            InitializeData();
        }
    }

    public bool IsLevelUnlocked(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levelData.Count) return false;
        return levelData[levelIndex].isUnlocked;
    }

    public int GetLevelStars(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levelData.Count) return 0;
        return levelData[levelIndex].stars;
    }

    /// <summary>
    /// Call this when a level is completed. 
    /// Unlocks next level ONLY if at least 1 star was earned.
    /// </summary>
    public void CompleteLevel(int levelIndex, int earnedStars)
    {
        if (levelIndex < 0 || levelIndex >= levelData.Count) return;

        // Update stars (keep highest)
        int clampedStars = Mathf.Clamp(earnedStars, 0, 3);
        if (clampedStars > levelData[levelIndex].stars)
            levelData[levelIndex].stars = clampedStars;

        // Unlock next level only if player got at least 1 star
        if (clampedStars >= 1 && levelIndex + 1 < levelData.Count)
        {
            levelData[levelIndex + 1].isUnlocked = true;
        }

        Save();
    }

    public void ResetAllProgress()
    {
        InitializeData();
        Save();
    }

    // Wrapper needed because JsonUtility can't serialize List<T> directly at root
    [System.Serializable]
    private class SaveWrapper
    {
        public List<LevelProgressData> data;
    }
}