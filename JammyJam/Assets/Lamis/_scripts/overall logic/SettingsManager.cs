using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    private static SettingsManager instance;

    public static SettingsManager Instance
    {
        get
        {
            if (instance != null)
                return instance;

            instance = FindAnyObjectByType<SettingsManager>();

            if (instance == null)
            {
                GameObject go = new GameObject("SettingsManager");
                instance = go.AddComponent<SettingsManager>();
            }

            return instance;
        }
    }

    public float MasterVolume { get; private set; } = 1f;

    private const string VolumeKey = "MasterVolume";

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

        Load();
        Apply();
    }

    public void SetVolume(float volume)
    {
        MasterVolume = Mathf.Clamp01(volume);
        Apply();
        Save();
    }

    private void Load()
    {
        MasterVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);
    }

    private void Apply()
    {
        AudioListener.volume = MasterVolume;
    }

    private void Save()
    {
        PlayerPrefs.SetFloat(VolumeKey, MasterVolume);
        PlayerPrefs.Save();
    }
}
