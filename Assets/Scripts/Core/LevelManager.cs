using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    /// <summary>
    /// Manages dynamic level discovery, loading, and progression tracking
    /// Scans for scenes in the Levels folder and provides level management functionality
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Configuration")]
        [Tooltip("Path to the levels folder relative to Assets/Scenes/")]
        public string levelsFolder = "Levels";

        [Tooltip("Path to the tutorials folder relative to Assets/Scenes/")]
        public string tutorialsFolder = "Tutorials";

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;

        // Cached level information
        private List<LevelInfo> allLevels = new List<LevelInfo>();
        private List<LevelInfo> tutorialLevels = new List<LevelInfo>();
        private List<LevelInfo> gameplayLevels = new List<LevelInfo>();

        private bool levelsScanned = false;

        /// <summary>
        /// Information about a discovered level
        /// </summary>
        [System.Serializable]
        public class LevelInfo
        {
            public string sceneName;
            public string scenePath;
            public int buildIndex;
            public SceneType sceneType;
            public LevelData levelData;
            public int sortOrder;

            // Derived from scene or level data
            public string displayName;
            public Sprite previewImage;
            public int parMoves;
            public float parTime;
            public bool requiresUnlock;
        }

        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                ScanForLevels();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Scan for all levels in the configured folders
        /// </summary>
        public void ScanForLevels()
        {
            if (levelsScanned) return;

            allLevels.Clear();
            tutorialLevels.Clear();
            gameplayLevels.Clear();

            // Scan build settings for scenes
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                if (string.IsNullOrEmpty(scenePath)) continue;

                string sceneName = Path.GetFileNameWithoutExtension(scenePath);

                // Check if this scene is in our target folders
                bool isTutorial = scenePath.Contains($"/{tutorialsFolder}/");
                bool isLevel = scenePath.Contains($"/{levelsFolder}/");

                if (!isTutorial && !isLevel) continue;

                // Create level info
                var levelInfo = new LevelInfo
                {
                    sceneName = sceneName,
                    scenePath = scenePath,
                    buildIndex = i,
                    sceneType = isTutorial ? SceneType.TutorialLevel : SceneType.GameplayLevel
                };

                // Try to load level data from the scene (this would require the scene to be loaded)
                // For now, we'll use naming conventions and default values
                PopulateLevelInfoFromPath(levelInfo);

                allLevels.Add(levelInfo);

                if (isTutorial)
                    tutorialLevels.Add(levelInfo);
                else
                    gameplayLevels.Add(levelInfo);
            }

            // Sort levels by sort order
            SortLevels();
            levelsScanned = true;

            if (debugMode)
            {
                Debug.Log($"[LevelManager] Scanned {allLevels.Count} levels ({tutorialLevels.Count} tutorials, {gameplayLevels.Count} gameplay levels)");
                foreach (var level in allLevels)
                {
                    Debug.Log($"[LevelManager] Found level: {level.displayName} ({level.scenePath})");
                }
            }
        }

        /// <summary>
        /// Populate level info from scene path and naming conventions
        /// </summary>
        private void PopulateLevelInfoFromPath(LevelInfo levelInfo)
        {
            // Default display name from scene name
            levelInfo.displayName = levelInfo.sceneName;

            // Try to extract order from filename (e.g., "01_Tutorial", "Level One" -> 1)
            levelInfo.sortOrder = ExtractSortOrderFromName(levelInfo.sceneName);

            // Set default values
            levelInfo.requiresUnlock = false; // Default to unlocked for easier testing
            levelInfo.parMoves = 0;
            levelInfo.parTime = 0f;

            // Try to find a matching LevelData asset
            LoadLevelDataAsset(levelInfo);

            // Special handling for known tutorial names
            if (levelInfo.sceneType == SceneType.TutorialLevel)
            {
                switch (levelInfo.sceneName.ToLower())
                {
                    case "moving tutorial":
                        levelInfo.sortOrder = 0;
                        break;
                    case "button tutorial":
                        levelInfo.sortOrder = 1;
                        break;
                    case "speed tutorial":
                        levelInfo.sortOrder = 2;
                        break;
                    case "confuse tutorial":
                        levelInfo.sortOrder = 3;
                        break;
                }
            }
        }

        /// <summary>
        /// Try to load a LevelData asset that matches this level
        /// </summary>
        private void LoadLevelDataAsset(LevelInfo levelInfo)
        {
            // Try to find LevelData asset by name
            string[] possibleNames = {
                levelInfo.sceneName + "_Data",
                levelInfo.sceneName + " Data",
                levelInfo.sceneName.Replace(" ", "_") + "_Data",
                levelInfo.sceneName.Replace(" ", "") + "Data"
            };

            foreach (string assetName in possibleNames)
            {
                var levelData = Resources.Load<LevelData>(assetName);
                if (levelData != null)
                {
                    levelInfo.levelData = levelData;

                    // Override with data from asset
                    if (!string.IsNullOrEmpty(levelData.levelTitle))
                        levelInfo.displayName = levelData.levelTitle;

                    levelInfo.parMoves = levelData.parMoves;
                    levelInfo.parTime = levelData.parTime;
                    levelInfo.requiresUnlock = levelData.requiresUnlock;
                    levelInfo.previewImage = levelData.previewImage;

                    if (levelData.sortOrder > 0)
                        levelInfo.sortOrder = levelData.sortOrder;

                    if (debugMode)
                        Debug.Log($"[LevelManager] Loaded LevelData asset '{assetName}' for {levelInfo.sceneName}");

                    break;
                }
            }
        }

        /// <summary>
        /// Extract sort order from scene name using various patterns
        /// </summary>
        private int ExtractSortOrderFromName(string sceneName)
        {
            // Try to find numbers at the start of the name
            var parts = sceneName.Split(' ');

            // Pattern: "01_Name" or "1_Name"
            if (parts[0].Contains('_'))
            {
                var prefix = parts[0].Split('_')[0];
                if (int.TryParse(prefix, out int order))
                    return order;
            }

            // Pattern: "Level One", "Level Two", etc.
            if (sceneName.ToLower().Contains("one"))
                return 1;
            if (sceneName.ToLower().Contains("two"))
                return 2;
            if (sceneName.ToLower().Contains("three"))
                return 3;

            // Default: use build index as fallback
            return 999; // Put at end by default
        }

        /// <summary>
        /// Sort levels by their sort order
        /// </summary>
        private void SortLevels()
        {
            allLevels.Sort((a, b) => a.sortOrder.CompareTo(b.sortOrder));
            tutorialLevels.Sort((a, b) => a.sortOrder.CompareTo(b.sortOrder));
            gameplayLevels.Sort((a, b) => a.sortOrder.CompareTo(b.sortOrder));
        }

        /// <summary>
        /// Get all levels of a specific type
        /// </summary>
        public List<LevelInfo> GetLevels(SceneType sceneType)
        {
            if (!levelsScanned) ScanForLevels();

            return sceneType switch
            {
                SceneType.TutorialLevel => tutorialLevels,
                SceneType.GameplayLevel => gameplayLevels,
                _ => new List<LevelInfo>()
            };
        }

        /// <summary>
        /// Get all levels
        /// </summary>
        public List<LevelInfo> GetAllLevels()
        {
            if (!levelsScanned) ScanForLevels();
            return allLevels;
        }

        /// <summary>
        /// Get a level by build index
        /// </summary>
        public LevelInfo GetLevelByBuildIndex(int buildIndex)
        {
            if (!levelsScanned) ScanForLevels();
            return allLevels.FirstOrDefault(l => l.buildIndex == buildIndex);
        }

        /// <summary>
        /// Get a level by scene name
        /// </summary>
        public LevelInfo GetLevelByName(string sceneName)
        {
            if (!levelsScanned) ScanForLevels();
            return allLevels.FirstOrDefault(l => l.sceneName == sceneName);
        }

        /// <summary>
        /// Check if a level can be loaded (handles unlock logic)
        /// </summary>
        public bool CanLoadLevel(LevelInfo levelInfo)
        {
            if (levelInfo == null) return false;

            // If level doesn't require unlock, it's always available
            if (!levelInfo.requiresUnlock) return true;

            // For the first tutorial, it's always unlocked
            if (levelInfo.sceneType == SceneType.TutorialLevel && levelInfo.sortOrder == 0)
                return true;

            // For now, make all levels available for testing
            // TODO: Implement proper progression system based on level completion
            return true;
        }

        /// <summary>
        /// Load a level by its info (additive) and unload menu/previous level scenes.
        /// Keeps the base Game scene loaded so singletons persist (Input/Audio/MoveCounter).
        /// </summary>
        public void LoadLevel(LevelInfo levelInfo)
        {
            if (levelInfo == null || !CanLoadLevel(levelInfo))
            {
                Debug.LogWarning($"[LevelManager] Cannot load level: {levelInfo?.displayName ?? "null"}");
                return;
            }

            StartCoroutine(LoadLevelAdditiveRoutine(levelInfo));
        }

        private IEnumerator LoadLevelAdditiveRoutine(LevelInfo levelInfo)
        {
            // Prevent duplicate loads
            Debug.Log($"[LevelManager] Loading level additively: {levelInfo.displayName} (buildIndex {levelInfo.buildIndex})");

            // Load target scene additively
            var async = SceneManager.LoadSceneAsync(levelInfo.buildIndex, LoadSceneMode.Additive);
            while (!async.isDone) yield return null;

            // Get the loaded scene and set active
            var loadedScene = SceneManager.GetSceneByBuildIndex(levelInfo.buildIndex);
            if (loadedScene.IsValid())
            {
                SceneManager.SetActiveScene(loadedScene);
                Debug.Log($"[LevelManager] Active scene set: {loadedScene.name}");
            }
            else
            {
                Debug.LogWarning($"[LevelManager] Loaded scene not valid for index {levelInfo.buildIndex}");
            }

            // Unload Main Menu if loaded (detect by name or path)
            var mainMenuScene = SceneManager.GetSceneByName("Main Menu");
            if (!mainMenuScene.IsValid())
            {
                mainMenuScene = SceneManager.GetSceneByPath("Assets/Scenes/Main Menu.unity");
            }
            if (mainMenuScene.IsValid() && mainMenuScene.isLoaded)
            {
                var unload = SceneManager.UnloadSceneAsync(mainMenuScene);
                while (unload != null && !unload.isDone) yield return null;
                Debug.Log("[LevelManager] Unloaded Main Menu scene after level load");
            }

            // Unload any other loaded level/tutorial scenes (avoid accumulating multiple level scenes)
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scn = SceneManager.GetSceneAt(i);
                if (!scn.isLoaded) continue;
                if (scn.buildIndex == 0) continue; // keep base Game scene
                if (scn.buildIndex == levelInfo.buildIndex) continue; // keep current level

                // Heuristic: unload if it's under Tutorials or Levels folder
                if (scn.path.Contains("/Scenes/Tutorials/") || scn.path.Contains("/Scenes/Levels/"))
                {
                    var u = SceneManager.UnloadSceneAsync(scn);
                    while (u != null && !u.isDone) yield return null;
                    Debug.Log($"[LevelManager] Unloaded previous level scene: {scn.name}");
                }
            }

            // Optional: free memory
            yield return Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// Load Main Menu additively and unload all gameplay/tutorial level scenes.
        /// Keeps the base Game scene loaded for singletons.
        /// </summary>
        public void LoadMainMenu()
        {
            StartCoroutine(LoadMainMenuRoutine());
        }

        private IEnumerator LoadMainMenuRoutine()
        {
            var load = SceneManager.LoadSceneAsync("Main Menu", LoadSceneMode.Additive);
            while (!load.isDone) yield return null;

            var menuScene = SceneManager.GetSceneByName("Main Menu");
            if (menuScene.IsValid())
            {
                SceneManager.SetActiveScene(menuScene);
            }

            // Unload any level scenes
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scn = SceneManager.GetSceneAt(i);
                if (!scn.isLoaded) continue;
                if (scn.buildIndex == 0) continue; // keep base Game scene
                if (scn.name == "Main Menu") continue; // keep menu

                if (scn.path.Contains("/Scenes/Tutorials/") || scn.path.Contains("/Scenes/Levels/"))
                {
                    var u = SceneManager.UnloadSceneAsync(scn);
                    while (u != null && !u.isDone) yield return null;
                }
            }

            yield return Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// Force rescan of levels (useful for development)
        /// </summary>
        [ContextMenu("Rescan Levels")]
        public void RescanLevels()
        {
            levelsScanned = false;
            ScanForLevels();
        }

        /// <summary>
        /// Get the next level in sequence
        /// </summary>
        public LevelInfo GetNextLevel(LevelInfo currentLevel)
        {
            if (currentLevel == null) return null;

            var levelList = currentLevel.sceneType == SceneType.TutorialLevel ? tutorialLevels : gameplayLevels;
            var currentIndex = levelList.FindIndex(l => l.buildIndex == currentLevel.buildIndex);

            if (currentIndex >= 0 && currentIndex < levelList.Count - 1)
                return levelList[currentIndex + 1];

            // If we're at the end of tutorials, move to first gameplay level
            if (currentLevel.sceneType == SceneType.TutorialLevel && gameplayLevels.Count > 0)
                return gameplayLevels[0];

            return null;
        }

        /// <summary>
        /// Mark a level as completed and unlock the next level
        /// </summary>
        public void MarkLevelCompleted(int buildIndex)
        {
            var level = GetLevelByBuildIndex(buildIndex);
            if (level != null)
            {
                // Store completion in PlayerPrefs
                PlayerPrefs.SetInt($"Level_{buildIndex}_Completed", 1);
                PlayerPrefs.Save();

                Debug.Log($"[LevelManager] Level {level.displayName} marked as completed");

                var nextLevel = GetNextLevel(level);
                if (nextLevel != null)
                {
                    Debug.Log($"[LevelManager] Next level {nextLevel.displayName} is now available");
                }
            }
        }
        /// <summary>
        /// Check if a level is completed
        /// </summary>
        public bool IsLevelCompleted(int buildIndex)
        {
            return PlayerPrefs.GetInt($"Level_{buildIndex}_Completed", 0) == 1;
        }

        /// <summary>
        /// Check if a level is completed by level info
        /// </summary>
        public bool IsLevelCompleted(LevelInfo levelInfo)
        {
            return levelInfo != null && IsLevelCompleted(levelInfo.buildIndex);
        }

        /// <summary>
        /// Get completion stats for a level
        /// </summary>
        public void GetLevelStats(int buildIndex, out int bestMoves, out float bestTime)
        {
            bestMoves = PlayerPrefs.GetInt($"Level_{buildIndex}_BestMoves", 0);
            bestTime = PlayerPrefs.GetFloat($"Level_{buildIndex}_BestTime", 0f);
        }

        /// <summary>
        /// Save level completion with stats
        /// </summary>
        public void SaveLevelStats(int buildIndex, int moves, float time)
        {
            // Save completion
            PlayerPrefs.SetInt($"Level_{buildIndex}_Completed", 1);

            // Save best moves (if better than previous or first completion)
            int currentBest = PlayerPrefs.GetInt($"Level_{buildIndex}_BestMoves", 0);
            if (currentBest == 0 || moves < currentBest)
            {
                PlayerPrefs.SetInt($"Level_{buildIndex}_BestMoves", moves);
            }

            // Save best time (if better than previous or first completion)
            float currentBestTime = PlayerPrefs.GetFloat($"Level_{buildIndex}_BestTime", 0f);
            if (currentBestTime == 0f || time < currentBestTime)
            {
                PlayerPrefs.SetFloat($"Level_{buildIndex}_BestTime", time);
            }

            PlayerPrefs.Save();

            var level = GetLevelByBuildIndex(buildIndex);
            if (level != null)
            {
                Debug.Log($"[LevelManager] Stats saved for {level.displayName}: {moves} moves, {time:F1}s");
            }
        }
    }
}
