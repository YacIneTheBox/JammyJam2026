using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class UIButtonJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Scale Settings")]
    [SerializeField] private Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1f);
    [SerializeField] private float scaleDuration = 0.1f;

    [Header("Audio SFX")]
    [SerializeField] private AudioClip hoverSFX;
    [SerializeField] private AudioClip clickSFX;
    [Range(0f, 1f)] [SerializeField] private float hoverVolume = 0.5f;
    [Range(0f, 1f)] [SerializeField] private float clickVolume = 0.8f;

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;
    private AudioSource audioSource;
    private Button button;

    private void Awake()
    {
        originalScale = transform.localScale;
        button = GetComponent<Button>();

        // Reuse existing AudioSource on this object or add one dynamically
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void OnDisable()
    {
        // Safety reset if button is disabled mid-hover
        transform.localScale = originalScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Don't trigger hover effects on locked/non-interactable buttons
        if (button != null && !button.interactable) return;

        StartScaleAnimation(hoverScale);

        if (hoverSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverSFX, hoverVolume);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;

        StartScaleAnimation(originalScale);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;

        if (clickSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSFX, clickVolume);
        }
    }

    private void StartScaleAnimation(Vector3 targetScale)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(AnimateScale(targetScale));
    }

    private IEnumerator AnimateScale(Vector3 targetScale)
    {
        Vector3 initialScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < scaleDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsed / scaleDuration);
            yield return null;
        }

        transform.localScale = targetScale;
    }
}