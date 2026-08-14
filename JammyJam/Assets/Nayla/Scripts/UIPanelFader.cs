using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class UIPanelFader : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.25f;
    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void FadeIn()
    {
        gameObject.SetActive(true);
        StartFade(1f, interactable: true);
    }

    public void FadeOut()
    {
        StartFade(0f, interactable: false, disableOnComplete: true);
    }

    private void StartFade(float targetAlpha, bool interactable, bool disableOnComplete = false)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(AnimateFade(targetAlpha, interactable, disableOnComplete));
    }

    private IEnumerator AnimateFade(float targetAlpha, bool interactable, bool disableOnComplete)
    {
        canvasGroup.blocksRaycasts = false; // Prevent double clicks mid-fade
        canvasGroup.interactable = false;

        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        canvasGroup.interactable = interactable;
        canvasGroup.blocksRaycasts = interactable;

        if (disableOnComplete && targetAlpha == 0f)
        {
            gameObject.SetActive(false);
        }
    }
}