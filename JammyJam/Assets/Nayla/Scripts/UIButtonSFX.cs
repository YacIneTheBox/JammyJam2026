using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSFX : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip hoverSFX;
    [SerializeField] private AudioClip clickSFX;

    [Header("Settings")]
    [Range(0f, 1f)] [SerializeField] private float hoverVolume = 0.5f;
    [Range(0f, 1f)] [SerializeField] private float clickVolume = 0.8f;

    private AudioSource audioSource;

    private void Awake()
    {
        // Check if an AudioSource exists on this object, otherwise grab one from the scene or add it
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSFX != null)
        {
            audioSource.PlayOneShot(hoverSFX, hoverVolume);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSFX != null)
        {
            audioSource.PlayOneShot(clickSFX, clickVolume);
        }
    }
}