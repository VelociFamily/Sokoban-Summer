using UnityEngine;
using System.Threading.Tasks;

/// <summary>
/// Centralized audio service to manage volume controls and audio sources
/// Reduces the need for multiple MonoBehaviour-based audio managers
/// Ensures only one main AudioSource exists across all scenes
/// </summary>
public class AudioService : IAsyncInitializable
{
    private static AudioService _instance;
    public static AudioService Instance => _instance ??= new AudioService();

    private VolumeControl _volumeControl;
    private SfxVolumeControl _sfxVolumeControl;
    private AudioSource _mainAudioSource;

    private AudioService() { }

    public async Task InitializeAsync()
    {
        Debug.Log("[AudioService]: Initializing audio systems...");
        
        // Initialize volume controls asynchronously
        await InitializeVolumeControls();
        await InitializeAudioSource();
        
        Debug.Log("[AudioService]: Audio systems initialized successfully");
    }

    /// <summary>
    /// Initialize with specific prefab references from GameInitializer
    /// </summary>
    public async Task InitializeWithPrefabsAsync(VolumeControl volumeControlPrefab, SfxVolumeControl sfxVolumeControlPrefab)
    {
        Debug.Log("[AudioService]: Initializing audio systems with prefabs...");
        
        // Initialize with provided prefabs
        await InitializeVolumeControlsWithPrefabs(volumeControlPrefab, sfxVolumeControlPrefab);
        await InitializeAudioSourceWithPrefab();
        
        Debug.Log("[AudioService]: Audio systems with prefabs initialized successfully");
    }

    private async Task InitializeVolumeControls()
    {
        // Find or create volume control instances
        _volumeControl = Object.FindFirstObjectByType<VolumeControl>();
        if (_volumeControl == null)
        {
            var volumeControlPrefab = Resources.Load<VolumeControl>("VolumeControl");
            if (volumeControlPrefab != null)
            {
                _volumeControl = Object.Instantiate(volumeControlPrefab);
            }
        }

        // Initialize VolumeControl if found/created
        if (_volumeControl != null)
        {
            _volumeControl.Initialize();
        }

        _sfxVolumeControl = Object.FindFirstObjectByType<SfxVolumeControl>();
        if (_sfxVolumeControl == null)
        {
            var sfxVolumeControlPrefab = Resources.Load<SfxVolumeControl>("SFXVolumeControl");
            if (sfxVolumeControlPrefab != null)
            {
                _sfxVolumeControl = Object.Instantiate(sfxVolumeControlPrefab);
            }
        }

        // Initialize SfxVolumeControl if found/created
        if (_sfxVolumeControl != null)
        {
            _sfxVolumeControl.Initialize();
        }
        
        await Task.Yield(); // Ensure async behavior
    }

    private async Task InitializeAudioSource()
    {
        // Destroy any existing extra AudioSources to ensure only one exists
        var existingAudioSources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        
        if (existingAudioSources.Length > 1)
        {
            Debug.LogWarning($"[AudioService]: Found {existingAudioSources.Length} AudioSources in scene. Consolidating to single instance.");
            
            // Keep the first one and destroy the rest
            _mainAudioSource = existingAudioSources[0];
            for (int i = 1; i < existingAudioSources.Length; i++)
            {
                if (existingAudioSources[i] != null && existingAudioSources[i] != _mainAudioSource)
                {
                    Debug.Log($"[AudioService]: Destroying duplicate AudioSource on '{existingAudioSources[i].gameObject.name}'");
                    Object.Destroy(existingAudioSources[i]);
                }
            }
        }
        else if (existingAudioSources.Length == 1)
        {
            _mainAudioSource = existingAudioSources[0];
        }
        else
        {
            // No AudioSource found, try to load from Resources
            var audioSourcePrefab = Resources.Load<AudioSource>("AudioSource");
            if (audioSourcePrefab != null)
            {
                _mainAudioSource = Object.Instantiate(audioSourcePrefab);
            }
            else
            {
                // Create a basic AudioSource GameObject
                var audioSourceObject = new GameObject("Main Audio Source");
                _mainAudioSource = audioSourceObject.AddComponent<AudioSource>();
            }
        }
        
        // Ensure the main AudioSource persists across scene changes
        if (_mainAudioSource != null)
        {
            Object.DontDestroyOnLoad(_mainAudioSource.gameObject);
            Debug.Log($"[AudioService]: Main AudioSource '{_mainAudioSource.gameObject.name}' set to persist across scenes");
        }
        
        await Task.Yield(); // Ensure async behavior
    }

