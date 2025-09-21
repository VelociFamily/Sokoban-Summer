using UnityEngine;

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
        var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        return activeScene.buildIndex is 0 or 1; // Game or Main Menu scenes
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
        var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        return activeScene.buildIndex switch
        {
            0 => SceneType.Game,
            1 => SceneType.MainMenu,
            >= 2 and <= 7 => SceneType.TutorialLevel, // Assume tutorials first, then gameplay
            _ => SceneType.GameplayLevel
        };
    }
}