using UnityEngine;
using UnityEngine.InputSystem;

namespace UI
{
    /// <summary>
    /// Controls UI elements based on the current input device type.
    /// Hides the pause button when keyboard/controller is used (since Escape/Start can be used),
    /// and shows it for touchscreen devices.
    /// Also respects the pause state - button is hidden when the game is paused.
    /// </summary>
    public class InputDeviceController : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The pause button GameObject to show/hide based on input device")]
        private GameObject pauseButton;

        private GameplayUIController gameplayUIController;

        private void OnEnable()
        {
            // Subscribe to device change events
            InputSystem.onActionChange += OnActionChange;
            
            // Initial check
            UpdatePauseButtonVisibility();
        }

        private void OnDisable()
        {
            // Unsubscribe from device change events
            InputSystem.onActionChange -= OnActionChange;
        }

        private void Start()
        {
            // Find GameplayUIController to check pause state
            gameplayUIController = FindFirstObjectByType<GameplayUIController>();
            if (gameplayUIController == null)
            {
                Debug.LogWarning("[InputDeviceController]: GameplayUIController not found - pause state checks will be skipped");
            }

            // Double-check on start
            UpdatePauseButtonVisibility();
        }

        private void OnActionChange(object obj, InputActionChange change)
        {
            // Update visibility when actions change (including device switches)
            if (change == InputActionChange.ActionPerformed ||
                change == InputActionChange.ActionStarted)
            {
                UpdatePauseButtonVisibility();
            }
        }

        private void UpdatePauseButtonVisibility()
        {
            if (pauseButton == null)
            {
                Debug.LogWarning("[InputDeviceController]: Pause button reference is not set!");
                return;
            }

            // Check if game is paused - if so, button should remain hidden
            if (gameplayUIController != null && gameplayUIController.IsPaused())
            {
                // Don't change visibility when paused - let GameplayUIController manage it
                if (Application.isEditor)
                {
                    Debug.Log("[InputDeviceController]: Game is paused, skipping visibility update");
                }
                return;
            }

            // Check if we have a touchscreen device currently active
            bool hasTouchscreen = IsTouchscreenActive();

            // Show pause button only for touchscreen (and when not paused)
            pauseButton.SetActive(hasTouchscreen);
            
            if (Application.isEditor)
            {
                Debug.Log($"[InputDeviceController]: Pause button visibility set to {hasTouchscreen} (Touchscreen: {hasTouchscreen})");
            }
        }

        private bool IsTouchscreenActive()
        {
            // Touchscreen.current is non-null when a touchscreen device is present.
            return Touchscreen.current != null;
        }

        // Public method to manually force an update (can be called from other scripts if needed)
        public void ForceUpdate()
        {
            UpdatePauseButtonVisibility();
        }

        /// <summary>
        /// Notify this controller that the pause state has changed.
        /// This ensures the button visibility is updated immediately when pause state changes.
        /// </summary>
        public void OnPauseStateChanged()
        {
            UpdatePauseButtonVisibility();
        }
    }
}
