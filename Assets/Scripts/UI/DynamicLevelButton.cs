using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
        
        [Tooltip("Text component for level name")]
        public Text levelNameText;
        
        [Tooltip("Text component for goals/par info")]
        public Text goalText;
        
        [Tooltip("Image component for level preview")]
        public Image previewImage;
        
        [Tooltip("Lock overlay GameObject (shown when level is locked)")]
        public GameObject lockOverlay;

        [Header("Level Data")]
        public LevelManager.LevelInfo levelInfo;

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
            
            // Simple scene loading - just load the scene directly
            SceneManager.LoadScene(levelInfo.buildIndex);
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