using Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace UI
{
    public class PauseButton : MonoBehaviour
    {
        public GameObject pauseMenu;
        public GameObject blocker; // This object blocks pause menu when active

        // --- NEW VARIABLES FOR BLUR ---
        public Volume volume; // Reference to the Volume component
        private DepthOfField depthOfField; // Reference to the Depth of Field effect

        private Camera mainCamera;
        private bool isUsingInputService = false;

        // Track the menu scene name or index
        private const int menuSceneBuildIndex = 1;
        private string menuSceneName;

        private void Start()
        {
            Time.timeScale = 1f;
            
            // Try to use InputService if available
            if (InputService.Instance != null && InputService.Instance.InputActions != null)
            {
                isUsingInputService = true;
                InputService.Instance.InputActions.UI.EscapeStart.performed += OnPausePerformed;
                InputService.Instance.InputActions.UI.Click.performed += OnClickPerformed;
                InputService.Instance.InputActions.UI.Enable();
            }
            else
            {
                // Fallback - create own instance if InputService not available
                Debug.LogWarning("[PauseButton]: InputService not available, creating fallback input actions");
                // Note: This fallback behavior would need InputSystem_Actions implementation
                // For now, we'll rely on InputService being available
            }

            // Get main camera for mouse position conversion with improved fallback
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                // Use FindAnyObjectByType instead of FindFirstObjectByType for better compatibility
                mainCamera = FindAnyObjectByType<Camera>();
                if (mainCamera != null)
                {
                    Debug.LogWarning("[PauseButton]: Main camera not tagged - using first available camera as fallback");
                }
                else
                {
                    Debug.LogError("[PauseButton]: No camera found in scene - mouse click detection will not work");
                }
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
            if (isUsingInputService && InputService.Instance?.InputActions?.UI != null)
            {
                InputService.Instance.InputActions.UI.EscapeStart.performed -= OnPausePerformed;
                InputService.Instance.InputActions.UI.Click.performed -= OnClickPerformed;
            }
        }

        private void OnDestroy()
        {
            if (isUsingInputService && InputService.Instance?.InputActions?.UI != null)
            {
                InputService.Instance.InputActions.UI.EscapeStart.performed -= OnPausePerformed;
                InputService.Instance.InputActions.UI.Click.performed -= OnClickPerformed;
            }
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

            // Get mouse position - use InputService if available
            Vector2 mousePosition;
            
            if (isUsingInputService && InputService.Instance?.InputActions?.UI != null)
            {
                mousePosition = InputService.Instance.InputActions.UI.Point.ReadValue<Vector2>();
            }
            else
            {
                // Fallback for when InputService is not available
                mousePosition = Mouse.current.position.ReadValue();
            }
            
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
}
