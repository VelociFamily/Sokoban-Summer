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

    private GameObject backgroundClone;
    private AchievementManager achievementManager;
    private List<IAsyncInitializable> asyncInitializables = new List<IAsyncInitializable>();

    private async void Start()
    {
        try
        {
            Debug.Log("[GameInitializer]: Starting async game initialization...");
            
            // Step 1: Ensure AudioListener exists first (like original)
            EnsureAudioListenerExists();
            
            // Step 2: Initialize background first (like original timing)
            await InitializeBackgroundAsync();
            
            // Step 3: Initialize core systems in parallel
            await InitializeCoreSystemsAsync();
            
            // Step 4: Initialize remaining scene-specific systems  
            await InitializeLevelLoggerAsync();
            
            // Step 5: Load main menu scene LAST (like original)
            await LoadMainMenuAsync();
            
            Debug.Log("[GameInitializer]: Game initialization completed successfully");
        }
        catch (Exception exception)
        {
            Debug.LogError($"[GameInitializer]: Error during game initialization: {exception}");
        }
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
        EnsureAudioListenerExists();
        
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
            achievementManager = AchievementManager.Instance;
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

        // Check the active scene instead of iterating through all loaded scenes
        var activeScene = SceneManager.GetActiveScene();
        
        switch (activeScene.buildIndex)
        {
            case >= 2 and <= 7: // Tutorial and level scenes
                if (backgroundClone.activeSelf)
                {
                    backgroundClone.SetActive(false);
                }
                break;
            case 1: // Main Menu scene
                if (!backgroundClone.activeSelf)
                {
                    backgroundClone.SetActive(true);
                }
                break;
            case 0: // Game scene - should show background when it's the startup scene before Main Menu loads
                if (!backgroundClone.activeSelf)
                {
                    backgroundClone.SetActive(true);
                }
                break;
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
            for (int i = 1; i < audioListeners.Length; i++)
            {
                if (audioListeners[i] != null)
                {
                    audioListeners[i].enabled = false;
                }
            }
        }
    }
}
