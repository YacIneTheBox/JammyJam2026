using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PatternChangeFX : MonoBehaviour
{
    [Header("Fog Target (assign one or both)")]
    [Tooltip("World-space fog sprite.")]
    public SpriteRenderer fogSprite;

    [Tooltip("UI fog image. Remember: Raycast Target = false!")]
    public Image fogImage;

    [Header("Fade Settings")]
    public float fadeInTime = 0.5f;
    public float fadeOutTime = 0.8f;

    [Range(0f, 1f)]
    [Tooltip("Peak opacity. 0.8 = 80%.")]
    public float maxOpacity = 0.8f;

    [Tooltip("Optional small delay after the color change before fading out.")]
    public float fadeOutDelay = 0f;

    private Color spriteBaseColor = Color.white;
    private Color imageBaseColor = Color.white;
    private float currentAlpha;
    private Coroutine fadeCoroutine;
    private PatternManager cachedPatternManager;

    private void Awake()
    {
        if (fogSprite != null)
            spriteBaseColor = fogSprite.color;

        if (fogImage != null)
            imageBaseColor = fogImage.color;

        SetAlpha(0f);
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void Start()
    {
        // In case PatternManager appeared after OnEnable
        Subscribe();
    }

    private void OnDisable()
    {
        if (cachedPatternManager != null)
        {
            cachedPatternManager.OnPatternAlert -= HandleAlert;
            cachedPatternManager.OnPatternStateChanged -= HandlePatternChanged;
        }

        cachedPatternManager = null;
    }

    private void Subscribe()
    {
        if (cachedPatternManager != null)
            return;

        cachedPatternManager = PatternManager.Instance;

        if (cachedPatternManager != null)
        {
            cachedPatternManager.OnPatternAlert += HandleAlert;
            cachedPatternManager.OnPatternStateChanged += HandlePatternChanged;
        }
    }

    private void HandleAlert()
    {
        StartFade(maxOpacity, fadeInTime);
    }

    private void HandlePatternChanged()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeOutAfterDelay());
    }

    private IEnumerator FadeOutAfterDelay()
    {
        if (fadeOutDelay > 0f)
            yield return new WaitForSeconds(fadeOutDelay);

        yield return StartCoroutine(Fade(0f, fadeOutTime));

        fadeCoroutine = null;
    }

    private void StartFade(float targetAlpha, float duration)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(Fade(targetAlpha, duration));
    }

    private IEnumerator Fade(float targetAlpha, float duration)
    {
        float startAlpha = currentAlpha;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;

            float k = Mathf.Clamp01(t / Mathf.Max(0.01f, duration));
            currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, k);
            SetAlpha(currentAlpha);

            yield return null;
        }

        currentAlpha = targetAlpha;
        SetAlpha(currentAlpha);
    }

    private void SetAlpha(float alpha)
    {
        currentAlpha = alpha;

        if (fogSprite != null)
        {
            Color c = spriteBaseColor;
            c.a = alpha;
            fogSprite.color = c;
        }

        if (fogImage != null)
        {
            Color c = imageBaseColor;
            c.a = alpha;
            fogImage.color = c;
        }
    }
}