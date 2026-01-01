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
            // Ensure the pause button is visible when this component is enabled in a resumed state.
            // This is important for touchscreen users after the pause panel has hidden the button.
            if (!isPaused)
            {
                gameObject.SetActive(true);
            }
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
        /// Synchronizes this button's internal pause state and label with the global pause state.
        /// </summary>
        /// <remarks>
        /// This method exists so that <see cref="GameplayUIController" /> can keep the touchscreen
        /// pause/resume button in sync when the game is paused or resumed via other inputs
        /// (for example, keyboard/controller through <see cref="PauseButton" />) instead of
        /// directly through <see cref="TogglePause" /> or <see cref="Resume" />.
        ///
        /// Call this after any pause state change that bypasses this component's direct control,
        /// passing in the authoritative global pause value managed by <see cref="GameplayUIController" />.
        /// The provided <paramref name="paused" /> value becomes the source of truth for this component's
        /// <c>isPaused</c> field and determines whether the button shows "Pause" or "Resume".
        /// </remarks>
        /// <param name="paused">
        /// The current global pause state as determined by <see cref="GameplayUIController" />.
        /// </param>
        public void UpdatePauseState(bool paused)
        {
            isPaused = paused;
            UpdateButtonText();
        }
    }
}
