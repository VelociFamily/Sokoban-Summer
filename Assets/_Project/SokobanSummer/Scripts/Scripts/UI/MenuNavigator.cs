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
        public string defaultPanelName = "Main Menu";

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
                    Debug.Log($"[MenuNavigator]: Initializing panel '{panel.panelName}' as hidden");
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
            Debug.Log($"[MenuNavigator]: Showing default panel '{defaultPanelName}' on Start");
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

            // If already showing this panel, do nothing
            if (currentPanel == panel)
            {
                Debug.Log($"[MenuNavigator]: Panel '{panelName}' is already showing");
                return;
            }

            // Hide current panel and show new panel
            if (currentPanel != null)
            {
                Debug.Log($"[MenuNavigator]: Hiding panel '{currentPanel.panelName}'");
                SetPanelVisibility(currentPanel, false, instant);
            }

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
        public void ShowMainMenu() => ShowPanel("Main Menu");

        /// <summary>
        /// Show settings panel
        /// </summary>
        public void ShowSettings() => ShowPanel("Settings");

        /// <summary>
        /// Show level selection panel
        /// </summary>
        public void ShowLevelSelection() => ShowPanel("Level Selection Menu");

        /// <summary>
        /// Show credits panel
        /// </summary>
        public void ShowCredits() => ShowPanel("Credits");

        /// <summary>
        /// Show achievements panel
        /// </summary>
        public void ShowAchievements() => ShowPanel("Achievements");

        /// <summary>
        /// Show accessories manager panel
        /// </summary>
        public void ShowAccessoriesManager() => ShowPanel("Accessories manager");

        /// <summary>
        /// Go back to main menu (common back button action)
        /// </summary>
        public void BackToMainMenu() => ShowMainMenu();

        /// <summary>
        /// Sets panel visibility using CanvasGroup
        /// </summary>
        private void SetPanelVisibility(MenuPanel panel, bool visible, bool instant = false)
        {
            if (panel.canvasGroup == null)
                return;

            float targetAlpha = visible ? 1f : 0f;

            Debug.Log($"[MenuNavigator]: SetPanelVisibility request panel='{panel.panelName}' visible={visible} instant={instant} targetAlpha={targetAlpha}");

            if (instant || transitionDuration <= 0f)
            {
                panel.canvasGroup.alpha = targetAlpha;
                panel.canvasGroup.interactable = visible;
                panel.canvasGroup.blocksRaycasts = visible;
                
                // Ensure active state matches visibility
                if (visible && !panel.canvasGroup.gameObject.activeSelf)
                {
                    panel.canvasGroup.gameObject.SetActive(true);
                }
                
                Debug.Log($"[MenuNavigator]: Instant visibility applied to '{panel.panelName}' alpha={panel.canvasGroup.alpha} interactable={panel.canvasGroup.interactable}");
            }
            else
            {
                // Stop any existing transition for this panel
                if (activeTransitions.TryGetValue(panel.canvasGroup, out var existingCoroutine))
                {
                    StopCoroutine(existingCoroutine);
                    activeTransitions.Remove(panel.canvasGroup);
                    Debug.Log($"[MenuNavigator]: Stopped existing transition for '{panel.panelName}' to start new one");
                }

                // Start new transition
                var coroutine = StartCoroutine(FadePanel(panel.canvasGroup, targetAlpha, visible));
                activeTransitions[panel.canvasGroup] = coroutine;
                Debug.Log($"[MenuNavigator]: Started fade transition for '{panel.panelName}' -> targetAlpha={targetAlpha}");
            }
        }

        /// <summary>
        /// Smoothly fades a panel to target alpha
        /// </summary>
        private IEnumerator FadePanel(CanvasGroup canvasGroup, float targetAlpha, bool visible)
        {
            float startAlpha = canvasGroup.alpha;
            float elapsed = 0f;

            Debug.Log($"[MenuNavigator]: FadePanel begin (visible={visible}) startAlpha={startAlpha} targetAlpha={targetAlpha}");

            // Enable interaction immediately if showing
            if (visible)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                
                // Ensure the GameObject is active if it was disabled
                if (!canvasGroup.gameObject.activeSelf)
                {
                    canvasGroup.gameObject.SetActive(true);
                }
                
                Debug.Log($"[MenuNavigator]: Interaction enabled at fade start");
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
                
                // Optionally disable the GameObject to save performance, but be careful if other scripts need it active
                // canvasGroup.gameObject.SetActive(false); 
                
                Debug.Log($"[MenuNavigator]: Interaction disabled after hide");
            }

            // Remove from active transitions
            activeTransitions.Remove(canvasGroup);
            Debug.Log($"[MenuNavigator]: FadePanel complete finalAlpha={canvasGroup.alpha}");
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
