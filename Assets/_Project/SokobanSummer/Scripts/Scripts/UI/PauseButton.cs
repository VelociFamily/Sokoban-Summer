using Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace UI
{
    /// <summary>
    /// Handles pause/resume functionality for gameplay scenes with input and UI management.
    /// </summary>
    public class PauseButton : MonoBehaviour
    {
        private GameplayUIController gameplayUIController;

        // --- NEW VARIABLES FOR BLUR ---
        public Volume volume; // Reference to the Volume component
        private DepthOfField depthOfField; // Reference to the Depth of Field effect

        private InputSystem_Actions inputActions;
        private InputService inputService;
        private MoveCounter moveCounter;
        private bool ownsInputActions;

        // Cache both UI and Player map pause bindings so Start/Escape works even if one map is disabled
        private InputAction pauseActionUI;
        private InputAction pauseActionPlayer;

        private void Start()
        {
            Time.timeScale = 1f;

            InitializeServices();
            SetupInputActions();

            // Find GameplayUIController in the persistent UI
            gameplayUIController = FindFirstObjectByType<GameplayUIController>();
            if (gameplayUIController == null)
            {
                Debug.LogError("[PauseButton]: GameplayUIController not found in scene - pause functionality will not work!");
            }

            // --- NEW CODE: Using the modern, recommended method ---
            volume = FindAnyObjectByType<Volume>();

            if (volume != null && volume.profile.TryGet(out depthOfField))
                depthOfField.active = false;
            else
                Debug.LogWarning(
                    "[PauseButton]: Depth of Field effect not found on volume profile or no Volume object found in scene");
        }

        private void OnDisable()
        {
            if (inputActions == null) return;
            if (pauseActionUI != null) pauseActionUI.performed -= OnPausePerformed;
            if (pauseActionPlayer != null) pauseActionPlayer.performed -= OnPausePerformed;

            if (ownsInputActions)
            {
                inputActions.UI.Disable();
            }
        }

        private void InitializeServices()
        {
            if (ServiceLocator.TryGet(out InputService service))
            {
                inputService = service;
            }

            ServiceLocator.TryGet(out moveCounter);
        }

        private void SetupInputActions()
        {
            if (inputService != null && inputService.InputActions != null)
            {
                inputActions = inputService.InputActions;
                ownsInputActions = false;
                // Ensure UI map is enabled; other systems may disable it on scene load
                inputService.EnableUIInput();
            }
            else
            {
                inputActions = new InputSystem_Actions();
                ownsInputActions = true;
                inputActions.UI.Enable();
            }

            // Listen on both UI and Player maps so Start/Escape always pauses even if one map is disabled
            pauseActionUI = inputActions.UI.EscapeStart;
            pauseActionPlayer = inputActions.Player.EscapeStart;

            // UI map may be disabled by other scripts after this Start; guard by enabling here as well
            if (!inputActions.UI.enabled) inputActions.UI.Enable();

            pauseActionUI.performed += OnPausePerformed;
            pauseActionPlayer.performed += OnPausePerformed;
        }

        private void OnDestroy()
        {
            if (inputActions == null) return;
            if (pauseActionUI != null) pauseActionUI.performed -= OnPausePerformed;
            if (pauseActionPlayer != null) pauseActionPlayer.performed -= OnPausePerformed;

            if (ownsInputActions)
            {
                inputActions.UI.Disable();
                inputActions.Dispose();
            }
        }

        private void OnPausePerformed(InputAction.CallbackContext context)
        {
            // Ignore pause input when not in a gameplay scene (prevents main menu from opening while paused)
            var activeScene = SceneManager.GetActiveScene();
            if (!SceneInfo.IsGameplayScene(activeScene))
            {
                return;
            }

            // Check if a GameObject with the tag "levelcomplete" is active
            var levelCompleteObject = GameObject.FindWithTag("levelcomplete");
            if (levelCompleteObject != null && levelCompleteObject.activeSelf) return;

            if (gameplayUIController == null) return;

            if (gameplayUIController.IsPaused())
                ResumeGame();
            else
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

            // Show main menu via MenuNavigator
            var menuNavigator = FindFirstObjectByType<MenuNavigator>();
            if (menuNavigator != null)
            {
                menuNavigator.ShowMainMenu();
                Debug.Log("[PauseButton]: Navigated to Main Menu via MenuNavigator");
            }
            else
            {
                Debug.LogWarning("[PauseButton]: MenuNavigator not available - cannot show main menu panel");
            }
        }

        public void PauseGame()
        {
            if (gameplayUIController == null)
            {
                Debug.LogError("[PauseButton]: GameplayUIController not found - cannot pause");
                return;
            }

            // Ensure legacy menu panels stay hidden when pausing from gameplay
            var menuNavigator = FindFirstObjectByType<MenuNavigator>();
            if (menuNavigator != null)
            {
                menuNavigator.HideAllPanels(instant: true);
            }

            // Disable player input first to prevent new moves
            inputService?.DisablePlayerInput();
            
            // Show pause menu immediately so it's interactive
            gameplayUIController.ShowPausePanel(instant: false);
            Time.timeScale = 0f;
            moveCounter?.PauseTimer();

            if (depthOfField != null) depthOfField.active = true;

            Debug.Log("[PauseButton]: Game paused");
        }

        public void ResumeGame()
        {
            if (gameplayUIController == null) return;

            gameplayUIController.HidePausePanel(instant: false);
            Time.timeScale = 1f;
            moveCounter?.ResumeTimer();
            inputService?.EnablePlayerInput();

            if (depthOfField != null) depthOfField.active = false;

            Debug.Log("[PauseButton]: Game resumed");
        }
    }
}