    private async Task InitializeVolumeControlsWithPrefabs(VolumeControl volumeControlPrefab, SfxVolumeControl sfxVolumeControlPrefab)
    {
        // Find existing or create new volume control instances
        _volumeControl = Object.FindFirstObjectByType<VolumeControl>();
        if (_volumeControl == null && volumeControlPrefab != null)
        {
            _volumeControl = Object.Instantiate(volumeControlPrefab);
        }

        // Initialize VolumeControl if found/created
        if (_volumeControl != null)
        {
            _volumeControl.Initialize();
        }

        _sfxVolumeControl = Object.FindFirstObjectByType<SfxVolumeControl>();
        if (_sfxVolumeControl == null && sfxVolumeControlPrefab != null)
        {
            _sfxVolumeControl = Object.Instantiate(sfxVolumeControlPrefab);
        }

        // Initialize SfxVolumeControl if found/created
        if (_sfxVolumeControl != null)
        {
            _sfxVolumeControl.Initialize();
        }
        
        await Task.Yield(); // Ensure async behavior
    }

    private async Task InitializeAudioSourceWithPrefab()
    {
        // Destroy any existing extra AudioSources to ensure only one exists
        var existingAudioSources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        
        switch (existingAudioSources.Length)
        {
            case > 1:
            {
                Debug.LogWarning($"[AudioService]: Found {existingAudioSources.Length} AudioSources in scene. Consolidating to single instance.");
            
                // Keep the first one and destroy the rest
                _mainAudioSource = existingAudioSources[0];
                for (var i = 1; i < existingAudioSources.Length; i++)
                {
                    if (existingAudioSources[i] == null || existingAudioSources[i] == _mainAudioSource) continue;
                    Debug.Log($"[AudioService]: Destroying duplicate AudioSource on '{existingAudioSources[i].gameObject.name}'");
                    Object.Destroy(existingAudioSources[i]);
                }

                break;
            }
            case 1:
                _mainAudioSource = existingAudioSources[0];
                break;
            default:
            {
                // Create a basic AudioSource GameObject
                var audioSourceObject = new GameObject("Main Audio Source");
                _mainAudioSource = audioSourceObject.AddComponent<AudioSource>();
                break;
            }
        }
        
        // Ensure the main AudioSource persists across scene changes
        if (_mainAudioSource != null)
        {
            Object.DontDestroyOnLoad(_mainAudioSource.gameObject);
            Debug.Log($"[AudioService]: Main AudioSource '{_mainAudioSource.gameObject.name}' set to persist across scenes");
        }
        
        await Task.Yield(); // Ensure async behavior
    }

    /// <summary>
    /// Get the centralized main AudioSource - use this instead of individual AudioSources
    /// </summary>
    public AudioSource GetMainAudioSource()
    {
        if (_mainAudioSource == null)
        {
            Debug.LogWarning("[AudioService]: Main AudioSource is null. Make sure AudioService is initialized first.");
        }
        return _mainAudioSource;
    }

    /// <summary>
    /// Play a one-shot audio clip using the centralized AudioSource
    /// </summary>
    public void PlayOneShot(AudioClip clip, float volumeScale = 1.0f)
    {
        if (_mainAudioSource != null && clip != null)
        {
            _mainAudioSource.PlayOneShot(clip, volumeScale);
        }
        else
        {
            Debug.LogWarning("[AudioService]: Cannot play one-shot clip - AudioSource or clip is null");
        }
    }

    /// <summary>
    /// Play an audio clip using the centralized AudioSource
    /// </summary>
    public void PlayClip(AudioClip clip)
    {
        if (_mainAudioSource != null && clip != null)
        {
            _mainAudioSource.clip = clip;
            _mainAudioSource.Play();
        }
        else
        {
            Debug.LogWarning("[AudioService]: Cannot play clip - AudioSource or clip is null");
        }
    }

    public VolumeControl GetVolumeControl() => _volumeControl;
    public SfxVolumeControl GetSfxVolumeControl() => _sfxVolumeControl;
}
