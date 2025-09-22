using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Audio
{
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
            
                // Try multiple approaches to find SFX slider
                var sliderConnector = FindFirstObjectByType<VolumeSliderConnector>();
                if (sliderConnector != null && sliderConnector.SfxSlider != null)
                {
                    sfxSlider = sliderConnector.SfxSlider;
                    Debug.Log("[SFXVolumeControl]: Found SFX slider reference via VolumeSliderConnector");
                }
                else
                {
                    // Try to find slider by name or tag as fallback
                    var sliderObject = GameObject.FindGameObjectWithTag("SFXSlider");
                    if (sliderObject != null)
                    {
                        sfxSlider = sliderObject.GetComponent<Slider>();
                        if (sfxSlider != null)
                        {
                            Debug.Log("[SFXVolumeControl]: Found SFX slider by tag");
                        }
                    }
                    
                    if (sfxSlider == null)
                    {
                        // Try finding by name
                        sliderObject = GameObject.Find("SFX Slider") ?? GameObject.Find("SfxSlider");
                        if (sliderObject != null)
                        {
                            sfxSlider = sliderObject.GetComponent<Slider>();
                            if (sfxSlider != null)
                            {
                                Debug.Log("[SFXVolumeControl]: Found SFX slider by name");
                            }
                        }
                    }
                    
                    if (sfxSlider == null)
                    {
                        Debug.LogWarning("[SFXVolumeControl]: Could not find SFX slider - SFX volume control will try again when sliders are available");
                        // Don't return here - let the volume control still initialize, but defer slider setup
                        SetSfxVolumeOnly(PlayerPrefs.GetFloat("SFXVolume", 1f));
                        return;
                    }
                }
            }

            var savedVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxSlider.value = Mathf.Clamp(savedVolume, 0f, 1f);
            SetSfxVolume(savedVolume);

            sfxSlider.onValueChanged.AddListener(OnSFXSliderValueChanged);
        }

        /// <summary>
        /// Set SFX volume without requiring a slider (fallback method)
        /// </summary>
        private void SetSfxVolumeOnly(float value)
        {
            var dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
            if (audioMixer != null)
                audioMixer.SetFloat("SFXVolume", dB);
            PlayerPrefs.SetFloat("SFXVolume", value);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Manually retry slider setup - can be called when sliders are available
        /// </summary>
        public void RetrySliderSetup()
        {
            if (sfxSlider == null)
            {
                Debug.Log("[SFXVolumeControl]: Retrying slider setup");
                SetupSfxSlider();
            }
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
}