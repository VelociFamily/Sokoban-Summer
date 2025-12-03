using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CompareMenuDimensions : MonoBehaviour
{
    public static void Execute()
    {
        var go = new GameObject("CompareMenuDimensions");
        go.AddComponent<CompareMenuDimensions>();
    }

    private IEnumerator Start()
    {
        Debug.Log("--- Comparing Menu Dimensions ---");

        // 1. Get PersistentUI dimensions
        RectTransform persistentRect = null;
        var persistentScene = SceneManager.GetSceneByName("PersistentUI");
        if (persistentScene.IsValid())
        {
            foreach (var root in persistentScene.GetRootGameObjects())
            {
                var selector = root.transform.Find("Menu Canvas/Level Selection Menu");
                if (selector != null)
                {
                    persistentRect = selector.GetComponent<RectTransform>();
                    break;
                }
            }
        }

        if (persistentRect != null)
        {
            Debug.Log($"PersistentUI 'Level Selection Menu' Rect:");
            Debug.Log($" - AnchoredPosition: {persistentRect.anchoredPosition}");
            Debug.Log($" - SizeDelta: {persistentRect.sizeDelta}");
            Debug.Log($" - AnchorMin: {persistentRect.anchorMin}");
            Debug.Log($" - AnchorMax: {persistentRect.anchorMax}");
            Debug.Log($" - Pivot: {persistentRect.pivot}");
            Debug.Log($" - Scale: {persistentRect.localScale}");
        }
        else
        {
            Debug.LogError("Could not find 'Level Selection Menu' in PersistentUI scene.");
        }

        // 2. Load Main Menu scene additively to compare
        Debug.Log("Loading 'Main Menu' scene additively...");
        yield return SceneManager.LoadSceneAsync("Main Menu", LoadSceneMode.Additive);

        RectTransform mainMenuRect = null;
        var mainMenuScene = SceneManager.GetSceneByName("Main Menu");
        if (mainMenuScene.IsValid())
        {
            foreach (var root in mainMenuScene.GetRootGameObjects())
            {
                // Path might be different in original scene, usually under a Canvas
                // Based on previous logs: Menus/Level Selection Menu
                var selector = root.transform.Find("Menus/Level Selection Menu"); 
                if (selector == null)
                {
                     // Try finding by name recursively if path differs
                     selector = FindChildByName(root.transform, "Level Selection Menu");
                }

                if (selector != null)
                {
                    mainMenuRect = selector.GetComponent<RectTransform>();
                    break;
                }
            }
        }

        if (mainMenuRect != null)
        {
            Debug.Log($"Main Menu 'Level Selection Menu' Rect:");
            Debug.Log($" - AnchoredPosition: {mainMenuRect.anchoredPosition}");
            Debug.Log($" - SizeDelta: {mainMenuRect.sizeDelta}");
            Debug.Log($" - AnchorMin: {mainMenuRect.anchorMin}");
            Debug.Log($" - AnchorMax: {mainMenuRect.anchorMax}");
            Debug.Log($" - Pivot: {mainMenuRect.pivot}");
            Debug.Log($" - Scale: {mainMenuRect.localScale}");
        }
        else
        {
            Debug.LogError("Could not find 'Level Selection Menu' in Main Menu scene.");
        }

        // 3. Compare and Report
        if (persistentRect != null && mainMenuRect != null)
        {
            bool posMatch = persistentRect.anchoredPosition == mainMenuRect.anchoredPosition;
            bool sizeMatch = persistentRect.sizeDelta == mainMenuRect.sizeDelta;
            bool scaleMatch = persistentRect.localScale == mainMenuRect.localScale;

            if (posMatch && sizeMatch && scaleMatch)
            {
                Debug.Log("SUCCESS: Dimensions match!");
            }
            else
            {
                Debug.LogWarning("MISMATCH DETECTED!");
                if (!posMatch) Debug.LogWarning($"Position mismatch: {persistentRect.anchoredPosition} vs {mainMenuRect.anchoredPosition}");
                if (!sizeMatch) Debug.LogWarning($"Size mismatch: {persistentRect.sizeDelta} vs {mainMenuRect.sizeDelta}");
                if (!scaleMatch) Debug.LogWarning($"Scale mismatch: {persistentRect.localScale} vs {mainMenuRect.localScale}");
            }
        }

        // Cleanup
        yield return SceneManager.UnloadSceneAsync("Main Menu");
        Destroy(gameObject);
    }

    private Transform FindChildByName(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform child in parent)
        {
            var result = FindChildByName(child, name);
            if (result != null) return result;
        }
        return null;
    }
}
