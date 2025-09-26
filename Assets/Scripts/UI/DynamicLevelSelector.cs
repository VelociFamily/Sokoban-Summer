using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Core;

namespace UI
{
    /// <summary>
    /// Dynamically populates level selection UI based on available levels
    /// Replaces hardcoded level buttons with dynamic generation
    /// </summary>
    public class DynamicLevelSelector : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Parent container where level buttons will be instantiated")]
        public Transform levelButtonContainer;
        
        [Tooltip("Prefab for level buttons (should have SceneButton component)")]
        public GameObject levelButtonPrefab;
        
        [Tooltip("Prefab for section headers (optional)")]
        public GameObject sectionHeaderPrefab;

        [Header("Configuration")]
        [Tooltip("Show tutorial levels")]
        public bool showTutorials = true;
        
        [Tooltip("Show gameplay levels")]
        public bool showGameplayLevels = true;
        
        [Tooltip("Add section headers between different level types")]
        public bool addSectionHeaders = true;

        [Header("Layout")]
        [Tooltip("Spacing between level buttons")]
        public float buttonSpacing = 10f;

        private List<GameObject> generatedButtons = new List<GameObject>();

        private void Start()
        {
            PopulateLevelButtons();
        }

        /// <summary>
        /// Clear and regenerate all level buttons
        /// </summary>
        public void PopulateLevelButtons()
        {
            // Clear existing buttons
            ClearGeneratedButtons();

            if (LevelManager.Instance == null)
            {
                Debug.LogError("[DynamicLevelSelector] LevelManager instance not found!");
                return;
            }

            if (levelButtonContainer == null || levelButtonPrefab == null)
            {
                Debug.LogError("[DynamicLevelSelector] Required UI references not set!");
                return;
            }

            // Add tutorial levels
            if (showTutorials)
            {
                var tutorials = LevelManager.Instance.GetLevels(SceneType.TutorialLevel);
                if (tutorials.Count > 0)
                {
                    if (addSectionHeaders)
                        CreateSectionHeader("Tutorials");
                    
                    foreach (var tutorial in tutorials)
                    {
                        CreateLevelButton(tutorial);
                    }
                }
            }

            // Add gameplay levels
            if (showGameplayLevels)
            {
                var levels = LevelManager.Instance.GetLevels(SceneType.GameplayLevel);
                if (levels.Count > 0)
                {
                    if (addSectionHeaders)
                        CreateSectionHeader("Levels");
                    
                    foreach (var level in levels)
                    {
                        CreateLevelButton(level);
                    }
                }
            }

            // Update layout
            UpdateLayout();
        }

        /// <summary>
        /// Create a section header
        /// </summary>
        private void CreateSectionHeader(string title)
        {
            if (sectionHeaderPrefab == null) return;

            var headerObj = Instantiate(sectionHeaderPrefab, levelButtonContainer);
            
            // Try TextMeshPro first, then fall back to legacy Text
            var headerTextTMP = headerObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (headerTextTMP != null)
            {
                headerTextTMP.text = title;
            }
            else
            {
                var headerText = headerObj.GetComponentInChildren<UnityEngine.UI.Text>();
                if (headerText != null)
                {
                    headerText.text = title;
                }
                else
                {
                    Debug.LogWarning($"[DynamicLevelSelector]: No text component found in section header prefab for '{title}'");
                }
            }

            generatedButtons.Add(headerObj);
        }

        /// <summary>
        /// Create a button for a specific level
        /// </summary>
        private void CreateLevelButton(LevelManager.LevelInfo levelInfo)
        {
            var buttonObj = Instantiate(levelButtonPrefab, levelButtonContainer);
            generatedButtons.Add(buttonObj);

            // Configure the SceneButton component
            var sceneButton = buttonObj.GetComponent<SceneButton>();
            if (sceneButton != null)
            {
                sceneButton.sceneIndex = levelInfo.buildIndex;
                
                // Update lock state based on progression
                UpdateButtonLockState(sceneButton, levelInfo);
            }

            // Update button text/image if available
            UpdateButtonAppearance(buttonObj, levelInfo);
        }

