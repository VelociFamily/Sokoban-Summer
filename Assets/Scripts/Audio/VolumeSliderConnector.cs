using UnityEngine;
using UnityEngine.UI;

namespace Audio
{
    /// <summary>
    /// Legacy volume slider connector - replaced by VolumeSlider components
    /// </summary>
    [System.Obsolete("VolumeSliderConnector is deprecated. Use individual VolumeSlider components for each channel instead.", false)]
    public class VolumeSliderConnector : MonoBehaviour
    {
        [Header("References")]
        public Slider volumeSlider; // Assign in Inspector
        public Slider sfxSlider;    // Assign in Inspector

        private VolumeControl volumeControl;
        private SfxVolumeControl sfxVolumeControl;

        // Public property to allow SFXVolumeControl to access the slider
        public Slider SfxSlider => sfxSlider;

        private void Start()
        {
            volumeControl = VolumeControl.instance;
            sfxVolumeControl = SfxVolumeControl.Instance;

            // Handle VolumeControl setup
            if (volumeControl == null)
            {
                Debug.LogWarning("[VolumeSliderConnector]: VolumeControl instance not found in scene - volume control disabled");
            }
            else if (volumeSlider == null)
            {
                Debug.LogWarning("[VolumeSliderConnector]: Volume slider component not assigned in inspector - volume control disabled");
            }
            else
            {
                var savedVolume = PlayerPrefs.GetFloat("Volume", 1f);
                volumeSlider.value = Mathf.Clamp(savedVolume, 0f, 1f);
                volumeSlider.onValueChanged.AddListener(OnVolumeSliderValueChanged);
            }

            // Handle SFXVolumeControl setup
            if (sfxVolumeControl == null)
            {
                Debug.LogWarning("[VolumeSliderConnector]: SFXVolumeControl instance not found in scene - SFX control disabled");
            }
            else if (sfxSlider == null)
            {
                Debug.LogWarning("[VolumeSliderConnector]: SFX slider component not assigned in inspector - SFX control disabled");
            }
            else
            {
                var savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
                sfxSlider.value = Mathf.Clamp(savedSFXVolume, 0f, 1f);
                sfxSlider.onValueChanged.AddListener(OnSFXSliderValueChanged);
                
                // Notify SfxVolumeControl that slider is now available
                if (sfxVolumeControl != null)
                {
                    sfxVolumeControl.RetrySliderSetup();
                }
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
}
