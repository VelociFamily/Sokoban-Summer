using Core;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace UI
{
    public class CompleteUI : MonoBehaviour
    {
        private AchievementManager achievementManager;
        private InputService inputService;
        private MenuNavigator menuNavigator;

        private void Awake()
        {
            // Resolve services defensively: these may not be registered in the locator
            ServiceLocator.TryGet(out achievementManager);
            ServiceLocator.TryGet(out inputService);

            if (!ServiceLocator.TryGet(out menuNavigator))
            {
                menuNavigator = FindFirstObjectByType<MenuNavigator>();
                if (menuNavigator == null)
                {
                    Debug.LogWarning("[CompleteUI]: MenuNavigator not found in ServiceLocator or scene");
                }
            }
        }

        public void LoadNextScene()
        {
            var currentScene = SceneManager.GetActiveScene();

            try
            {
                // Call achievement unlock logic based on current level completion
                UnlockNextLevel(currentScene);

                if (!ServiceLocator.TryGet<LevelManager>(out var levelManager))
                {
                    Debug.LogError("[CompleteUI]: LevelManager not found in ServiceLocator");
                    return;
                }

                // Use LevelManager's GetNextLevel for proper progression logic
                var currentLevel = levelManager.GetLevelByBuildIndex(currentScene.buildIndex);
                if (currentLevel == null)
                {
                    Debug.LogWarning($"[CompleteUI]: Current level with build index {currentScene.buildIndex} not found in LevelManager");
                    return;
                }

                var nextLevel = levelManager.GetNextLevel(currentLevel);
                if (nextLevel != null)
                {
                    Debug.Log($"[CompleteUI]: Loading next level: {nextLevel.displayName}");
                    levelManager.LoadLevel(nextLevel);
                }
                else
                {
                    Debug.Log($"[CompleteUI]: No next level after {currentLevel.displayName} - returning to main menu");
                    LoadMenu();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[CompleteUI]: Error loading next scene: {e.Message}");
            }
        }

        /// <summary>
        /// Unlock the next level based on the current scene being completed
        /// </summary>
        /// <param name="completedScene">The scene that was just completed</param>
        private void UnlockNextLevel(Scene completedScene)
        {
            if (achievementManager == null)
            {
                Debug.LogWarning("[CompleteUI]: AchievementManager instance not found - cannot unlock levels");
                return;
            }

            var sceneInfo = SceneInfo.FindSceneInfoInScene(completedScene);

            // Use SceneInfo exclusively - all scenes should have SceneInfo per architecture guidelines
            if (sceneInfo != null)
            {
                switch (sceneInfo.sceneType)
                {
                    case SceneType.TutorialLevel:
                        if (sceneInfo.levelNumber == 1) // First tutorial level
                        {
                            Debug.Log("[CompleteUI]: Tutorial completed - unlocking Confuse and Speed power-ups");
                            achievementManager.UnlockConfuseAndSpeed();
                        }
                        break;

                    case SceneType.GameplayLevel:
                        if (sceneInfo.levelNumber == 2) // Level Two
                        {
                            Debug.Log("[CompleteUI]: Level Two completed - unlocking next achievement");
                            achievementManager.UnlockLevelTwo();
                        }
                        break;
                }
            }
            else
            {
                // Warn if SceneInfo is missing - all scenes should have it
                Debug.LogWarning($"[CompleteUI]: Scene '{completedScene.name}' is missing SceneInfo component. Add SceneInfo to all level scenes per architecture guidelines.");
            }
        }

        public async void LoadMenu()
        {
            Time.timeScale = 1f;

            // Ensure input is in UI mode during menu navigation
            inputService?.DisablePlayerInput();
            inputService?.EnableUIInput();

            // Delegate to LevelManager to properly unload gameplay scenes and load main menu
            if (ServiceLocator.TryGet<LevelManager>(out var levelManager))
            {
                Debug.Log("[CompleteUI]: Using LevelManager to load main menu");
                levelManager.LoadMainMenu();
            }
            else
            {
                // Fallback: manually unload gameplay scenes if LevelManager isn't available
                Debug.LogWarning("[CompleteUI]: LevelManager not found, using fallback scene unload logic");
                var unloadOps = new List<AsyncOperation>();
                for (var i = 0; i < SceneManager.sceneCount; i++)
                {
                    var loadedScene = SceneManager.GetSceneAt(i);
                    if (SceneInfo.IsGameplayScene(loadedScene) && loadedScene.isLoaded)
                    {
                        var op = SceneManager.UnloadSceneAsync(loadedScene);
                        if (op != null) unloadOps.Add(op);
                    }
                }
                foreach (var op in unloadOps)
                {
                    while (!op.isDone) { await Task.Yield(); }
                }
            }

            // If the persistent UI was hidden for gameplay, ensure it is visible again
            if (PersistentUIManager.Exists)
            {
                PersistentUIManager.Show(animated: false);
            }

            // After unloading gameplay, ensure a non-gameplay scene is active
            Scene? target = null;
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var s = SceneManager.GetSceneAt(i);
                if (s.isLoaded && !SceneInfo.IsGameplayScene(s)) { target = s; break; }
            }
            if (target.HasValue)
            {
                SceneManager.SetActiveScene(target.Value);
            }

            // Show Main Menu panel via MenuNavigator
            if (menuNavigator == null)
            {
                ServiceLocator.TryGet(out menuNavigator);
                if (menuNavigator == null)
                {
                    menuNavigator = FindFirstObjectByType<MenuNavigator>();
                }
            }

            if (menuNavigator != null)
            {
                Debug.Log("[CompleteUI]: Requesting Main Menu show via MenuNavigator");
                menuNavigator.ShowPanel("Main Menu", instant: true);
            }
        }
    }
}