using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using CoreShared;

namespace Audio
{
    /// <summary>
    /// Modern, unified audio manager following Unity best practices
    /// Handles all audio types (Music, SFX, etc.) through AudioMixer groups
    /// Replaces the fragmented VolumeControl/SfxVolumeControl system
    /// </summary>
    public class UnifiedAudioManager : MonoBehaviour, IAudioManager
    {
        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer masterMixer;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Volume Settings")]
        [SerializeField] private AudioVolumeSettings volumeSettings;

        // Singleton instance
        public static UnifiedAudioManager Instance { get; private set; }

        // Events for volume changes (uses enum for readability in-audio domain)
        public static event Action<AudioChannelType, float> OnVolumeChanged;

        // UI sliders for volume control
        private Dictionary<AudioChannelType, Slider> volumeSliders = new Dictionary<AudioChannelType, Slider>();

        private void Awake()
        {
            // Implement singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
        }

        private void Start()
        {
            InitializeVolumeSettings();
            FindAndRegisterSliders();
        }

        private void InitializeAudioSources()
        {
            // Create audio sources if not assigned
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.playOnAwake = false;
                musicSource.loop = true;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.loop = false;
            }

            // Set audio sources to use correct mixer groups
            if (masterMixer != null)
            {
                var musicGroup = masterMixer.FindMatchingGroups("Music");
                if (musicGroup.Length > 0)
                    musicSource.outputAudioMixerGroup = musicGroup[0];

                var sfxGroup = masterMixer.FindMatchingGroups("SFX");
                if (sfxGroup.Length > 0)
                    sfxSource.outputAudioMixerGroup = sfxGroup[0];
            }
        }

        private void InitializeVolumeSettings()
        {
            if (volumeSettings == null)
            {
                Debug.LogWarning("[UnifiedAudioManager]: No AudioVolumeSettings assigned, creating default settings");
                volumeSettings = ScriptableObject.CreateInstance<AudioVolumeSettings>();
            }

            // Load saved volume settings and apply them
            var settingsData = Core.SaveFacade.Instance?.Settings;
            if (volumeSettings != null)
            {
                foreach (var setting in volumeSettings.VolumeChannels)
                {
                    float savedVolume = setting.DefaultVolume;
                    if (settingsData != null)
                    {
                        switch (setting.ChannelType)
                        {
                            case AudioChannelType.Master:
                                savedVolume = settingsData.masterVolume; break;
                            case AudioChannelType.Music:
                                savedVolume = settingsData.musicVolume; break;
                            case AudioChannelType.SFX:
                                savedVolume = settingsData.sfxVolume; break;
                        }
                    }

                    SetVolume(setting.ChannelType, savedVolume, false); // Don't save again
                }
            }
        }

        private void FindAndRegisterSliders()
        {
            // Find all VolumeSlider components in the scene and register them
            try
            {
                var volumeSliderComponents = FindObjectsByType<VolumeSlider>(FindObjectsSortMode.None);
                foreach (var sliderComponent in volumeSliderComponents)
                    RegisterVolumeSlider(sliderComponent.ChannelType, sliderComponent.Slider);
            }
            catch
            {
                // fallback for older/newer Unity APIs
            }
        }

        public void RegisterVolumeSlider(AudioChannelType channelType, Slider slider)
        {
            if (slider == null) return;

            volumeSliders[channelType] = slider;

            // Set slider to current volume
            var volumeChannel = volumeSettings?.GetVolumeChannel(channelType);
            if (volumeChannel != null)
            {
                float currentVolume = volumeChannel.DefaultVolume;
                var settingsData = Core.SaveFacade.Instance?.Settings;
                if (settingsData != null)
                {
                    switch (channelType)
                    {
                        case AudioChannelType.Master: currentVolume = settingsData.masterVolume; break;
                        case AudioChannelType.Music: currentVolume = settingsData.musicVolume; break;
                        case AudioChannelType.SFX: currentVolume = settingsData.sfxVolume; break;
                    }
                }
                slider.value = currentVolume;
            }

            // Listen for slider changes
            slider.onValueChanged.RemoveAllListeners(); // Remove existing listeners to prevent duplicates
            slider.onValueChanged.AddListener(value => SetVolume(channelType, value));

            Debug.Log($"[UnifiedAudioManager]: Registered {channelType} volume slider");
        }

