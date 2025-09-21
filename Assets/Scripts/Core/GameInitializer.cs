using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Collections.Generic;

public class GameInitializer : MonoBehaviour
{
    [Header("Prefab References")]
    public GameObject Background;
    public VolumeControl VolumeControl;
    public SfxVolumeControl SFXVolumeControl;
    public LevelLogger LevelLogger;
    public GameObject MusicPlayer;
    public SceneInfo SceneInfo;

    private GameObject backgroundClone;

    private async void Start()
    {
        try
        {
            Debug.Log("[GameInitializer]: Starting async game initialization...");
            
            // Step 1: Ensure AudioListener exists first (like original)
            //EnsureAudioListenerExists();
            
            // Step 2: Initialize background first (like original timing)
            await InitializeBackgroundAsync();
            
            // Step 3: Initialize music player (needed before VolumeControl)
            await InitializeMusicPlayerAsync();
            
            // Step 4: Initialize core systems in parallel
            await InitializeCoreSystemsAsync();
            
            // Step 5: Initialize remaining scene-specific systems  
            await InitializeLevelLoggerAsync();
            
            // Step 6: Load main menu scene LAST (like original)
            await LoadMainMenuAsync();
            
            Debug.Log("[GameInitializer]: Game initialization completed successfully");
        }
        catch (Exception exception)
        {
            Debug.LogError($"[GameInitializer]: Error during game initialization: {exception}");
        }
    }

    /// <summary>
    /// Initialize music player asynchronously - needed before VolumeControl
    /// </summary>
    private async Task InitializeMusicPlayerAsync()
    {
        if (MusicPlayer != null)
        {
            var musicPlayerClone = Instantiate(MusicPlayer);
            Debug.Log($"[GameInitializer]: Music Player '{musicPlayerClone.name}' initialized with tag '{musicPlayerClone.tag}'");
        }
        else
        {
            Debug.LogError("[GameInitializer]: Music Player prefab is null - background music will not play. Check GameInitializer prefab assignments!");
        }
        await Task.Yield();
    }

    /// <summary>
    /// Initialize core systems asynchronously in parallel for better performance
    /// </summary>
    private async Task InitializeCoreSystemsAsync()
    {
        Debug.Log("[GameInitializer]: Initializing core systems...");
        
        // Start all async initializations in parallel
        var initTasks = new List<Task>
        {
            InitializeAudioSystemAsync(),
            InitializeInputSystemAsync(),
            InitializeAchievementSystemAsync()
        };
        
        // Wait for all core systems to initialize
        await Task.WhenAll(initTasks);
        
        Debug.Log("[GameInitializer]: Core systems initialized");
    }

    /// <summary>
    /// Initialize audio systems asynchronously
    /// </summary>
    private async Task InitializeAudioSystemAsync()
    {
        // Ensure AudioListener exists first (synchronous but fast)
        //EnsureAudioListenerExists();
        
        // Initialize audio service asynchronously with prefab references
        await AudioService.Instance.InitializeWithPrefabsAsync(VolumeControl, SFXVolumeControl);
    }

    /// <summary>
    /// Initialize input systems asynchronously
    /// </summary>
    private async Task InitializeInputSystemAsync()
    {
        await InputService.Instance.InitializeAsync();
    }

    /// <summary>
    /// Initialize achievement system asynchronously
    /// </summary>
    private async Task InitializeAchievementSystemAsync()
    {
        // Initialize AchievementManager asynchronously
        var achievementManagerObj = FindFirstObjectByType<AchievementManager>();
        if (achievementManagerObj != null)
        {
            await achievementManagerObj.InitializeAsync();
            Debug.Log("[GameInitializer]: AchievementManager initialized");
        }
        else
        {
            Debug.LogWarning("[GameInitializer]: AchievementManager not found in scene");
        }
    }

    /// <summary>
    /// Initialize background asynchronously
    /// </summary>
    private async Task InitializeBackgroundAsync()
    {
        if (Background != null)
        {
            backgroundClone = Instantiate(Background);
            if (!backgroundClone.CompareTag("background"))
                backgroundClone.tag = "background";
            
            // Ensure background is visible initially (it should be managed by Update() later)
            backgroundClone.SetActive(true);
            Debug.Log($"[GameInitializer]: Background '{backgroundClone.name}' initialized and activated");
        }
        else
        {
            Debug.LogError("[GameInitializer]: Background prefab is null - cannot initialize background. Check GameInitializer prefab assignments!");
        }
        await Task.Yield();
    }

    /// <summary>
    /// Initialize level logger asynchronously
    /// </summary>
    private async Task InitializeLevelLoggerAsync()
    {
        if (LevelLogger != null)
        {
            Instantiate(LevelLogger);
            Debug.Log("[GameInitializer]: LevelLogger initialized");
        }
        await Task.Yield();
    }

    /// <summary>
    /// Load main menu scene asynchronously
    /// </summary>
    private async Task LoadMainMenuAsync()
    {
        Debug.Log("[GameInitializer]: Loading Main Menu scene...");
        await SceneManager.LoadSceneAsync("Main Menu", LoadSceneMode.Additive);
        Debug.Log("[GameInitializer]: Main Menu scene loaded");
    }

    private void Update()
    {
        if (backgroundClone == null)
        {
            return;
        }

        // Use the new SceneInfo system instead of hardcoded build indices
        var shouldShow = SceneInfo.ShouldShowBackground();
        
        if (backgroundClone.activeSelf != shouldShow)
        {
            backgroundClone.SetActive(shouldShow);
        }
    }

    private void OnDestroy()
    {
        // Clean up input service when GameInitializer is destroyed
        InputService.Instance?.Dispose();
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
            Debug.LogWarning("[GameInitializer]: No AudioListener found - creating temporary one");
            
            // Create a temporary GameObject with AudioListener to prevent Unity warnings
            var tempAudioListenerObject = new GameObject("TempAudioListener");
            tempAudioListenerObject.AddComponent<AudioListener>();
        }
        else if (audioListeners.Length > 1)
        {
            Debug.LogWarning($"[GameInitializer]: Found {audioListeners.Length} AudioListeners - disabling extras to prevent warnings");
            for (var i = 1; i < audioListeners.Length; i++)
            {
                if (audioListeners[i] != null)
                {
                    audioListeners[i].enabled = false;
                }
            }
        }
    }
}
