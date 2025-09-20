using UnityEngine;
using UnityEngine.SceneManagement;

public class VolumeControl : MonoBehaviour
{
    public static VolumeControl instance;

    [Header("Tags")]
    public string musicLooperTag = "MusicLooper";

    private AudioSource audioSource;
    private GameObject audioManagerInstance;

    /// <summary>
    /// Initialize this VolumeControl instance - called by AudioService
    /// </summary>
    public void Initialize()
    {
        instance = this;
        SetupAudioManager();
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log("[VolumeControl]: Initialized successfully");
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0) // Menu scene
        {
            SetupAudioManager(); // Just rebind UI, don't destroy or duplicate
        }
    }

    private void SetupAudioManager()
    {
        var managers = GameObject.FindGameObjectsWithTag(musicLooperTag);

        if (managers.Length > 0)
        {
            audioManagerInstance = managers[0];
            for (var i = 1; i < managers.Length; i++)
            {
                Destroy(managers[i]);
            }
        }
        else
        {
            audioManagerInstance = null;
        }

        if (audioManagerInstance != null)
            DontDestroyOnLoad(audioManagerInstance);

        if (audioManagerInstance != null)
            audioSource = audioManagerInstance.GetComponent<AudioSource>();
        else
            audioSource = null;
    }

    public void SetVolume(float value)
    {
        if (audioSource != null)
            audioSource.volume = value;
        PlayerPrefs.SetFloat("Volume", value);
        PlayerPrefs.Save();
    }
}