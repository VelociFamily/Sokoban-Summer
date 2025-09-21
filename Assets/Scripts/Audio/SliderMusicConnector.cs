using UnityEngine;
using UnityEngine.UI;

namespace Audio
{
    public class SliderMusicConnector : MonoBehaviour
    {
        private Slider slider;
        private AudioSource musicPlayer;

        private void Awake()
        {
            // Get the slider attached to this GameObject
            slider = GetComponent<Slider>();

            if (slider == null)
            {
                Debug.LogError($"[SliderMusicConnector]: No Slider component found on '{gameObject.name}'");
                return;
            }

            // Find the AudioSource with the tag "Music Looper"
            var musicObj = GameObject.FindWithTag("Music Looper");
            if (musicObj != null)
            {
                musicPlayer = musicObj.GetComponent<AudioSource>();
            }

            if (musicPlayer == null)
            {
                Debug.LogError("[SliderMusicConnector]: No AudioSource with tag 'Music Looper' found in scene");
                return;
            }

            // Set slider to current music volume
            slider.value = musicPlayer.volume;

            // Listen for slider changes
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        private void OnSliderValueChanged(float value)
        {
            if (musicPlayer != null)
            {
                musicPlayer.volume = value;
            }
        }

        private void OnDestroy()
        {
            // Clean up listener
            if (slider != null)
                slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
    }
}
