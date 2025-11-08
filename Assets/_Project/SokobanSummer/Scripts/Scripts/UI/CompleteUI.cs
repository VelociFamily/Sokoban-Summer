using Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class CompleteUI : MonoBehaviour
    {
        private AchievementManager achievementManager;

        private void Awake()
        {
            achievementManager = SokobanSummer.Core.ServiceLocator.Get<AchievementManager>();
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

                var levelManager = SokobanSummer.Core.ServiceLocator.Get<LevelManager>();
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
            SokobanSummer.Core.ServiceLocator.Get<LevelManager>().LoadMainMenu();
            // Example usage: achievementManager?.UnlockTutorial();
        }
    }
}
