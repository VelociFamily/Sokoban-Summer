using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSelector : MonoBehaviour
{
    public int sceneToLoadIndex; // Set this in Inspector
    public GameObject lockOverlay; // Assign a lock icon in the Inspector

    private static bool[] completedScenes;

    private void Awake()
    {
        InitializeCompletionTracker();

        // Handle lock overlay
        if (lockOverlay != null)
        {
            if (CanLoadScene(sceneToLoadIndex))
            {
                lockOverlay.SetActive(false); // Unlocked → hide lock
            }
            else
            {
                lockOverlay.SetActive(true); // Locked → show lock
            }
        }
    }

    private void Start()
    {
        var currentScene = SceneManager.GetActiveScene().buildIndex;
        completedScenes[currentScene] = true;
    }

    public void TryLoadScene()
    {
        InitializeCompletionTracker();

        if (sceneToLoadIndex < 0 || sceneToLoadIndex >= completedScenes.Length)
        {
            Debug.LogWarning("Invalid scene index");
            return;
        }

        if (sceneToLoadIndex == 0 || completedScenes[sceneToLoadIndex - 1])
        {
            SceneManager.LoadScene(sceneToLoadIndex);
        }
        else
        {
            Debug.Log("❌ You must complete the previous scene first!");
        }
    }

    public static void MarkSceneCompleted(int sceneIndex)
    {
        InitializeCompletionTracker();

        if (sceneIndex >= 0 && sceneIndex < completedScenes.Length)
        {
            completedScenes[sceneIndex] = true;
            Debug.Log($"✅ Scene {sceneIndex} marked as completed.");
        }
    }

    public static bool CanLoadScene(int sceneIndex)
    {
        InitializeCompletionTracker();

        if (sceneIndex == 0) return true;
        if (sceneIndex < 0 || sceneIndex >= completedScenes.Length) return false;

        return completedScenes[sceneIndex - 1];
    }

    private static void InitializeCompletionTracker()
    {
        var totalScenes = SceneManager.sceneCountInBuildSettings;

        if (completedScenes == null || completedScenes.Length != totalScenes)
        {
            completedScenes = new bool[totalScenes];
            completedScenes[1] = true;
        }
    }
}