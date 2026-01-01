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
        private PauseButton pauseButton;

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

            // Find PauseButton (handles keyboard/controller input)
            pauseButton = FindFirstObjectByType<PauseButton>();
            if (pauseButton == null)
            {
                Debug.LogWarning("[PauseResumeButton]: PauseButton not found. Keyboard/controller pause won't work.");
            }
        }

        private void OnEnable()
        {
            // Note: Visibility (SetActive state) of this button is managed by InputDeviceController
            // and/or GameplayUIController depending on input device. Do not call gameObject.SetActive(true)
            // here to avoid conflicting with those controllers; we only refresh the label.
            UpdateButtonText();
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
            // Delegate to PauseButton to ensure keyboard/controller input is properly configured
            if (pauseButton != null)
            {
                pauseButton.PauseGame();
            }
            else
            {
                // Fallback if PauseButton not found
                isPaused = true;
                Time.timeScale = 0f;

                var moveCounter = ServiceLocator.TryGet<MoveCounter>(out var mc) ? mc : null;
                if (moveCounter != null)
                {
                    moveCounter.PauseTimer();
                }

                if (gameplayUIController != null)
                {
                    gameplayUIController.ShowPausePanel(instant: false);
                }

                UpdateButtonText();
            }

            Debug.Log("[PauseResumeButton]: Game paused via touchscreen button");
        }

        /// <summary>
        /// Resume the game - called by resume button in pause panel
        /// </summary>
        public void Resume()
        {
            // Delegate to PauseButton to ensure keyboard/controller input is properly configured
            if (pauseButton != null)
            {
                pauseButton.ResumeGame();
            }
            else
            {
                // Fallback if PauseButton not found
                isPaused = false;
                Time.timeScale = 1f;

                var moveCounter = ServiceLocator.TryGet<MoveCounter>(out var mc) ? mc : null;
                if (moveCounter != null)
                {
                    moveCounter.ResumeTimer();
                }

                if (gameplayUIController != null)
                {
                    gameplayUIController.HidePausePanel(instant: false);
                }

                UpdateButtonText();
            }

            Debug.Log("[PauseResumeButton]: Game resumed via touchscreen button");
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

        /// <summary>
        /// Update internal pause state and button text (called by GameplayUIController)
        /// </summary>
        public void UpdatePauseState(bool paused)
        {
            isPaused = paused;
            UpdateButtonText();
        }
    }
}
