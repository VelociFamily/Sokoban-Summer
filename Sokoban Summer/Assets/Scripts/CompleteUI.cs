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
        SceneManager.LoadSceneAsync(nextSceneIndex, LoadSceneMode.Additive)
            .completed += _ =>
        {
            SceneManager.UnloadSceneAsync(currentSceneIndex);
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(nextSceneIndex));
            // Example usage: achievementManager?.UnlockLevelTwo();
        };
    }

    public void LoadMenu()
    {
        var sceneCount = SceneManager.sceneCount;
        for (var i = 0; i < sceneCount; i++)
        {
            var loadedScene = SceneManager.GetSceneAt(i);
            var buildIndex = loadedScene.buildIndex;
            if (loadedScene.isLoaded && buildIndex is >= 2 and <= 7) SceneManager.UnloadSceneAsync(loadedScene);
        }

        SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        // Example usage: achievementManager?.UnlockTutorial();
    }
}