using UnityEngine;
using TMPro;
using Core;

namespace UI
{
    /// <summary>
    /// Toggles pause/resume state when button is clicked.
    /// Shows pause panel when paused and hides button.
    /// </summary>
    public class PauseResumeButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI pauseText;

        private bool isPaused = false;
        private GameplayUIController gameplayUIController;

        private void Awake()
        {
            // Find the GameplayUIController (should be on parent Gameplay Canvas)
            gameplayUIController = GetComponentInParent<GameplayUIController>();
            if (gameplayUIController == null)
            {
                gameplayUIController = FindFirstObjectByType<GameplayUIController>();
            }

            if (gameplayUIController == null)
            {
                Debug.LogWarning("[PauseResumeButton]: GameplayUIController not found. Pause panel won't show.");
            }
        }

        private void OnEnable()
        {
            UpdateButtonText();
            // Ensure button is visible when enabled
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Toggle between pause and resume
        /// </summary>
        public void TogglePause()
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        /// <summary>
        /// Pause the game
        /// </summary>
        private void Pause()
        {
            isPaused = true;
            Time.timeScale = 0f;

            // Pause timer if available
            var moveCounter = ServiceLocator.TryGet<MoveCounter>(out var mc) ? mc : null;
            if (moveCounter != null)
            {
                moveCounter.PauseTimer();
            }

            // Show pause panel
            if (gameplayUIController != null)
            {
                gameplayUIController.ShowPausePanel(instant: false);
            }

            // Hide this button while pause panel is shown
            gameObject.SetActive(false);

            UpdateButtonText();
            Debug.Log("[PauseResumeButton]: Game paused, pause panel shown, button hidden");
        }

        /// <summary>
        /// Resume the game - called by resume button in pause panel
        /// </summary>
        public void Resume()
        {
            isPaused = false;
            Time.timeScale = 1f;

            // Resume timer if available
            var moveCounter = ServiceLocator.TryGet<MoveCounter>(out var mc) ? mc : null;
            if (moveCounter != null)
            {
                moveCounter.ResumeTimer();
            }

            // Hide pause panel
            if (gameplayUIController != null)
            {
                gameplayUIController.HidePausePanel(instant: false);
            }

            // Show this button again
            gameObject.SetActive(true);

            UpdateButtonText();
            Debug.Log("[PauseResumeButton]: Game resumed, pause panel hidden, button shown");
        }

        /// <summary>
        /// Update button text based on pause state
        /// </summary>
        private void UpdateButtonText()
        {
            if (pauseText != null)
            {
                pauseText.text = isPaused ? "Resume" : "Pause";
            }
        }
    }
}
