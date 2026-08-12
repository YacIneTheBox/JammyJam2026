using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuUI : MonoBehaviour
{
    public Slider masterVolumeSlider;

    private void OnEnable()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.SetValueWithoutNotify(SettingsManager.Instance.MasterVolume);
            masterVolumeSlider.onValueChanged.AddListener(HandleVolumeChanged);
        }
    }

    private void OnDisable()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.RemoveListener(HandleVolumeChanged);
    }

    private void HandleVolumeChanged(float value)
    {
        SettingsManager.Instance.SetVolume(value);
    }
}