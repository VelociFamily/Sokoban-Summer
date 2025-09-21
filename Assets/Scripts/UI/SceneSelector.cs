using UnityEngine;
using System.Collections.Generic;

public class SceneSelector
{
    private static Dictionary<int, bool> _completedScenes;

    private static Dictionary<int, bool> CompletedScenes
    {
        get
        {
            _completedScenes ??= new Dictionary<int, bool> { [2] = true };
            return _completedScenes;
        }
    }

    public static void MarkNextLevelUnlocked(int sceneIndex)
    {
        CompletedScenes[sceneIndex] = true;
        Debug.Log($"[SceneSelector]: Scene {sceneIndex} completed and unlocked for progression");
    }

    public static bool CanLoadScene(int sceneIndex)
    {
        return CompletedScenes.TryGetValue(sceneIndex, out var isUnlocked) && isUnlocked;
    }
}
