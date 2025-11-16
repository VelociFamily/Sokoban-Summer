using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    public class GameInitializer : MonoBehaviour
    {
        [Header("Prefab References")]
        public GameObject Background;

        [Header("Modern Audio System")]
    public UnityEngine.Object UnifiedAudioManagerPrefab; // Use UnityEngine.Object for prefab references

        [Header("Other Systems")]
        public LevelLogger LevelLogger;
        public MoveCounter MoveCounterPrefab;
        public GameObject MusicPlayer;
        public SceneInfo SceneInfo;

        [Header("Splash Screen")]
        [Tooltip("Optional prefab for the startup splash. When empty, a simple default splash is generated at runtime.")]
        public GameObject SplashScreenPrefab;

        [Min(0f)]
        [Tooltip("Seconds to keep the splash visible before fading.")]
        public float SplashHoldDuration = 2f;

        [Min(0f)]
        [Tooltip("Seconds the splash takes to fade out.")]
        public float SplashFadeDuration = 1f;

        [Tooltip("Title text displayed on the splash when using the default generated layout.")]
        public string SplashTitleText = "Title";

        [Header("Foreground Ambient FX")]
        [Tooltip("Looping wind VFX prefab to display in gameplay scenes.")]
        public GameObject WindForegroundEffectPrefab;

        [Tooltip("Position offset for the wind effect relative to the camera origin.")]
        public Vector3 WindEffectOffset = new Vector3(0f, 0f, 0f);

        [Tooltip("Leaf burst VFX prefab to spawn periodically in gameplay scenes.")]
        public GameObject LeafBurstEffectPrefab;

    [Tooltip("Optional collection of leaf VFX prefabs that will be chosen at random when spawning.")]
    public List<GameObject> LeafBurstEffectPrefabs = new List<GameObject>();

        [Tooltip("Central point for spawning leaf bursts.")]
        public Vector3 LeafEffectPivot = new Vector3(0f, 2f, 0f);

        [Tooltip("Width (x) and height (y) of the rectangle to randomize leaf burst positions.")]
        public Vector2 LeafSpawnArea = new Vector2(18f, 6f);

        [Tooltip("Minimum and maximum delay between leaf bursts.")]
    public Vector2 LeafSpawnIntervalRange = new Vector2(0.35f, 0.85f);

        [Tooltip("Lifetime assigned to spawned leaf bursts (seconds).")]
        public float LeafBurstLifetime = 12f;

    [Tooltip("Sorting layer used by the foreground ambient effects.")]
    public string ForegroundSortingLayer = "Foreground";

    [Tooltip("Sorting order used by the foreground ambient effects.")]
    public int ForegroundSortingOrder = 500;

            private GameObject backgroundClone;
            private ForegroundAmbientManager foregroundAmbientManager;

        // Service instances managed by this initializer
        private ModernAudioService _audioService;
        private InputService _inputService;
        private UIService _uiService;
        private MoveCounter _moveCounter;
        private AchievementManager _achievementManager;
        private LevelManager _levelManager;
        private LevelLogger _levelLogger;

        private async void Start()
        {
            try
            {
                Debug.Log("[GameInitializer]: Starting async game initialization...");

                if (_servicesStarted)
                {
                    Debug.Log("[GameInitializer]: Services already started - skipping duplicate startup.");
                }

                // Step 1: Initialize background first (like original timing)
                await InitializeBackgroundAsync();

                // Step 2: Initialize music player (needed before VolumeControl)
                await InitializeMusicPlayerAsync();

                // Step 3: Initialize core systems in parallel (guarded)
                if (!_servicesStarted)
                {
                    await InitializeCoreSystemsAsync();
                    _servicesStarted = true;
                }

                // Step 4: Initialize remaining scene-specific systems
                await InitializeLevelLoggerAsync();

                // Step 4b: Prepare ambient foreground effects (if configured)
                await InitializeForegroundEffectsAsync();

                // Step 5: Load the main menu scene so its camera becomes available
                await LoadMainMenuAsync();

                // Step 6: Show splash/title screen (UIService already handled camera resolution)
                await ShowSplashScreenAsync();

                Debug.Log("[GameInitializer]: Game initialization completed successfully");
            }
            catch (Exception exception)
            {
                Debug.LogError($"[GameInitializer]: Error during game initialization: {exception}");
            }
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        private void Awake()
        {
            // Ensure a DevBuildIndicator is present in dev/editor builds
            if (FindFirstObjectByType<global::Core.DevBuildIndicator>() == null)
            {
                var go = new GameObject("DevBuildIndicator");
                go.AddComponent<global::Core.DevBuildIndicator>();
                DontDestroyOnLoad(go);
            }
        }
#endif

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
                Debug.LogError(
                    "[GameInitializer]: Music Player prefab is null - background music will not play. Check GameInitializer prefab assignments!");

            await Task.Yield();
        }

        /// <summary>
        /// Initialize core systems asynchronously in parallel for better performance
        /// </summary>
        private async Task InitializeCoreSystemsAsync()
        {
            Debug.Log("[GameInitializer]: Initializing core systems...");

            // Initialize persistence (synchronous/lightweight) BEFORE starting parallel tasks
            Core.SaveFacade.Instance.InitializeAndMaybeMigrate();

            // Start all async initializations in parallel
            var initTasks = new List<Task>
            {
                InitializeAudioSystemAsync(),
                InitializeInputSystemAsync(),
                InitializeUIServiceAsync(),
                InitializeAchievementSystemAsync(),
                InitializeMoveCounterAsync(),
                InitializeLevelManagerAsync()
            };

            // Wait for all core systems to initialize
            await Task.WhenAll(initTasks);

            // Register all services in ServiceLocator after initialization
            RegisterServicesInLocator();

            Debug.Log("[GameInitializer]: Core systems initialized and registered in ServiceLocator");
        }

        /// <summary>
        /// Register all initialized services in the ServiceLocator
        /// </summary>
        private void RegisterServicesInLocator()
        {
            if (_audioService != null) ServiceLocator.Register(_audioService);
            if (_inputService != null) ServiceLocator.Register(_inputService);
            if (_uiService != null) ServiceLocator.Register(_uiService);
            if (_moveCounter != null) ServiceLocator.Register(_moveCounter);
            if (_achievementManager != null) ServiceLocator.Register(_achievementManager);
            if (_levelManager != null) ServiceLocator.Register(_levelManager);
            if (_levelLogger != null) ServiceLocator.Register(_levelLogger);

            Debug.Log($"[GameInitializer]: {ServiceLocator.Count} services registered in ServiceLocator");
        }

        /// <summary>
        /// Initialize audio systems asynchronously
        /// </summary>
        private async Task InitializeAudioSystemAsync()
        {
            // Create ModernAudioService instance
            _audioService = new ModernAudioService();
            
            // Use modern audio system if available, otherwise fallback to legacy
            if (UnifiedAudioManagerPrefab != null)
            {
                await _audioService.InitializeWithPrefabAsync(UnifiedAudioManagerPrefab);
            }
        }

        /// <summary>
        /// Initialize input systems asynchronously
        /// </summary>
        private async Task InitializeInputSystemAsync()
        {
            // Create InputService instance
            _inputService = new InputService();
            await _inputService.InitializeAsync();
        }

        /// <summary>
        /// Initialize UI systems asynchronously
        /// </summary>
        private async Task InitializeUIServiceAsync()
        {
            // Create UIService instance
            _uiService = new UIService();
            await _uiService.InitializeAsync();
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
                _achievementManager = achievementManagerObj;
                await achievementManagerObj.InitializeAsync();
                Debug.Log("[GameInitializer]: AchievementManager initialized");
            }
            else
            {
                // Try to create an AchievementManager if none exists
                var achievementManagerGameObject = new GameObject("AchievementManager");
                _achievementManager = achievementManagerGameObject.AddComponent<AchievementManager>();
                await _achievementManager.InitializeAsync();
                Debug.Log("[GameInitializer]: AchievementManager created and initialized");
            }
        }

        /// <summary>
        /// Initialize LevelManager system asynchronously
        /// </summary>
        private async Task InitializeLevelManagerAsync()
        {
            // Check if LevelManager already exists in the scene
            var existingLevelManager = FindFirstObjectByType<LevelManager>();
            if (existingLevelManager != null)
            {
                _levelManager = existingLevelManager;
                Debug.Log("[GameInitializer]: LevelManager already exists in scene");
                await Task.Yield();
                return;
            }

            // Create LevelManager if it doesn't exist
            var levelManagerGameObject = new GameObject("LevelManager");
            _levelManager = levelManagerGameObject.AddComponent<LevelManager>();
            Debug.Log("[GameInitializer]: LevelManager created");

            await Task.Yield();
        }

        /// <summary>
        /// Initialize MoveCounter system asynchronously
        /// </summary>
        private async Task InitializeMoveCounterAsync()
        {
            // Check if MoveCounter already exists in the scene
            var existingMoveCounter = FindFirstObjectByType<MoveCounter>();
            if (existingMoveCounter != null)
            {
                _moveCounter = existingMoveCounter;
                Debug.Log("[GameInitializer]: MoveCounter already exists in scene");
                await Task.Yield();
                return;
            }

            // Create MoveCounter from prefab if available, otherwise create empty one
            if (MoveCounterPrefab != null)
            {
                var moveCounterClone = Instantiate(MoveCounterPrefab);
                _moveCounter = moveCounterClone;
                Debug.Log($"[GameInitializer]: MoveCounter '{moveCounterClone.name}' created from prefab");
            }
            else
            {
                // Create a basic MoveCounter if no prefab is assigned
                var moveCounterGameObject = new GameObject("MoveCounter");
                _moveCounter = moveCounterGameObject.AddComponent<MoveCounter>();
                Debug.Log("[GameInitializer]: MoveCounter created programmatically - UI components will need to be assigned in scenes");
            }

            await Task.Yield();
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
                Debug.LogError(
                    "[GameInitializer]: Background prefab is null - cannot initialize background. Check GameInitializer prefab assignments!");

            await Task.Yield();
        }

        /// <summary>
        /// Initialize level logger asynchronously
        /// </summary>
        private async Task InitializeLevelLoggerAsync()
        {
            if (LevelLogger != null)
            {
                _levelLogger = Instantiate(LevelLogger);
                Debug.Log("[GameInitializer]: LevelLogger initialized");
            }
            await Task.Yield();
        }

        /// <summary>
        /// Initialize the foreground ambient effect manager if prefabs are provided.
        /// </summary>
        private async Task InitializeForegroundEffectsAsync()
        {
            var leafPrefabsBuffer = new List<GameObject>();
            if (LeafBurstEffectPrefabs != null)
            {
                foreach (var prefab in LeafBurstEffectPrefabs)
                {
                    if (prefab != null && !leafPrefabsBuffer.Contains(prefab))
                    {
                        leafPrefabsBuffer.Add(prefab);
                    }
                }
            }

            if (leafPrefabsBuffer.Count == 0 && LeafBurstEffectPrefab != null)
            {
                leafPrefabsBuffer.Add(LeafBurstEffectPrefab);
            }

            if (WindForegroundEffectPrefab == null && leafPrefabsBuffer.Count == 0)
            {
                await Task.Yield();
                return;
            }

            if (foregroundAmbientManager == null)
            {
                var managerObject = new GameObject("ForegroundAmbientManager");
                foregroundAmbientManager = managerObject.AddComponent<ForegroundAmbientManager>();
            }

            foregroundAmbientManager.Initialize(
                WindForegroundEffectPrefab,
                leafPrefabsBuffer,
                WindEffectOffset,
                LeafEffectPivot,
                LeafSpawnArea,
                LeafSpawnIntervalRange,
                LeafBurstLifetime,
                ForegroundSortingLayer,
                ForegroundSortingOrder);

            await Task.Yield();
        }

        /// <summary>
        /// Display a splash/title screen for a short duration before continuing initialization.
        /// </summary>
        private async Task ShowSplashScreenAsync()
        {
            CoreShared.ISplashScreenController controller = null;
            GameObject splashInstance = null;

            if (SplashScreenPrefab != null)
            {
                splashInstance = Instantiate(SplashScreenPrefab);
                controller = splashInstance.GetComponent<CoreShared.ISplashScreenController>();
                if (controller == null)
                {
                    Debug.LogWarning("[GameInitializer]: Splash prefab instantiated but does not implement ISplashScreenController; skipping splash sequence.");
                }
            }
            else
            {
                Debug.Log("[GameInitializer]: No splash screen prefab assigned; skipping generated splash (no UI available in Core). Consider providing a prefab implementing ISplashScreenController.");
            }

            if (controller != null)
            {
                controller.SetTitle(SplashTitleText);
                await controller.PlaySequenceAsync(SplashHoldDuration, SplashFadeDuration);
                Debug.Log("[GameInitializer]: Splash screen sequence completed");
            }
            else
            {
                // No controller available; wait briefly to preserve startup timing
                await Task.Delay((int)((SplashHoldDuration + SplashFadeDuration) * 1000f));
                Debug.Log("[GameInitializer]: No splash controller available; continuing without splash.");
            }
        }

        /// <summary>
        /// Load main menu scene asynchronously
        /// </summary>
        private static async Task LoadMainMenuAsync()
        {
            Debug.Log("[GameInitializer]: Loading Main Menu scene...");
            await SceneManager.LoadSceneAsync("Main Menu", LoadSceneMode.Additive);
            Debug.Log("[GameInitializer]: Main Menu scene loaded");
        }

        /// <summary>
        /// Wait for a camera to exist so UI canvases can target it.
        /// NOTE: This method is now deprecated in favor of UIService.InitializeAsync().
        /// Kept for backward compatibility in case it's called elsewhere.
        /// </summary>
        [Obsolete("Use UIService.InitializeAsync() instead - camera resolution is handled there")]
        private static async Task WaitForPrimaryCameraAsync(float timeoutSeconds = 5f)
        {
            var startTime = Time.realtimeSinceStartup;

            while (Time.realtimeSinceStartup - startTime < timeoutSeconds)
            {
                var camera = Camera.main ?? FindFirstObjectByType<Camera>();
                if (camera != null)
                {
                    Debug.Log($"[GameInitializer]: Primary camera '{camera.name}' detected; proceeding with splash.");
                    return;
                }

                await Task.Yield();
            }

            Debug.LogWarning("[GameInitializer]: Timed out waiting for a primary camera. Proceeding without explicit camera binding.");
        }

        private void Update()
        {
            if (backgroundClone == null)
                return;

            // Use the new SceneInfo system instead of hardcoded build indices
            var shouldShow = SceneInfo.ShouldShowBackground();

            if (backgroundClone.activeSelf != shouldShow)
            {
                backgroundClone.SetActive(shouldShow);
            }
        }

        private void OnDestroy()
        {
            // Coordinated teardown in reverse init order
            try
            {
                Debug.Log("[GameInitializer]: Beginning coordinated service teardown...");
                _achievementManager?.Shutdown();
                _moveCounter?.Shutdown();
                _audioService?.Shutdown();
                _inputService?.Shutdown();
                
                // Clear ServiceLocator
                ServiceLocator.Clear();
            }
            finally
            {
                _servicesStarted = false;
                Debug.Log("[GameInitializer]: Service teardown completed.");
            }
        }

        // Track whether services have already been started (prevents duplication on play-mode reentry)
        private static bool _servicesStarted;
    }
}
