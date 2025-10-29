using UnityEngine;
using Core;

namespace UI
{
    /// <summary>
    /// Example script showing how to integrate the dynamic level system with existing UI
    /// This can be used to upgrade hardcoded level selection screens
    /// </summary>
    public class MainMenuLevelSelector : MonoBehaviour
    {
        [Header("UI Integration")]
        [Tooltip("The DynamicLevelSelector component (add to level selection area)")]
        public DynamicLevelSelector dynamicSelector;
        
        [Tooltip("Legacy level buttons to hide when using dynamic system")]
        public GameObject[] legacyButtons;
        
        [Tooltip("Show dynamic system instead of legacy buttons")]
        public bool useDynamicSystem = true;

        [Header("Configuration")]
        [Tooltip("Automatically hide legacy buttons when dynamic system is enabled")]
        public bool autoHideLegacyButtons = true;

        private void Start()
        {
            SetupLevelSelection();
        }

        /// <summary>
        /// Configure the level selection system
        /// </summary>
        private void SetupLevelSelection()
        {
            if (useDynamicSystem)
            {
                EnableDynamicSystem();
            }
            else
            {
                EnableLegacySystem();
            }
        }

        /// <summary>
        /// Enable the dynamic level selection system
        /// </summary>
        private void EnableDynamicSystem()
        {
            // Ensure LevelManager is available
            if (LevelManager.Instance == null)
            {
                Debug.LogError("[MainMenuLevelSelector] LevelManager not found! Make sure it's in the Game scene.");
                EnableLegacySystem();
                return;
            }

            // Enable dynamic selector if available
            if (dynamicSelector != null)
            {
                dynamicSelector.gameObject.SetActive(true);
                dynamicSelector.PopulateLevelButtons();
                Debug.Log("[MainMenuLevelSelector] Dynamic level system enabled");
            }
            else
            {
                Debug.LogWarning("[MainMenuLevelSelector] DynamicLevelSelector component not assigned!");
                EnableLegacySystem();
                return;
            }

            // Hide legacy buttons if requested
            if (autoHideLegacyButtons)
            {
                HideLegacyButtons();
            }
        }

        /// <summary>
        /// Enable the legacy hardcoded level selection system
        /// </summary>
        private void EnableLegacySystem()
        {
            // Disable dynamic selector
            if (dynamicSelector != null)
            {
                dynamicSelector.gameObject.SetActive(false);
            }

            // Show legacy buttons
            ShowLegacyButtons();
            
            Debug.Log("[MainMenuLevelSelector] Legacy level system enabled");
        }

        /// <summary>
        /// Hide legacy level buttons
        /// </summary>
        private void HideLegacyButtons()
        {
            foreach (var button in legacyButtons)
            {
                if (button != null)
                    button.SetActive(false);
            }
        }

        /// <summary>
        /// Show legacy level buttons
        /// </summary>
        private void ShowLegacyButtons()
        {
            foreach (var button in legacyButtons)
            {
                if (button != null)
                    button.SetActive(true);
            }
        }

        /// <summary>
        /// Toggle between dynamic and legacy systems (useful for testing)
        /// </summary>
        [ContextMenu("Toggle System")]
        public void ToggleSystem()
        {
            useDynamicSystem = !useDynamicSystem;
            SetupLevelSelection();
        }

        /// <summary>
        /// Refresh the level selection (call after level completion)
        /// </summary>
        public void RefreshLevelSelection()
        {
            if (useDynamicSystem && dynamicSelector != null)
            {
                dynamicSelector.RefreshLevelSelection();
            }
        }

        /// <summary>
        /// Called when returning to main menu from a level
        /// </summary>
        public void OnReturnToMenu()
        {
            RefreshLevelSelection();
        }
    }
}