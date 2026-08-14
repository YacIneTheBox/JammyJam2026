using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MixerAudio : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("UI Sliders")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;

    private const string SFX_KEY = "SFXVolume";
    private const string MUSIC_KEY = "MusicVolume";

    private void Start()
    {
        // Load saved volumes or default to 0.75f
        float savedSFX = PlayerPrefs.GetFloat(SFX_KEY, 0.75f);
        float savedMusic = PlayerPrefs.GetFloat(MUSIC_KEY, 0.75f);

        if (sfxSlider != null)
        {
            sfxSlider.value = savedSFX;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        if (musicSlider != null)
        {
            musicSlider.value = savedMusic;
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        // Apply loaded values to AudioMixer
        SetSFXVolume(savedSFX);
        SetMusicVolume(savedMusic);
    }

    public void SetSFXVolume(float sliderValue)
    {
        // Convert 0.0001 - 1 slider scale to logarithmic decibel scale (-80dB to 0dB)
        float db = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat("SFXVolume", db);
        PlayerPrefs.SetFloat(SFX_KEY, sliderValue);
    }

    public void SetMusicVolume(float sliderValue)
    {
        float db = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat("MusicVolume", db);
        PlayerPrefs.SetFloat(MUSIC_KEY, sliderValue);
    }
}