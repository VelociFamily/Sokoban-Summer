using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    /// <summary>
    /// Manages navigation between different menu panels using CanvasGroup-based visibility.
    /// Replaces SetActive toggling with smooth alpha transitions.
    /// </summary>
    public class MenuNavigator : MonoBehaviour
    {
        [System.Serializable]
        public class MenuPanel
        {
            public string panelName;
            public CanvasGroup canvasGroup;
            public GameObject firstSelectedButton; // For gamepad/keyboard navigation
        }

        [Header("Menu Panels")]
        [Tooltip("All menu panels managed by this navigator")]
        public List<MenuPanel> menuPanels = new List<MenuPanel>();

        [Header("Settings")]
        [Tooltip("Default panel to show when starting")]
        public string defaultPanelName = "MainMenu";

        [Tooltip("Transition duration for fading panels")]
        public float transitionDuration = 0.2f;

        private MenuPanel currentPanel;
        private Dictionary<string, MenuPanel> panelLookup;
        private Dictionary<CanvasGroup, Coroutine> activeTransitions = new Dictionary<CanvasGroup, Coroutine>();

        private void Awake()
        {
            // Build lookup dictionary
            panelLookup = new Dictionary<string, MenuPanel>();
            foreach (var panel in menuPanels)
            {
                if (panel.canvasGroup != null)
                {
                    panelLookup[panel.panelName] = panel;
                    // Initialize all panels as hidden
                    SetPanelVisibility(panel, false, instant: true);
                }
                else
                {
                    Debug.LogWarning($"[MenuNavigator]: Panel '{panel.panelName}' has no CanvasGroup assigned!");
                }
            }

            Debug.Log($"[MenuNavigator]: Initialized with {panelLookup.Count} menu panels");
        }

        private void Start()
        {
            // Show default panel
            ShowPanel(defaultPanelName, instant: true);
        }

        /// <summary>
        /// Show a specific panel by name, hiding all others
        /// </summary>
        public void ShowPanel(string panelName, bool instant = false)
        {
            if (!panelLookup.TryGetValue(panelName, out var panel))
            {
                Debug.LogError($"[MenuNavigator]: Panel '{panelName}' not found!");
                return;
            }

            // Hide current panel
            if (currentPanel != null && currentPanel != panel)
            {
                SetPanelVisibility(currentPanel, false, instant);
            }

            // Show new panel
            SetPanelVisibility(panel, true, instant);
            currentPanel = panel;

            // Set first selected button for input navigation
            if (panel.firstSelectedButton != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(panel.firstSelectedButton);
            }

            Debug.Log($"[MenuNavigator]: Switched to panel '{panelName}'");
        }

        /// <summary>
        /// Show main menu panel
        /// </summary>
        public void ShowMainMenu() => ShowPanel("MainMenu");

        /// <summary>
        /// Show settings panel
        /// </summary>
        public void ShowSettings() => ShowPanel("Settings");

        /// <summary>
        /// Show level selection panel
        /// </summary>
        public void ShowLevelSelection() => ShowPanel("LevelSelection");

        /// <summary>
        /// Show credits panel
        /// </summary>
        public void ShowCredits() => ShowPanel("Credits");

        /// <summary>
        /// Show achievements panel
        /// </summary>
        public void ShowAchievements() => ShowPanel("Achievements");

        /// <summary>
        /// Go back to main menu (common back button action)
        /// </summary>
        public void BackToMainMenu() => ShowMainMenu();

        /// <summary>
        /// Sets panel visibility using CanvasGroup
        /// </summary>
        private void SetPanelVisibility(MenuPanel panel, bool visible, bool instant = false)
        {
            if (panel.canvasGroup == null) return;

            float targetAlpha = visible ? 1f : 0f;

            if (instant || transitionDuration <= 0f)
            {
                panel.canvasGroup.alpha = targetAlpha;
                panel.canvasGroup.interactable = visible;
                panel.canvasGroup.blocksRaycasts = visible;
            }
            else
            {
                // Stop any existing transition for this panel
                if (activeTransitions.TryGetValue(panel.canvasGroup, out var existingCoroutine))
                {
                    StopCoroutine(existingCoroutine);
                    activeTransitions.Remove(panel.canvasGroup);
                }

                // Start new transition
                var coroutine = StartCoroutine(FadePanel(panel.canvasGroup, targetAlpha, visible));
                activeTransitions[panel.canvasGroup] = coroutine;
            }
        }

        /// <summary>
        /// Smoothly fades a panel to target alpha
        /// </summary>
        private IEnumerator FadePanel(CanvasGroup canvasGroup, float targetAlpha, bool visible)
        {
            float startAlpha = canvasGroup.alpha;
            float elapsed = 0f;

            // Enable interaction immediately if showing
            if (visible)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / transitionDuration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;

            // Disable interaction after hiding
            if (!visible)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            // Remove from active transitions
            activeTransitions.Remove(canvasGroup);
        }

        /// <summary>
        /// Get the currently active panel name
        /// </summary>
        public string GetCurrentPanelName()
        {
            return currentPanel?.panelName ?? "None";
        }

        /// <summary>
        /// Check if a specific panel is currently showing
        /// </summary>
        public bool IsPanelShowing(string panelName)
        {
            return currentPanel != null && currentPanel.panelName == panelName;
        }

        /// <summary>
        /// Get reference to a specific panel by name
        /// </summary>
        public MenuPanel GetPanel(string panelName)
        {
            panelLookup.TryGetValue(panelName, out var panel);
            return panel;
        }

        /// <summary>
        /// Hide all panels (useful for gameplay transitions)
        /// </summary>
        public void HideAllPanels(bool instant = false)
        {
            foreach (var panel in menuPanels)
            {
                if (panel.canvasGroup != null)
                {
                    SetPanelVisibility(panel, false, instant);
                }
            }
            currentPanel = null;
        }
    }
}
