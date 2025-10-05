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
        public Audio.UnifiedAudioManager UnifiedAudioManagerPrefab;

        [Header("Other Systems")]
        public LevelLogger LevelLogger;
        public MoveCounter MoveCounterPrefab;
        public GameObject MusicPlayer;
        public SceneInfo SceneInfo;

        private GameObject backgroundClone;

        private async void Start()
        {
            try
            {
                Debug.Log("[GameInitializer]: Starting async game initialization...");

                // Step 1: Initialize background first (like original timing)
                await InitializeBackgroundAsync();

                // Step 2: Initialize music player (needed before VolumeControl)
                await InitializeMusicPlayerAsync();

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
                InitializeAchievementSystemAsync(),
                InitializeMoveCounterAsync()
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
            // Use modern audio system if available, otherwise fallback to legacy
            if (UnifiedAudioManagerPrefab != null)
            {
                await ModernAudioService.Instance.InitializeWithPrefabAsync(UnifiedAudioManagerPrefab);
            }
        }

        /// <summary>
        /// Initialize input systems asynchronously
        /// </summary>
        private static async Task InitializeInputSystemAsync()
        {
            await InputService.Instance.InitializeAsync();
        }

        /// <summary>
        /// Initialize achievement system asynchronously
        /// </summary>
        private static async Task InitializeAchievementSystemAsync()
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
                // Try to create an AchievementManager if none exists
                var achievementManagerGameObject = new GameObject("AchievementManager");
                var achievementManager = achievementManagerGameObject.AddComponent<AchievementManager>();
                await achievementManager.InitializeAsync();
                Debug.Log("[GameInitializer]: AchievementManager created and initialized");
            }
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
                Debug.Log("[GameInitializer]: MoveCounter already exists in scene");
                await Task.Yield();
                return;
            }

            // Create MoveCounter from prefab if available, otherwise create empty one
            if (MoveCounterPrefab != null)
            {
                var moveCounterClone = Instantiate(MoveCounterPrefab);
                Debug.Log($"[GameInitializer]: MoveCounter '{moveCounterClone.name}' created from prefab");
            }
            else
            {
                // Create a basic MoveCounter if no prefab is assigned
                var moveCounterGameObject = new GameObject("MoveCounter");
                var moveCounter = moveCounterGameObject.AddComponent<MoveCounter>();
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
                Instantiate(LevelLogger);
                Debug.Log("[GameInitializer]: LevelLogger initialized");
            }
            await Task.Yield();
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
            // Clean up input service when GameInitializer is destroyed
            InputService.Instance?.Dispose();
        }
    }
}
