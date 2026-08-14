using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [Tooltip("La source dédiée à la musique de fond (doit être en boucle)")]
    public AudioSource musicSource;
    
    [Tooltip("La source dédiée aux effets sonores")]
    public AudioSource sfxSource;

    private void Awake()
    {
        // Pattern Singleton avec persistance (comme ton GameManager)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // On détache l'objet de tout parent pour que DontDestroyOnLoad fonctionne
        transform.SetParent(null); 
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Joue une musique de fond. Ne relance pas la musique si c'est déjà la même qui tourne.
    /// </summary>
    public void PlayMusic(AudioClip musicClip)
    {
        if (musicClip == null) return;
        
        if (musicSource.clip == musicClip && musicSource.isPlaying) 
            return;

        musicSource.clip = musicClip;
        musicSource.Play();
    }

    /// <summary>
    /// Joue un effet sonore une seule fois (permet de superposer plusieurs sons).
    /// </summary>
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        
        sfxSource.PlayOneShot(clip, volume);
    }
}