        /// <summary>
        /// Update button appearance with level information
        /// </summary>
        private void UpdateButtonAppearance(GameObject buttonObj, LevelManager.LevelInfo levelInfo)
        {
            // Update button text - try TextMeshPro first, then fall back to legacy Text
            var buttonTextTMP = buttonObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (buttonTextTMP != null)
            {
                buttonTextTMP.text = levelInfo.displayName;
            }
            else
            {
                var buttonText = buttonObj.GetComponentInChildren<UnityEngine.UI.Text>();
                if (buttonText != null)
                {
                    buttonText.text = levelInfo.displayName;
                }
                else
                {
                    Debug.LogWarning($"[DynamicLevelSelector]: No text component found in button for '{levelInfo.displayName}'");
                }
            }

            // Update button image if preview is available
            if (levelInfo.previewImage != null)
            {
                var buttonImage = buttonObj.GetComponent<UnityEngine.UI.Image>();
                if (buttonImage != null)
                {
                    buttonImage.sprite = levelInfo.previewImage;
                }
            }

            // Add goal information as a tooltip or sub-text
            AddGoalInformation(buttonObj, levelInfo);
        }

        /// <summary>
        /// Add goal information to the button
        /// </summary>
        private void AddGoalInformation(GameObject buttonObj, LevelManager.LevelInfo levelInfo)
        {
            // Look for a secondary text component for goals - try TextMeshPro first
            var textsTMP = buttonObj.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
            if (textsTMP.Length > 1)
            {
                var goalText = textsTMP[1]; // Assume second text component is for goals
                var goals = new List<string>();
                
                if (levelInfo.parMoves > 0)
                    goals.Add($"Moves: {levelInfo.parMoves}");
                if (levelInfo.parTime > 0f)
                    goals.Add($"Time: {FormatTime(levelInfo.parTime)}");

                goalText.text = goals.Count > 0 ? string.Join(" | ", goals) : "";
            }
            else
            {
                // Fall back to legacy Text components
                var texts = buttonObj.GetComponentsInChildren<UnityEngine.UI.Text>();
                if (texts.Length > 1)
                {
                    var goalText = texts[1]; // Assume second text component is for goals
                    var goals = new List<string>();
                    
                    if (levelInfo.parMoves > 0)
                        goals.Add($"Moves: {levelInfo.parMoves}");
                    if (levelInfo.parTime > 0f)
                        goals.Add($"Time: {FormatTime(levelInfo.parTime)}");

                    goalText.text = goals.Count > 0 ? string.Join(" | ", goals) : "";
                }
            }
        }

        /// <summary>
        /// Update button lock state
        /// </summary>
        private void UpdateButtonLockState(SceneButton sceneButton, LevelManager.LevelInfo levelInfo)
        {
            bool canLoad = LevelManager.Instance.CanLoadLevel(levelInfo);
            
            // Enable/disable the button
            var button = sceneButton.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                button.interactable = canLoad;
            }

            // Update lock overlay if it exists
            if (sceneButton.lockOverlay != null)
            {
                sceneButton.lockOverlay.SetActive(!canLoad);
            }
        }

        /// <summary>
        /// Clear all generated buttons
        /// </summary>
        private void ClearGeneratedButtons()
        {
            foreach (var button in generatedButtons)
            {
                if (button != null)
                {
                    // Use regular Destroy instead of DestroyImmediate for better performance
                    // DestroyImmediate can cause frame hitches
                    Destroy(button);
                }
            }
            generatedButtons.Clear();
        }

        /// <summary>
        /// Update the layout of the level buttons
        /// </summary>
        private void UpdateLayout()
        {
            // If using a layout group, force rebuild
            var layoutGroup = levelButtonContainer.GetComponent<LayoutGroup>();
            if (layoutGroup != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(levelButtonContainer.GetComponent<RectTransform>());
            }
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

        /// <summary>
        /// Refresh the level selection (useful when progression changes)
        /// </summary>
        public void RefreshLevelSelection()
        {
            PopulateLevelButtons();
        }

        /// <summary>
        /// Called when a level is completed to update UI
        /// </summary>
        public void OnLevelCompleted()
        {
            // Refresh lock states without fully regenerating
            if (generatedButtons.Count > 0)
            {
                RefreshButtonStates();
            }
        }

        /// <summary>
        /// Refresh just the button states without regenerating
        /// </summary>
        private void RefreshButtonStates()
        {
            foreach (var buttonObj in generatedButtons)
            {
                var sceneButton = buttonObj.GetComponent<SceneButton>();
                if (sceneButton != null)
                {
                    var levelInfo = LevelManager.Instance.GetLevelByBuildIndex(sceneButton.sceneIndex);
                    if (levelInfo != null)
                    {
                        UpdateButtonLockState(sceneButton, levelInfo);
                    }
                }
            }
        }

        // Development helper
        [ContextMenu("Refresh Level Selection")]
        private void RefreshLevelSelectionContext()
        {
            RefreshLevelSelection();
        }
    }
}