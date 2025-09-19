using UnityEngine;
using UnityEngine.UI;

public class VolumeSliderConnector : MonoBehaviour
{
    [Header("References")]
    public Slider volumeSlider; // Assign in Inspector
    public Slider sfxSlider;    // Assign in Inspector

    private VolumeControl volumeControl;
    private SfxVolumeControl sfxVolumeControl;

    private void Start()
    {
        volumeControl = VolumeControl.instance;
        sfxVolumeControl = SfxVolumeControl.Instance;

        if (volumeControl == null)
        {
            Debug.LogError("[VolumeSliderConnector]: VolumeControl instance not found in scene");
        }
        else if (volumeSlider == null)
        {
            Debug.LogError("[VolumeSliderConnector]: Volume slider component not assigned in inspector");
        }
        else
        {
            var savedVolume = PlayerPrefs.GetFloat("Volume", 1f);
            volumeSlider.value = Mathf.Clamp(savedVolume, 0f, 1f);
            volumeSlider.onValueChanged.AddListener(OnVolumeSliderValueChanged);
        }

        if (sfxVolumeControl == null)
        {
            Debug.LogError("[VolumeSliderConnector]: SFXVolumeControl instance not found in scene");
        }
        else if (sfxSlider == null)
        {
            Debug.LogError("[VolumeSliderConnector]: SFX slider component not assigned in inspector");
        }
        else
        {
            var savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxSlider.value = Mathf.Clamp(savedSFXVolume, 0f, 1f);
            sfxSlider.onValueChanged.AddListener(OnSFXSliderValueChanged);
        }
    }

    private void OnVolumeSliderValueChanged(float value)
    {
        if (volumeControl != null)
            volumeControl.SetVolume(value);
    }

    private void OnSFXSliderValueChanged(float value)
    {
        if (sfxVolumeControl != null)
            sfxVolumeControl.SetSfxVolume(value);
    }

    private void OnDestroy()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnVolumeSliderValueChanged);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnSFXSliderValueChanged);
    }
}
