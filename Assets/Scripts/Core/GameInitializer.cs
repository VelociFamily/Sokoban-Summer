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
    public AudioSource AudioSource;
    public LevelLogger LevelLogger;

    private GameObject backgroundClone;
    private AchievementManager achievementManager;
    private List<IAsyncInitializable> asyncInitializables = new List<IAsyncInitializable>();

    private async void Start()
    {
        try
        {
            Debug.Log("[GameInitializer]: Starting async game initialization...");
            
            // Initialize core systems asynchronously in parallel where possible
            await InitializeCoreSystemsAsync();
            
            // Initialize scene-specific systems
            await InitializeSceneSystemsAsync();
            
            // Load main menu scene
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
        await AudioService.Instance.InitializeWithPrefabsAsync(VolumeControl, SFXVolumeControl, AudioSource);
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
        var achievementManagerObj = FindObjectOfType<AchievementManager>();
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
    /// Initialize scene-specific systems
    /// </summary>
    private async Task InitializeSceneSystemsAsync()
    {
        Debug.Log("[GameInitializer]: Initializing scene systems...");
        
        // Initialize background
        await InitializeBackgroundAsync();
        
        // Initialize level logger
        await InitializeLevelLoggerAsync();
        
        Debug.Log("[GameInitializer]: Scene systems initialized");
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
            
            Debug.Log($"[GameInitializer]: Background initialized - GameObject: '{backgroundClone.name}', Active: {backgroundClone.activeSelf}");
            
            // Ensure background is visible initially (it should be managed by Update() later)
            backgroundClone.SetActive(true);
            Debug.Log($"[GameInitializer]: Background set to active: {backgroundClone.activeSelf}");
        }
        else
        {
            Debug.LogWarning("[GameInitializer]: Background prefab is null - cannot initialize background");
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
        
        // Set Main Menu as the active scene after loading
        var mainMenuScene = SceneManager.GetSceneByName("Main Menu");
        if (mainMenuScene.IsValid())
        {
            SceneManager.SetActiveScene(mainMenuScene);
            Debug.Log($"[GameInitializer]: Main Menu scene loaded and set as active. Active scene: '{SceneManager.GetActiveScene().name}'");
        }
        else
        {
            Debug.LogError("[GameInitializer]: Failed to find Main Menu scene after loading");
        }
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
                    Debug.Log($"[GameInitializer]: Background hidden for scene '{activeScene.name}' (build index: {activeScene.buildIndex})");
                }
                break;
            case 1: // Main Menu scene
                if (!backgroundClone.activeSelf)
                {
                    backgroundClone.SetActive(true);
                    Debug.Log($"[GameInitializer]: Background shown for scene '{activeScene.name}' (build index: {activeScene.buildIndex})");
                }
                break;
            case 0: // Game scene - should show background when it's the startup scene before Main Menu loads
                if (!backgroundClone.activeSelf)
                {
                    backgroundClone.SetActive(true);
                    Debug.Log($"[GameInitializer]: Background shown for scene '{activeScene.name}' (build index: {activeScene.buildIndex})");
                }
                break;
            default:
                Debug.Log($"[GameInitializer]: No background rules defined for scene '{activeScene.name}' (build index: {activeScene.buildIndex})");
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
            Debug.LogWarning("[GameInitializer]: No AudioListener found in scene - creating temporary one");
            
            // Create a temporary GameObject with AudioListener to prevent Unity warnings
            var tempAudioListenerObject = new GameObject("TempAudioListener");
            tempAudioListenerObject.AddComponent<AudioListener>();
            
            Debug.Log("[GameInitializer]: Temporary AudioListener created - will be managed by scene loading system");
        }
        else
        {
            Debug.Log($"[GameInitializer]: Found {audioListeners.Length} AudioListener(s) in scene");
        }
    }

    private void OnDestroy()
    {
        // Clean up input service when GameInitializer is destroyed
        InputService.Instance?.Dispose();
    }
}
