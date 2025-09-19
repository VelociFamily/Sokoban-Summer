using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInitiator : MonoBehaviour
{
    public GameObject Background;
    private GameObject backgroundClone;
    public SfxVolumeControl SFXVolumeControl;
    public VolumeControl VolumeControl;
    public AudioSource AudioSource;
    public LevelLogger LevelLogger;

    private AchievementManager achievementManager;

    private async void Start()
    {
        try
        {
            // Ensure there's an AudioListener in the scene before doing anything else
            EnsureAudioListenerExists();
            
            backgroundClone = Instantiate(Background);
            if (!backgroundClone.CompareTag("background"))
                backgroundClone.tag = "background";
            Instantiate(VolumeControl);
            Instantiate(AudioSource);
            Instantiate(LevelLogger);
            Instantiate(SFXVolumeControl);
            achievementManager = AchievementManager.Instance;
            await SceneManager.LoadSceneAsync("Main Menu", LoadSceneMode.Additive);
        }
        catch (Exception exception)
        {
            Debug.LogError($"[GameInitiator]: Error during game initialization: {exception}");
        }
    }

    private void Update()
    {
        if (backgroundClone == null)
            return;
        for (var index = 0; index < SceneManager.sceneCount; index++)
            switch (SceneManager.GetSceneAt(index).buildIndex)
            {
                case >= 2 and <= 7:
                    backgroundClone.SetActive(false);
                    break;
                case 1:
                    backgroundClone.SetActive(true);
                    break;
            }
    }

    /// <summary>
    /// Ensures there's always an AudioListener in the scene to prevent Unity warnings.
    /// Creates a minimal AudioListener if none exists.
    /// </summary>
    private void EnsureAudioListenerExists()
    {
        var audioListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        
        if (audioListeners.Length == 0)
        {
            Debug.LogWarning("[GameInitiator]: No AudioListener found in scene - creating temporary one");
            
            // Create a temporary GameObject with AudioListener to prevent Unity warnings
            var tempAudioListenerObject = new GameObject("TempAudioListener");
            tempAudioListenerObject.AddComponent<AudioListener>();
            
            Debug.Log("[GameInitiator]: Temporary AudioListener created - will be managed by scene loading system");
        }
        else
        {
            Debug.Log($"[GameInitiator]: Found {audioListeners.Length} AudioListener(s) in scene");
        }
    }
}
