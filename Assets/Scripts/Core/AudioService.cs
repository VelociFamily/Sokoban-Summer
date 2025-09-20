using UnityEngine;
using System.Threading.Tasks;

/// <summary>
/// Centralized audio service to manage volume controls and audio sources
/// Reduces the need for multiple MonoBehaviour-based audio managers
/// </summary>
public class AudioService : IAsyncInitializable
{
    private static AudioService _instance;
    public static AudioService Instance => _instance ??= new AudioService();

    private VolumeControl _volumeControl;
    private SfxVolumeControl _sfxVolumeControl;
    private AudioSource _audioSource;

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
    public async Task InitializeWithPrefabsAsync(VolumeControl volumeControlPrefab, SfxVolumeControl sfxVolumeControlPrefab, AudioSource audioSourcePrefab)
    {
        Debug.Log("[AudioService]: Initializing audio systems with prefabs...");
        
        // Initialize with provided prefabs
        await InitializeVolumeControlsWithPrefabs(volumeControlPrefab, sfxVolumeControlPrefab);
        await InitializeAudioSourceWithPrefab(audioSourcePrefab);
        
        Debug.Log("[AudioService]: Audio systems with prefabs initialized successfully");
    }

    private async Task InitializeVolumeControls()
    {
        // Find or create volume control instances
        _volumeControl = Object.FindObjectOfType<VolumeControl>();
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

        _sfxVolumeControl = Object.FindObjectOfType<SfxVolumeControl>();
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
        _audioSource = Object.FindObjectOfType<AudioSource>();
        if (_audioSource == null)
        {
            var audioSourcePrefab = Resources.Load<AudioSource>("AudioSource");
            if (audioSourcePrefab != null)
            {
                _audioSource = Object.Instantiate(audioSourcePrefab);
            }
        }
        
        await Task.Yield(); // Ensure async behavior
    }

    private async Task InitializeVolumeControlsWithPrefabs(VolumeControl volumeControlPrefab, SfxVolumeControl sfxVolumeControlPrefab)
    {
        // Find existing or create new volume control instances
        _volumeControl = Object.FindObjectOfType<VolumeControl>();
        if (_volumeControl == null && volumeControlPrefab != null)
        {
            _volumeControl = Object.Instantiate(volumeControlPrefab);
        }

        // Initialize VolumeControl if found/created
        if (_volumeControl != null)
        {
            _volumeControl.Initialize();
        }

        _sfxVolumeControl = Object.FindObjectOfType<SfxVolumeControl>();
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

    private async Task InitializeAudioSourceWithPrefab(AudioSource audioSourcePrefab)
    {
        _audioSource = Object.FindObjectOfType<AudioSource>();
        if (_audioSource == null && audioSourcePrefab != null)
        {
            _audioSource = Object.Instantiate(audioSourcePrefab);
        }
        
        await Task.Yield(); // Ensure async behavior
    }

    public VolumeControl GetVolumeControl() => _volumeControl;
    public SfxVolumeControl GetSfxVolumeControl() => _sfxVolumeControl;
    public AudioSource GetAudioSource() => _audioSource;
}