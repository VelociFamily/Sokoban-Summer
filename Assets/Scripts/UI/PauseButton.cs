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
    private Camera mainCamera;

    // Track the menu scene name or index
    private const int menuSceneBuildIndex = 0;
    private string menuSceneName;

    private void Start()
    {
        Time.timeScale = 1f;
        inputActions = new InputSystem_Actions();
        inputActions.UI.EscapeStart.performed += OnPausePerformed;
        inputActions.UI.Click.performed += OnClickPerformed;
        inputActions.UI.Enable();

        // Get main camera for mouse position conversion
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
            Debug.LogWarning("[PauseButton]: Main camera not tagged - using first available camera as fallback");
        }

        // --- NEW CODE: Using the modern, recommended method ---
        volume = FindAnyObjectByType<Volume>();

        if (volume != null && volume.profile.TryGet(out depthOfField))
            depthOfField.active = false;
        else
            Debug.LogWarning(
                "[PauseButton]: Depth of Field effect not found on volume profile or no Volume object found in scene");

        // Ensure the "game" scene is always loaded
        if (!SceneManager.GetSceneByName("game").isLoaded) SceneManager.LoadSceneAsync("game", LoadSceneMode.Additive);

        // Get menu scene name from build index
        menuSceneName = SceneUtility.GetScenePathByBuildIndex(menuSceneBuildIndex);
        if (!string.IsNullOrEmpty(menuSceneName)) menuSceneName = System.IO.Path.GetFileNameWithoutExtension(menuSceneName);
    }

    private void OnDisable()
    {
        if (inputActions == null) return;
        inputActions.UI.EscapeStart.performed -= OnPausePerformed;
        inputActions.UI.Click.performed -= OnClickPerformed;
        inputActions.UI.Disable();
    }

    private void OnDestroy()
    {
        if (inputActions == null) return;
        inputActions.UI.EscapeStart.performed -= OnPausePerformed;
        inputActions.UI.Click.performed -= OnClickPerformed;
        inputActions.UI.Disable();
        inputActions?.Dispose();
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        if (blocker != null && blocker.activeInHierarchy) return;

        // Check if a GameObject with the tag "levelcomplete" is active
        var levelCompleteObject = GameObject.FindWithTag("levelcomplete");
        if (levelCompleteObject != null && levelCompleteObject.activeSelf) return;

        var isActive = pauseMenu.activeSelf;

        if (isActive)
            ResumeGame();
        else
            PauseGame();
    }

    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        if (mainCamera == null) return;
        if (blocker != null && blocker.activeInHierarchy) return;

        // Check if a GameObject with the tag "levelcomplete" is active
        var levelCompleteObject = GameObject.FindWithTag("levelcomplete");
        if (levelCompleteObject != null && levelCompleteObject.activeSelf) return;

        // Get mouse position and check if this pause button was clicked
        var mousePosition = inputActions.UI.Point.ReadValue<Vector2>();
        Vector2 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        var hit = Physics2D.Raycast(worldPosition, Vector2.zero);

        if (hit.collider == null || hit.collider.gameObject != gameObject) return;
        Debug.Log("[PauseButton]: Game paused via mouse click");
        PauseGame();
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;

        // Unload any loaded gameplay scenes (tutorial or game levels)
        for (var i = 0; i < SceneManager.sceneCount; i++)
        {
            var loadedScene = SceneManager.GetSceneAt(i);
            if (SceneInfo.IsGameplayScene(loadedScene) && loadedScene.isLoaded) 
                SceneManager.UnloadSceneAsync(loadedScene);
        }

        var asyncOp = SceneManager.LoadSceneAsync(menuSceneBuildIndex, LoadSceneMode.Additive);
        asyncOp.completed += (op) => 
        { 
            // Handle potential EventSystem and AudioListener duplication after menu load
            HandleEventSystemDuplication();
            HandleAudioListenerDuplication();
        };
    }

    /// <summary>
    /// Handles EventSystem duplication when returning to main menu from pause screen.
    /// Ensures only one EventSystem exists to prevent input conflicts.
    /// </summary>
    private static void HandleEventSystemDuplication()
    {
        var eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);

        if (eventSystems.Length <= 1) return;
        Debug.LogWarning($"[PauseButton]: Detected {eventSystems.Length} EventSystems after menu load - removing duplicates");
            
        // Keep the first EventSystem and destroy the rest
        for (var i = 1; i < eventSystems.Length; i++)
        {
            Debug.Log($"[PauseButton]: Removing duplicate EventSystem from '{eventSystems[i].gameObject.name}'");
            Destroy(eventSystems[i].gameObject);
        }
    }

    /// <summary>
    /// Handles AudioListener duplication when returning to main menu from pause screen.
    /// Ensures only one AudioListener exists to prevent audio conflicts.
    /// </summary>
    private static void HandleAudioListenerDuplication()
    {
        var audioListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);

        if (audioListeners.Length <= 1) return;
        Debug.LogWarning($"[PauseButton]: Detected {audioListeners.Length} AudioListeners after menu load - disabling duplicates");
            
        // Keep the first AudioListener and disable the rest (don't destroy the camera, just disable the AudioListener component)
        for (var i = 1; i < audioListeners.Length; i++)
        {
            Debug.Log($"[PauseButton]: Disabling duplicate AudioListener on '{audioListeners[i].gameObject.name}'");
            audioListeners[i].enabled = false;
        }
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;

        if (depthOfField != null) depthOfField.active = true;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;

        if (depthOfField != null) depthOfField.active = false;
    }
}
