using Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class CompleteUI : MonoBehaviour
    {
        private AchievementManager achievementManager;
        private MenuNavigator menuNavigator;

        private void Awake()
        {
            // Resolve services defensively: these may not be registered in the locator
            ServiceLocator.TryGet(out achievementManager);

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
            var currentSceneIndex = currentScene.buildIndex;
            var nextSceneIndex = currentSceneIndex + 1;

            try
            {
                // Call achievement unlock logic based on current level completion
                UnlockNextLevel(currentScene);

                var levelManager = ServiceLocator.Get<LevelManager>();
                var nextLevel = levelManager.GetLevelByBuildIndex(nextSceneIndex);
                if (nextLevel != null)
                {
                    // Delegate to LevelManager additive loader
                    levelManager.LoadLevel(nextLevel);
                }
                else
                {
                    Debug.LogWarning($"[CompleteUI]: Next level with index {nextSceneIndex} not found in LevelManager");
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

            // Use SceneInfo if available, otherwise fallback to build index
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
                // Fallback to build index logic for scenes without SceneInfo
                switch (completedScene.buildIndex)
                {
                    case 2: // Tutorial level (build index 2)
                        Debug.Log("[CompleteUI]: Tutorial completed - unlocking Confuse and Speed power-ups");
                        achievementManager.UnlockConfuseAndSpeed();
                        break;

                    case 6: // Level Two (build index 6)
                        Debug.Log("[CompleteUI]: Level Two completed - unlocking next achievement");
                        achievementManager.UnlockLevelTwo();
                        break;
                }
            }
        }

        public void LoadMenu()
        {
            Time.timeScale = 1f;

            // Unload any loaded gameplay scenes (tutorial or game levels)
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var loadedScene = SceneManager.GetSceneAt(i);
                if (SceneInfo.IsGameplayScene(loadedScene) && loadedScene.isLoaded) 
                    SceneManager.UnloadSceneAsync(loadedScene);
            }

            // If the persistent UI was hidden for gameplay, ensure it is visible again
            if (PersistentUIManager.Exists)
            {
                PersistentUIManager.Show(animated: false);
            }

            // Show Main Menu panel via MenuNavigator instead of loading scene
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
                Debug.Log("[CompleteUI]: Requesting Main Menu show (instant)");
                menuNavigator.ShowPanel("Main Menu", true);

                // Safety: force canvas settings in case the panel was left hidden by previous scene transitions
                var panel = menuNavigator.GetPanel("Main Menu");
                if (panel?.canvasGroup != null)
                {
                    panel.canvasGroup.gameObject.SetActive(true);
                    panel.canvasGroup.alpha = 1f;
                    panel.canvasGroup.interactable = true;
                    panel.canvasGroup.blocksRaycasts = true;
                }
                else
                {
                    // Fallback: find a CanvasGroup named "Main Menu" in the scene and force it visible
                    var mainMenuGo = GameObject.Find("Main Menu");
                    var cg = mainMenuGo != null ? mainMenuGo.GetComponent<CanvasGroup>() : null;
                    if (cg != null)
                    {
                        cg.gameObject.SetActive(true);
                        cg.alpha = 1f;
                        cg.interactable = true;
                        cg.blocksRaycasts = true;
                        Debug.Log("[CompleteUI]: Forced Main Menu CanvasGroup visible via fallback search");
                    }
                    else
                    {
                        Debug.LogWarning("[CompleteUI]: Could not find Main Menu CanvasGroup to show");
                    }
                }
            }
            else
            {
                Debug.LogWarning("[CompleteUI]: MenuNavigator not found - cannot show main menu");
            }
        }
    }
}
