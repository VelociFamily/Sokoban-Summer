using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SfxVolumeControl : MonoBehaviour
{
    [Header("References")]
    public Slider sfxSlider; // Assign in Inspector
    public AudioMixer audioMixer;

    public static SfxVolumeControl Instance;

    /// <summary>
    /// Initialize this SfxVolumeControl instance - called by AudioService
    /// </summary>
    public void Initialize()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        SetupSfxSlider();
        Debug.Log("[SfxVolumeControl]: Initialized successfully");
    }

    private void SetupSfxSlider()
    {
        if (sfxSlider == null)
        {
            Debug.LogWarning("[SFXVolumeControl]: SFX slider component not assigned in inspector - attempting to find it automatically");
            
            // Try to find SFX slider automatically
            var sliderConnector = FindFirstObjectByType<VolumeSliderConnector>();
            if (sliderConnector != null && sliderConnector.SfxSlider != null)
            {
                sfxSlider = sliderConnector.SfxSlider;
                Debug.Log("[SFXVolumeControl]: Found SFX slider reference via VolumeSliderConnector");
            }
            else
            {
                Debug.LogWarning("[SFXVolumeControl]: Could not find SFX slider - SFX volume control disabled");
                return;
            }
        }

        var savedVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        sfxSlider.value = Mathf.Clamp(savedVolume, 0f, 1f);
        SetSfxVolume(savedVolume);

        sfxSlider.onValueChanged.AddListener(OnSFXSliderValueChanged);
    }

    private void OnSFXSliderValueChanged(float value)
    {
        SetSfxVolume(value);
    }

    private void OnDestroy()
    {
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(OnSFXSliderValueChanged);
    }

    public void SetSfxVolume(float value)
    {
        var dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
        if (audioMixer != null)
            audioMixer.SetFloat("SFXVolume", dB);
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
    }
}