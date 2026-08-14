using UnityEngine;
using System.Collections;

public class ScreenTransitionManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Drag the FadeOverlay object (CanvasGroup) here")]
    [SerializeField] private CanvasGroup fadeOverlay;

    [Header("Audio References")]
    [Tooltip("Drag your main menu AudioSource here")]
    [SerializeField] private AudioSource bgmAudioSource;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.5f;

    private void Awake()
    {
        if (fadeOverlay == null)
        {
            Debug.LogError("[ScreenTransitionManager] Fade Overlay is missing in Inspector!");
            return;
        }

        // 1. Start completely black on launch
        fadeOverlay.alpha = 1f;
        fadeOverlay.blocksRaycasts = true;
    }

    private void Start()
    {
        // 2. Fade from Black -> Clear on start (100% -> 0%)
        StartCoroutine(FadeFromBlack());
    }

    /// <summary>
    /// Smoothly fades the overlay to 100% black FIRST, then executes the passed action (scene switch).
    /// </summary>
    public void FadeAndExecute(System.Action onFadeComplete)
    {
        if (fadeOverlay == null)
        {
            onFadeComplete?.Invoke();
            return;
        }

        StartCoroutine(FadeToBlackRoutine(onFadeComplete));
    }

    private IEnumerator FadeFromBlack()
    {
        float elapsed = 0f;
        float targetVolume = bgmAudioSource != null ? bgmAudioSource.volume : 1f;

        if (bgmAudioSource != null)
        {
            bgmAudioSource.volume = 0f;
        }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeDuration;

            // Alpha goes from 1.0 (Black) -> 0.0 (Clear)
            fadeOverlay.alpha = Mathf.Lerp(1f, 0f, t);

            // Audio fades in
            if (bgmAudioSource != null)
            {
                bgmAudioSource.volume = Mathf.Lerp(0f, targetVolume, t);
            }

            yield return null;
        }

        fadeOverlay.alpha = 0f;
        fadeOverlay.blocksRaycasts = false;
    }

    private IEnumerator FadeToBlackRoutine(System.Action onFadeComplete)
    {
        // Block interaction during transition
        fadeOverlay.blocksRaycasts = true;

        float elapsed = 0f;
        float startVolume = bgmAudioSource != null ? bgmAudioSource.volume : 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeDuration;

            // Alpha goes from 0.0 (Clear) -> 1.0 (Black)
            fadeOverlay.alpha = Mathf.Lerp(0f, 1f, t);

            // Audio fades out
            if (bgmAudioSource != null)
            {
                bgmAudioSource.volume = Mathf.Lerp(startVolume, 0f, t);
            }

            yield return null;
        }

        fadeOverlay.alpha = 1f;

        // Execute scene load action ONLY after screen is 100% black
        onFadeComplete?.Invoke();
    }
}