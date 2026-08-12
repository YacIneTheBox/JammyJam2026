using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
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

    [Header("UI & Setup")]
    public TMP_Text collectionText;
    public List<LevelInfo> levels = new List<LevelInfo>();

    private int totalInLevel = 3;
    private int collected = 0;
    private string currentSceneName;

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

    private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        collected = 0;
        currentSceneName = scene.name;

        LevelInfo match = levels.Find(l => l.sceneName == currentSceneName);
        totalInLevel = (match.totalItems > 0) ? match.totalItems : 3;

        // Auto-find the UI text in the newly loaded scene if it's missing
        FindUI();
        UpdateUI();
    }

    public void FindUI()
    {
        if (collectionText == null)
        {
            // Tries to find any TMP_Text component tagged or named properly, 
            // or you can search by finding the object containing "Text" in the new scene.
            TMP_Text foundText = FindObjectOfType<TMP_Text>();
            if (foundText != null)
            {
                collectionText = foundText;
            }
        }
    }

    public void CollectItem()
    {
        collected++;
        UpdateUI();

        Debug.Log($"[Collection] Level: {currentSceneName} | Collected: {collected} / {totalInLevel}");
        GameProgress.SaveLevelProgress(currentSceneName, collected, totalInLevel);

        if (collected >= totalInLevel)
        {
            Debug.Log($"[Collection] Level {currentSceneName} Completed!");
        }
    }

    private void UpdateUI()
    {
        // Safety fallback check just in case
        if (collectionText == null)
        {
            FindUI();
        }

        if (collectionText != null)
        {
            collectionText.text = $"{collected} / {totalInLevel}";
        }
    }
}