using UnityEngine;
using UnityEngine.InputSystem;

namespace UI
{
    /// <summary>
    /// Controls UI elements based on the current input device type.
    /// Hides the pause button when keyboard/controller is used (since Escape/Start can be used),
    /// and shows it for touchscreen devices.
    /// </summary>
    public class InputDeviceController : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The pause button GameObject to show/hide based on input device")]
        private GameObject pauseButton;

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

            // Check if we have a touchscreen device currently active
            bool hasTouchscreen = IsTouchscreenActive();

            // Show pause button only for touchscreen
            pauseButton.SetActive(hasTouchscreen);
            
            if (Application.isEditor)
            {
                Debug.Log($"[InputDeviceController]: Pause button visibility set to {hasTouchscreen} (Touchscreen: {hasTouchscreen})");
            }
        }

        private bool IsTouchscreenActive()
        {
            // Check if touchscreen device exists and is active
            var touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
                return true;
            }

            // Alternative: Check all input devices
            foreach (var device in InputSystem.devices)
            {
                if (device is Touchscreen)
                {
                    return true;
                }
            }

            return false;
        }

        // Public method to manually force an update (can be called from other scripts if needed)
        public void ForceUpdate()
        {
            UpdatePauseButtonVisibility();
        }
    }
}
