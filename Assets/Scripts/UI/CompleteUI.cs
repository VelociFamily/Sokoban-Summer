using UnityEngine;
using UnityEngine.SceneManagement;

public class CompleteUI : MonoBehaviour
{
    private AchievementManager achievementManager;

    private void Awake()
    {
        achievementManager = AchievementManager.Instance;
    }

    public async void LoadNextScene()
    {
        var currentScene = SceneManager.GetActiveScene();
        var currentSceneIndex = currentScene.buildIndex;
        var nextSceneIndex = currentSceneIndex + 1;
        
        try
        {
            // Call achievement unlock logic based on current level completion
            UnlockNextLevel(currentScene);
            
            var loadOp = SceneManager.LoadSceneAsync(nextSceneIndex, LoadSceneMode.Additive);
            await loadOp;
            
            await SceneManager.UnloadSceneAsync(currentSceneIndex);
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(nextSceneIndex));
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
        var sceneCount = SceneManager.sceneCount;
        for (var i = 0; i < sceneCount; i++)
        {
            var loadedScene = SceneManager.GetSceneAt(i);
            
            // Check if this is a level scene using SceneInfo instead of build indices
            var sceneObjects = loadedScene.GetRootGameObjects();
            var isLevelScene = false;
            
            foreach (var obj in sceneObjects)
            {
                var sceneInfo = obj.GetComponent<SceneInfo>();
                if (sceneInfo != null && (sceneInfo.sceneType == SceneType.TutorialLevel || sceneInfo.sceneType == SceneType.GameplayLevel))
                {
                    isLevelScene = true;
                    break;
                }
            }
            
            // Fallback to build index for scenes without SceneInfo
            if (!isLevelScene && loadedScene.buildIndex is >= 2 and <= 7)
            {
                isLevelScene = true;
            }
            
            if (loadedScene.isLoaded && isLevelScene) 
            {
                SceneManager.UnloadSceneAsync(loadedScene);
            }
        }

        // Load main menu scene - use SceneInfo to determine the main menu build index
        var mainMenuBuildIndex = 1; // Default fallback
        
        // Try to find a better way to identify main menu if needed
        SceneManager.LoadSceneAsync(mainMenuBuildIndex, LoadSceneMode.Additive);
        // Example usage: achievementManager?.UnlockTutorial();
    }
}
