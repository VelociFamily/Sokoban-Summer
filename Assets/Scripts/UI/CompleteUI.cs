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
        var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        var nextSceneIndex = currentSceneIndex + 1;
        
        try
        {
            var loadOp = SceneManager.LoadSceneAsync(nextSceneIndex, LoadSceneMode.Additive);
            await loadOp;
            
            await SceneManager.UnloadSceneAsync(currentSceneIndex);
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(nextSceneIndex));
            // Example usage: achievementManager?.UnlockLevelTwo();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[CompleteUI]: Error loading next scene: {e.Message}");
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

        SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        // Example usage: achievementManager?.UnlockTutorial();
    }
}
