using UnityEngine;

public static class GameProgress
{
    public static void SaveLevelProgress(string sceneName, int collected, int total)
    {
        int currentBest = GetLevelCollected(sceneName);
        if (collected > currentBest)
        {
            PlayerPrefs.SetInt(sceneName + "_Collected", collected);
            PlayerPrefs.Save();
            
            Debug.Log($"[GameProgress] New High Score! Level: {sceneName} | Collected: {collected} / {total}");
        }
    }

    public static int GetLevelCollected(string sceneName)
    {
        return PlayerPrefs.GetInt(sceneName + "_Collected", 0);
    }
}