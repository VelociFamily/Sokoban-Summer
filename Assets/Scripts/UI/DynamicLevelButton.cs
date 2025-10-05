using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Core;

namespace UI
{
    /// <summary>
    /// Simple level button component for the Dynamic Level Management System
    /// Replaces the complex SceneButton for cleaner level loading
    /// </summary>
    public class DynamicLevelButton : MonoBehaviour
    {
        [Header("UI Components")]
        [Tooltip("Main button component")]
        public Button button;

    [Tooltip("TextMeshProUGUI component for level name")]
    public TextMeshProUGUI levelNameText;

    [Tooltip("TextMeshProUGUI component for goals/par info")]
    public TextMeshProUGUI goalText;

        [Tooltip("Image component for level preview")]
        public Image previewImage;

        [Tooltip("Lock overlay GameObject (shown when level is locked)")]
        public GameObject lockOverlay;

    [Header("Audio")]
    [Tooltip("Optional click sound to play when selecting a level")]
    public AudioClip clickSfx;

        [Header("Level Data")]
        public LevelManager.LevelInfo levelInfo;

    // Prevent double-activation while scenes are loading
    private bool _clicked = false;

        private void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();

            if (button != null)
                button.onClick.AddListener(LoadLevel);
        }

        /// <summary>
        /// Configure this button with level information
        /// </summary>
        public void SetupLevel(LevelManager.LevelInfo info)
        {
            levelInfo = info;

            // Update UI elements
            if (levelNameText != null)
                levelNameText.text = info.displayName;

            if (goalText != null)
                SetupGoalText(info);

            if (previewImage != null && info.previewImage != null)
                previewImage.sprite = info.previewImage;

            // Update lock state
            UpdateLockState();
        }

        /// <summary>
        /// Setup the goal text with par moves and time
        /// </summary>
        private void SetupGoalText(LevelManager.LevelInfo info)
        {
            var goals = new System.Collections.Generic.List<string>();

            if (info.parMoves > 0)
                goals.Add($"Par: {info.parMoves} moves");
            if (info.parTime > 0f)
                goals.Add($"Time: {FormatTime(info.parTime)}");

            goalText.text = goals.Count > 0 ? string.Join(" | ", goals) : "";
        }

        /// <summary>
        /// Update button lock state and interactability
        /// </summary>
        public void UpdateLockState()
        {
            if (levelInfo == null) return;

            bool canLoad = LevelManager.Instance.CanLoadLevel(levelInfo);

            // Update button interactability
            if (button != null)
                button.interactable = canLoad;

            // Update lock overlay
            if (lockOverlay != null)
                lockOverlay.SetActive(!canLoad);
        }

        /// <summary>
        /// Load the associated level
        /// </summary>
        public void LoadLevel()
        {
            if (levelInfo == null)
            {
                Debug.LogError("[DynamicLevelButton] No level info assigned!");
                return;
            }

            if (!LevelManager.Instance.CanLoadLevel(levelInfo))
            {
                Debug.Log($"[DynamicLevelButton] Level {levelInfo.displayName} is locked");
                return;
            }

            Debug.Log($"[DynamicLevelButton] Loading level: {levelInfo.displayName}");

                // Prevent double clicks
                if (_clicked) return;
                _clicked = true;

                if (clickSfx) ModernAudioService.Instance?.PlaySFX(clickSfx);

                // Delegate to LevelManager so loading respects additive/persistence rules.
                LevelManager.Instance?.LoadLevel(levelInfo);
        }

        /// <summary>
        /// Format time in MM:SS format
        /// </summary>
        private string FormatTime(float time)
        {
            var minutes = Mathf.FloorToInt(time / 60f);
            var seconds = Mathf.FloorToInt(time % 60f);
            return $"{minutes:00}:{seconds:00}";
        }
    }
}
