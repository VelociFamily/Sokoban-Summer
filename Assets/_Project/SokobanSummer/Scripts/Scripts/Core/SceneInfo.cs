using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    /// <summary>
    /// Component to be added to scenes to identify their type instead of using hardcoded build indices
    /// This should be attached to a GameObject in each scene to define what type of scene it is
    /// </summary>
    public class SceneInfo : MonoBehaviour
    {
        [Header("Scene Classification")]
        [Tooltip("Type of scene - determines background visibility and other scene-specific behaviors")]
        public SceneType sceneType = SceneType.MainMenu;
    
        [Tooltip("Whether this scene should show the background")]
        public bool showBackground = true;
    
        [Header("Level Information (for Tutorial/Gameplay levels)")]
        [Tooltip("Level number for progression tracking (only used for level scenes)")]
        public int levelNumber = 1;
        
        [Tooltip("Optional level data with metadata, goals, and display information")]
        public LevelData levelData;
    
        /// <summary>
        /// Get the SceneInfo for the currently active scene
        /// </summary>
        public static SceneInfo GetActiveSceneInfo()
        {
            // Look for SceneInfo in the active scene
            var sceneInfo = FindFirstObjectByType<SceneInfo>();
            return sceneInfo;
        }
    
        /// <summary>
        /// Find SceneInfo component in a specific scene
        /// </summary>
        /// <param name="scene">Scene to search in</param>
        /// <returns>SceneInfo component if found, null otherwise</returns>
        public static SceneInfo FindSceneInfoInScene(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.Log($"[SceneInfo]: FindSceneInfoInScene('{scene.name}') -> Scene invalid or not loaded");
                return null;
            }
            
            var rootObjects = scene.GetRootGameObjects();
            Debug.Log($"[SceneInfo]: FindSceneInfoInScene('{scene.name}') -> Searching {rootObjects.Length} root objects");
            foreach (var rootObj in rootObjects)
            {
                var sceneInfo = rootObj.GetComponentInChildren<SceneInfo>();
                if (sceneInfo != null)
                {
                    Debug.Log($"[SceneInfo]: FindSceneInfoInScene('{scene.name}') -> Found SceneInfo on '{rootObj.name}'");
                    return sceneInfo;
                }
            }
            return null;
        }
    
        /// <summary>
        /// Check if the active scene should show the background
        /// </summary>
        public static bool ShouldShowBackground()
        {
            var sceneInfo = GetActiveSceneInfo();
            if (sceneInfo != null)
            {
                return sceneInfo.showBackground;
            }
        
            // Fallback to old build index logic if no SceneInfo found
            var activeScene = SceneManager.GetActiveScene();
            return activeScene.buildIndex is 0 or 1; // Game or Main Menu scenes
        }
    
        /// <summary>
        /// Checks if the current active scene is the main menu
        /// </summary>
        /// <returns>True if current scene is main menu</returns>
        public static bool IsMainMenuScene()
        {
            var activeScene = SceneManager.GetActiveScene();
            return IsMainMenuScene(activeScene);
        }

        /// <summary>
        /// Checks if the specified scene is the main menu
        /// </summary>
        /// <param name="scene">Scene to check</param>
        /// <returns>True if scene is main menu</returns>
        public static bool IsMainMenuScene(Scene scene)
        {
            var sceneInfo = FindSceneInfoInScene(scene);
            if (sceneInfo != null)
            {
                bool isMenu = sceneInfo.sceneType == SceneType.MainMenu;
                Debug.Log($"[SceneInfo]: IsMainMenuScene('{scene.name}') -> SceneInfo found (type={sceneInfo.sceneType}), result={isMenu}");
                return isMenu;
            }
            // Fallback to build index logic
            bool fallback = scene.buildIndex == 0;
            Debug.Log($"[SceneInfo]: IsMainMenuScene('{scene.name}') -> No SceneInfo component; fallback buildIndex={scene.buildIndex}, result={fallback}");
            return fallback;
        }

        /// <summary>
        /// Checks if the current active scene is a gameplay scene (tutorial or game level)
        /// </summary>
        /// <returns>True if current scene is gameplay</returns>
        public static bool IsGameplayScene()
        {
            var activeScene = SceneManager.GetActiveScene();
            return IsGameplayScene(activeScene);
        }

        /// <summary>
        /// Checks if the specified scene is a gameplay scene (tutorial or game level)
        /// </summary>
        /// <param name="scene">Scene to check</param>
        /// <returns>True if scene is gameplay</returns>
        public static bool IsGameplayScene(Scene scene)
        {
            var sceneInfo = FindSceneInfoInScene(scene);
            if (sceneInfo != null)
            {
                bool isGameplay = sceneInfo.sceneType == SceneType.TutorialLevel || sceneInfo.sceneType == SceneType.GameplayLevel;
                Debug.Log($"[SceneInfo]: IsGameplayScene('{scene.name}') -> SceneInfo found (type={sceneInfo.sceneType}), result={isGameplay}");
                return isGameplay;
            }
        
            // Fallback to build index logic (levels 1-6)
            bool fallback = scene.buildIndex is >= 1 and <= 6;
            Debug.Log($"[SceneInfo]: IsGameplayScene('{scene.name}') -> No SceneInfo component; fallback buildIndex={scene.buildIndex}, result={fallback}");
            return fallback;
        }

        /// <summary>
        /// Checks if the specified scene is the final tutorial level (Level Two)
        /// </summary>
        /// <param name="scene">Scene to check</param>
        /// <returns>True if scene is Level Two (final tutorial)</returns>
        public static bool IsLevelTwo(Scene scene)
        {
            var sceneInfo = FindSceneInfoInScene(scene);
            if (sceneInfo != null)
            {
                // Check if it's specifically marked as Level Two or if it has a specific name/identifier
                // For now, we'll assume Level Two is a GameplayLevel - you might want to add more specific identification
                return sceneInfo.sceneType == SceneType.GameplayLevel;
            }
        
            // Fallback to build index logic (assuming scene 6 is Level Two)
            return scene.buildIndex == 6;
        }
    
        /// <summary>
        /// Get the scene type for a specific scene
        /// </summary>
        /// <param name="scene">Scene to check</param>
        /// <returns>SceneType of the scene</returns>
        public static SceneType GetSceneType(Scene scene)
        {
            var sceneInfo = FindSceneInfoInScene(scene);
            if (sceneInfo != null)
            {
                return sceneInfo.sceneType;
            }
        
            // Fallback to build index mapping
            return scene.buildIndex switch
            {
                0 => SceneType.Game,
                1 => SceneType.MainMenu,
                >= 2 and <= 7 => SceneType.TutorialLevel, // Assume tutorials first, then gameplay
                _ => SceneType.GameplayLevel
            };
        }
    
        /// <summary>
        /// Get the scene type of the active scene
        /// </summary>
        public static SceneType GetActiveSceneType()
        {
            var sceneInfo = GetActiveSceneInfo();
            if (sceneInfo != null)
            {
                return sceneInfo.sceneType;
            }
        
            // Fallback to build index mapping
            var activeScene = SceneManager.GetActiveScene();
            return activeScene.buildIndex switch
            {
                0 => SceneType.Game,
                1 => SceneType.MainMenu,
                >= 2 and <= 7 => SceneType.TutorialLevel, // Assume tutorials first, then gameplay
                _ => SceneType.GameplayLevel
            };
        }
    }
}
