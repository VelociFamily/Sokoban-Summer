using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MenuPersistence : MonoBehaviour
{
    private static bool isInitialized;
    private static HashSet<GameObject> preservedObjects = new HashSet<GameObject>();

    [Tooltip("List of menu-related objects you want to preserve (e.g., Menu UI, AudioManager, AchievementManager)")]
    public List<GameObject> objectsToPersist;

    public List<GameObject> objectsToPersistPrefabs; // assign prefabs in inspector

    private void Awake()
    {
        if (!isInitialized)
        {
            foreach (var prefab in objectsToPersistPrefabs)
            {
                if (prefab != null)
                {
                    var obj = Instantiate(prefab);
                    DontDestroyOnLoad(obj);
                    preservedObjects.Add(obj);
                }
            }
            DontDestroyOnLoad(gameObject);
            isInitialized = true;
        }
        else
        {
            Destroy(gameObject);
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0) // Menu scene
        {
            var rootObjects = scene.GetRootGameObjects();

            foreach (var obj in rootObjects)
            {
                // Destroy duplicate MenuUI
                if (obj.CompareTag("MenuUI") && !preservedObjects.Contains(obj))
                {
                    Debug.Log("Destroying duplicate MenuUI: " + obj.name);
                    Destroy(obj);
                }
                // Destroy duplicate music player (assuming it has tag "Music" or similar)
                if (obj.CompareTag("Music") && !preservedObjects.Contains(obj))
                {
                    Debug.Log("Destroying duplicate Music: " + obj.name);
                    Destroy(obj);
                }
            }

            // Reactivate original preserved objects
            foreach (var obj in preservedObjects)
            {
                obj.SetActive(true);
            }
        }
        else
        {
            foreach (var obj in preservedObjects)
            {
                if (obj != null && obj.CompareTag("MenuUI"))
                {
                    obj.SetActive(false);
                }
            }
        }
    }

    public void RestoreMenu()
    {
        foreach (var obj in preservedObjects)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }


}