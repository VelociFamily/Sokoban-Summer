using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

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
        // Handle EventSystem and AudioListener duplication for all scenes loaded additively
        if (mode == LoadSceneMode.Additive)
        {
            HandleEventSystemDuplication();
            HandleAudioListenerDuplication();
        }

        if (SceneInfo.IsMainMenuScene(scene)) // Menu scene
        {
            var rootObjects = scene.GetRootGameObjects();

            foreach (var obj in rootObjects)
            {
                // Destroy duplicate MenuUI
                if (obj.CompareTag("MenuUI") && !preservedObjects.Contains(obj))
                {
                    Debug.Log($"[MenuPersistence]: Removing duplicate MenuUI object '{obj.name}' from scene");
                    Destroy(obj);
                }
                // Destroy duplicate music player (assuming it has tag "Music" or similar)
                if (obj.CompareTag("Music") && !preservedObjects.Contains(obj))
                {
                    Debug.Log($"[MenuPersistence]: Removing duplicate Music object '{obj.name}' from scene");
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

    /// <summary>
    /// Handles EventSystem duplication by ensuring only one EventSystem exists in the scene.
    /// When multiple scenes are loaded additively, each may have an EventSystem causing conflicts.
    /// </summary>
    private void HandleEventSystemDuplication()
    {
        var eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        
        if (eventSystems.Length > 1)
        {
            Debug.LogWarning($"[MenuPersistence]: Detected {eventSystems.Length} EventSystems in scene - removing duplicates to prevent input conflicts");
            
            // Keep the first EventSystem and destroy the rest
            for (var i = 1; i < eventSystems.Length; i++)
            {
                Debug.Log($"[MenuPersistence]: Removing duplicate EventSystem from '{eventSystems[i].gameObject.name}'");
                Destroy(eventSystems[i].gameObject);
            }
        }
    }

    /// <summary>
    /// Handles AudioListener duplication by ensuring only one AudioListener exists in the scene.
    /// When multiple scenes are loaded additively, each may have a Camera with AudioListener causing conflicts.
    /// </summary>
    private void HandleAudioListenerDuplication()
    {
        var audioListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        
        if (audioListeners.Length > 1)
        {
            Debug.LogWarning($"[MenuPersistence]: Detected {audioListeners.Length} AudioListeners in scene - removing duplicates to prevent audio conflicts");
            
            // Keep the first AudioListener and disable the rest (don't destroy the camera, just disable the AudioListener component)
            for (var i = 1; i < audioListeners.Length; i++)
            {
                Debug.Log($"[MenuPersistence]: Disabling duplicate AudioListener on '{audioListeners[i].gameObject.name}'");
                audioListeners[i].enabled = false;
            }
        }
    }


}
