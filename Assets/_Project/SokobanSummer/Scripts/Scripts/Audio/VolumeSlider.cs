using UnityEngine;
using UnityEngine.UI;

namespace Audio
{
    /// <summary>
    /// Modern UI component that automatically connects sliders to the UnifiedAudioManager
    /// Replaces the old VolumeSliderConnector approach with a cleaner, single-purpose component
    /// </summary>
    public class VolumeSlider : MonoBehaviour
    {
        [Header("Volume Channel")]
        [SerializeField] private AudioChannelType channelType = AudioChannelType.Master;
        
        [Header("UI Reference")]
        [SerializeField] private Slider slider;
        
        public AudioChannelType ChannelType => channelType;
        public Slider Slider => slider;
        
        private void Awake()
        {
            // Auto-find slider if not assigned
            if (slider == null)
                slider = GetComponent<Slider>();
                
            if (slider == null)
                slider = GetComponentInChildren<Slider>();
                
            if (slider == null)
            {
                Debug.LogError($"[VolumeSlider]: No Slider component found for {channelType} volume control on '{gameObject.name}'");
            }
        }
        
        private void Start()
        {
            // Register with UnifiedAudioManager when it's available
            RegisterWithAudioManager();
        }
        
        private void RegisterWithAudioManager()
        {
            if (UnifiedAudioManager.Instance != null && slider != null)
            {
                UnifiedAudioManager.Instance.RegisterVolumeSlider(channelType, slider);
            }
            else if (slider != null)
            {
                // If AudioManager isn't ready yet, try again next frame
                Invoke(nameof(RegisterWithAudioManager), 0.1f);
            }
        }
        
        private void OnValidate()
        {
            // Auto-find slider in editor
            if (slider == null)
                slider = GetComponent<Slider>();
        }
    }
}
