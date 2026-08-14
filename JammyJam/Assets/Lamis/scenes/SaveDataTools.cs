using UnityEditor;
using UnityEngine;

public static class SaveDataTools
{
    [MenuItem("Tools/Clear All Save Data")]
    public static void ClearAllSaveData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[SaveDataTools] All PlayerPrefs cleared.");
    }
}
