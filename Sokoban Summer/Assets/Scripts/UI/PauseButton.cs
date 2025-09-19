using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseButton : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject blocker; // This object blocks pause menu when active

    // --- NEW VARIABLES FOR BLUR ---
    public Volume volume; // Reference to the Volume component
    private DepthOfField depthOfField; // Reference to the Depth of Field effect

    private InputSystem_Actions inputActions;

    // Track the menu scene name or index
    private const int menuSceneBuildIndex = 0;
    private string menuSceneName;
    private bool menuSceneLoaded;

    private void Start()
    {
        Time.timeScale = 1f;
        inputActions = new InputSystem_Actions();
        inputActions.UI.EscapeStart.performed += OnPausePerformed;
        inputActions.UI.Enable();

        // --- NEW CODE: Using the modern, recommended method ---
        volume = FindAnyObjectByType<Volume>();

        if (volume != null && volume.profile.TryGet(out depthOfField))
        {
            depthOfField.active = false;
        }
        else
        {
            Debug.LogWarning("Depth of Field effect not found on volume profile or no Volume object found in the scene.");
        }

        // Ensure the "game" scene is always loaded
        if (!SceneManager.GetSceneByName("game").isLoaded)
        {
            SceneManager.LoadSceneAsync("game", LoadSceneMode.Additive);
        }

        // Get menu scene name from build index
        menuSceneName = SceneUtility.GetScenePathByBuildIndex(menuSceneBuildIndex);
        if (!string.IsNullOrEmpty(menuSceneName))
        {
            menuSceneName = System.IO.Path.GetFileNameWithoutExtension(menuSceneName);
        }
    }

    private void OnDisable()
    {
        inputActions?.UI.Disable();
    }

    private void OnDestroy()
    {
        if (inputActions == null) return;
        inputActions.UI.EscapeStart.performed -= OnPausePerformed;
        inputActions.UI.Disable();
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        if (blocker != null && blocker.activeInHierarchy) return;

        // Check if a GameObject with the tag "levelcomplete" is active
        var levelCompleteObject = GameObject.FindWithTag("levelcomplete");
        if (levelCompleteObject != null && levelCompleteObject.activeSelf) return;

        var isActive = pauseMenu.activeSelf;

        if (isActive)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void OnMouseDown()
    {
        if (blocker != null && blocker.activeInHierarchy) return;

        // Check if a GameObject with the tag "levelcomplete" is active
        var levelCompleteObject = GameObject.FindWithTag("levelcomplete");
        if (levelCompleteObject != null && levelCompleteObject.activeSelf) return;

        PauseGame();
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;

        // Unload any loaded scene with build index between 1 and 6 (inclusive)
        for (var i = 0; i < SceneManager.sceneCount; i++)
        {
            var loadedScene = SceneManager.GetSceneAt(i);
            var buildIndex = loadedScene.buildIndex;
            if (buildIndex >= 1 && buildIndex <= 6 && loadedScene.isLoaded)
            {
                SceneManager.UnloadSceneAsync(loadedScene);
            }
        }

        var asyncOp = SceneManager.LoadSceneAsync(menuSceneBuildIndex, LoadSceneMode.Additive);
        asyncOp.completed += (op) => 
        { 
            menuSceneLoaded = true;
            // Handle potential EventSystem duplication after menu load
            HandleEventSystemDuplication();
        };
    }

    /// <summary>
    /// Handles EventSystem duplication when returning to main menu from pause screen.
    /// Ensures only one EventSystem exists to prevent input conflicts.
    /// </summary>
    private void HandleEventSystemDuplication()
    {
        var eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        
        if (eventSystems.Length > 1)
        {
            Debug.LogWarning($"PauseButton: Found {eventSystems.Length} EventSystems after loading menu. Removing duplicates.");
            
            // Keep the first EventSystem and destroy the rest
            for (int i = 1; i < eventSystems.Length; i++)
            {
                Debug.Log($"PauseButton: Destroying duplicate EventSystem on '{eventSystems[i].gameObject.name}'");
                Destroy(eventSystems[i].gameObject);
            }
        }
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;

        if (depthOfField != null)
        {
            depthOfField.active = true;
        }
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;

        if (depthOfField != null)
        {
            depthOfField.active = false;
        }
        // Removed scene-unloading logic from here!
    }
}