        // Primary SetVolume implementation using enum
        public void SetVolume(AudioChannelType channelType, float volume, bool saveToPrefs = true)
        {
            volume = Mathf.Clamp01(volume);

            var volumeChannel = volumeSettings?.GetVolumeChannel(channelType);
            if (volumeChannel == null)
            {
                Debug.LogWarning($"[UnifiedAudioManager]: Unknown audio channel type: {channelType}");
                return;
            }

            // Convert linear volume to decibels for AudioMixer
            float volumeDb = volume > 0f ? Mathf.Log10(volume) * 20f : -80f;

            // Apply to AudioMixer
            if (masterMixer != null && !string.IsNullOrEmpty(volumeChannel.MixerParameter))
            {
                masterMixer.SetFloat(volumeChannel.MixerParameter, volumeDb);
            }

            // Persist to SaveService
            if (saveToPrefs && Core.SaveFacade.Instance != null)
            {
                switch (channelType)
                {
                    case AudioChannelType.Master: Core.SaveFacade.Instance.Settings.masterVolume = volume; break;
                    case AudioChannelType.Music: Core.SaveFacade.Instance.Settings.musicVolume = volume; break;
                    case AudioChannelType.SFX: Core.SaveFacade.Instance.Settings.sfxVolume = volume; break;
                }
                Core.SaveFacade.Instance.SaveSettings();
            }

            // Update UI slider if exists
            if (volumeSliders.TryGetValue(channelType, out Slider slider) && slider != null)
            {
                if (Mathf.Abs(slider.value - volume) > 0.001f) // Prevent feedback loop
                {
                    slider.value = volume;
                }
            }

            // Notify listeners
            OnVolumeChanged?.Invoke(channelType, volume);

            Debug.Log($"[UnifiedAudioManager]: Set {channelType} volume to {volume:F2} ({volumeDb:F1}dB)");
        }

        // IAudioManager-friendly overloads (int-based) so Core can use the shared interface
        public void SetVolume(int channelType, float volume)
        {
            SetVolume((AudioChannelType)channelType, volume, true);
        }

        public float GetVolume(AudioChannelType channelType)
        {
            var vc = volumeSettings?.GetVolumeChannel(channelType);
            float def = vc != null ? vc.DefaultVolume : 1f;
            var settingsData = Core.SaveFacade.Instance?.Settings;
            if (settingsData == null) return def;
            return channelType switch
            {
                AudioChannelType.Master => settingsData.masterVolume,
                AudioChannelType.Music => settingsData.musicVolume,
                AudioChannelType.SFX => settingsData.sfxVolume,
                _ => def
            };
        }

        public float GetVolume(int channelType)
        {
            return GetVolume((AudioChannelType)channelType);
        }

        // Music playback methods
        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (musicSource != null && clip != null)
            {
                musicSource.clip = clip;
                musicSource.loop = loop;
                musicSource.Play();
            }
        }

        public void StopMusic()
        {
            if (musicSource != null)
                musicSource.Stop();
        }

        public void PauseMusic()
        {
            if (musicSource != null)
                musicSource.Pause();
        }

        public void ResumeMusic()
        {
            if (musicSource != null)
                musicSource.UnPause();
        }

        // SFX playback methods
        public void PlaySFX(AudioClip clip, float volumeScale = 1f)
        {
            if (sfxSource != null && clip != null)
            {
                sfxSource.PlayOneShot(clip, volumeScale);
            }
        }

        public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volumeScale = 1f)
        {
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, position, volumeScale * GetVolume(AudioChannelType.SFX));
            }
        }

        private void OnDestroy()
        {
            // Clean up slider listeners
            foreach (var kvp in volumeSliders)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.onValueChanged.RemoveAllListeners();
                }
            }
        }
    }

    /// <summary>
    /// Enum for different audio channel types
    /// </summary>
    public enum AudioChannelType
    {
        Master,
        Music,
        SFX,
        Voice,
        Ambient
    }
}
