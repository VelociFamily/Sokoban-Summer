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
            Debug.Log("[GameInitializer]: Background initialized");
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

    private void OnDestroy()
    {
        // Clean up input service when GameInitializer is destroyed
        InputService.Instance?.Dispose();
    }
}
