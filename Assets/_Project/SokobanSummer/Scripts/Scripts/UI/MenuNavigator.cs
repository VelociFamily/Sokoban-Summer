using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Core;

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
        private InputService inputService;

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
            
            // Subscribe to scene events to hide menus when gameplay starts
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnEnable()
        {
            if (ServiceLocator.TryGet(out inputService))
            {
                inputService.OnUICancel += HandleUICancel;
                inputService.EnableUIInput();
            }
            else
            {
                Debug.LogWarning("[MenuNavigator]: InputService not available - UI cancel to main menu will be disabled");
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from scene events
            SceneManager.sceneLoaded -= OnSceneLoaded;

            if (inputService != null)
            {
                inputService.OnUICancel -= HandleUICancel;
                inputService = null;
            }
        }

        private void OnDisable()
        {
            if (inputService != null)
            {
                inputService.OnUICancel -= HandleUICancel;
                inputService = null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // When a gameplay scene is loaded additively and becomes active, hide all menu panels
            // For single scene loading, this happens immediately
            // For additive loading, only hide if the gameplay scene is or becomes active
            if (Core.SceneInfo.IsGameplayScene(scene))
            {
                // If loaded scene is now active, hide immediately
                if (SceneManager.GetActiveScene() == scene)
                {
                    Debug.Log($"[MenuNavigator]: Gameplay scene '{scene.name}' is active - hiding all menu panels");
                    HideAllPanels(instant: true);
                }
                else if (mode == LoadSceneMode.Single)
                {
                    // Single mode always makes the scene active eventually
                    Debug.Log($"[MenuNavigator]: Gameplay scene '{scene.name}' loaded in Single mode - hiding all menu panels");
                    HideAllPanels(instant: true);
                }
                else
                {
                    Debug.Log($"[MenuNavigator]: Gameplay scene '{scene.name}' loaded additively but not active yet - will check on active scene change");
                }
            }
        }

        private void Start()
        {
            // Defer showing default panel to ensure scene state is settled
            StartCoroutine(ShowDefaultPanelWhenReady());
        }

        private IEnumerator ShowDefaultPanelWhenReady()
        {
            // Wait a frame for scene loading to settle
            yield return null;
            
            // Show default panel only if we're not in a gameplay scene
            var activeScene = SceneManager.GetActiveScene();
            if (!Core.SceneInfo.IsGameplayScene(activeScene))
            {
                Debug.Log($"[MenuNavigator]: Showing default panel '{defaultPanelName}' after scene settled");
                ShowPanel(defaultPanelName, instant: true);
            }
            else
            {
                Debug.Log($"[MenuNavigator]: Active scene is gameplay - skipping default panel display");
            }
        }

        /// <summary>
        /// Show a specific panel by name, hiding all others
        /// </summary>
        public void ShowPanel(string panelName, bool instant = false)
        {
            if (!panelLookup.TryGetValue(panelName, out var panel))
            {
                // Try case-insensitive lookup as fallback
                panel = menuPanels.Find(p => p.panelName.Equals(panelName, System.StringComparison.OrdinalIgnoreCase));
                if (panel == null)
                {
                    Debug.LogError($"[MenuNavigator]: Panel '{panelName}' not found! Available panels: {string.Join(", ", panelLookup.Keys)}");
                    return;
                }
                Debug.LogWarning($"[MenuNavigator]: Found panel using case-insensitive match for '{panelName}'. Please use exact name.");
            }

            // If already showing this panel, check if it's actually visible
            if (currentPanel == panel)
            {
                // Check if fully visible and active
                if (panel.canvasGroup.alpha > 0.99f && panel.canvasGroup.interactable && panel.canvasGroup.gameObject.activeSelf)
                {
                    Debug.Log($"[MenuNavigator]: Panel '{panelName}' is already showing and visible");
                    return;
                }
                Debug.Log($"[MenuNavigator]: Panel '{panelName}' is current but not fully visible (alpha={panel.canvasGroup.alpha}) - forcing show");
            }

            // Hide current panel if it's different from the new one
            if (currentPanel != null && currentPanel != panel)
            {
                Debug.Log($"[MenuNavigator]: Hiding panel '{currentPanel.panelName}'");
                SetPanelVisibility(currentPanel, false, instant);
            }

            SetPanelVisibility(panel, true, instant);
            currentPanel = panel;

            // Set first selected button for input navigation with a small delay to ensure UI is ready
            if (panel.firstSelectedButton != null && EventSystem.current != null)
            {
                StartCoroutine(SelectButtonNextFrame(panel.firstSelectedButton));
            }

            Debug.Log($"[MenuNavigator]: Switched to panel '{panelName}'");
        }

        /// <summary>
        /// Defers button selection to the next frame to ensure UI is fully ready
        /// </summary>
        private IEnumerator SelectButtonNextFrame(GameObject button)
        {
            yield return null; // Wait one frame for UI to settle
            
            if (button == null)
            {
                Debug.LogWarning("[MenuNavigator]: Button reference became null before selection");
                yield break;
            }
            
            var eventSystem = EventSystem.current;
            if (eventSystem != null)
            {
                eventSystem.SetSelectedGameObject(null); // Clear first
                yield return null;
                
                // Re-check EventSystem after frame wait
                eventSystem = EventSystem.current;
                if (eventSystem != null && button != null)
                {
                    eventSystem.SetSelectedGameObject(button);
                    Debug.Log($"[MenuNavigator]: Selected button '{button.name}' for navigation");
                }
            }
        }

        /// <summary>
        /// Show main menu and ensure gameplay scenes are unloaded.
        /// Safe to call even if no gameplay scenes are loaded.
        /// </summary>
        public void ShowMainMenu() => LoadMenu();

        /// <summary>
        /// Unload any gameplay level scenes, restore persistent UI/background, and show Main Menu.
        /// Mirrors the logic used by PauseButton/CompleteUI for consistency.
        /// </summary>
        public async void LoadMenu()
        {
            // Make sure time scale is normal when returning to menu
            Time.timeScale = 1f;

            // Ensure input is in UI mode during menu navigation
            if (inputService == null)
            {
                ServiceLocator.TryGet(out inputService);
            }
            inputService?.DisablePlayerInput();
            inputService?.EnableUIInput();

            // Preferred path: let LevelManager handle scene transitions if available
            if (ServiceLocator.TryGet<LevelManager>(out var levelManager))
            {
                Debug.Log("[MenuNavigator]: Delegating to LevelManager.LoadMainMenu()");
                levelManager.LoadMainMenu();

                // Ensure persistent UI is visible (and its background logic can kick in)
                if (Core.PersistentUIManager.Exists)
                {
                    Core.PersistentUIManager.Show(animated: true);
                }

                // Show the menu panel instantly to avoid a flash of hidden UI
                ShowPanel("Main Menu", instant: true);
                return;
            }

            // Fallback: manually unload all gameplay scenes
            var unloadOps = new List<AsyncOperation>();
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var loadedScene = SceneManager.GetSceneAt(i);
                if (Core.SceneInfo.IsGameplayScene(loadedScene) && loadedScene.isLoaded)
                {
                    var op = SceneManager.UnloadSceneAsync(loadedScene);
                    if (op != null)
                    {
                        unloadOps.Add(op);
                        Debug.Log($"[MenuNavigator]: Unloading gameplay scene '{loadedScene.name}'");
                    }
                }
            }

            // Await unloads to complete before switching active scene/UI
            foreach (var op in unloadOps)
            {
                while (!op.isDone)
                {
                    await Task.Yield();
                }
            }

            // After unloading gameplay, set a non-gameplay scene active so background shows
            Scene? target = null;
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var s = SceneManager.GetSceneAt(i);
                if (s.isLoaded && !Core.SceneInfo.IsGameplayScene(s))
                {
                    target = s;
                    break;
                }
            }
            if (target.HasValue)
            {
                SceneManager.SetActiveScene(target.Value);
                Debug.Log($"[MenuNavigator]: Active scene set to '{target.Value.name}'");
            }

            // Ensure persistent UI (and its background management) is visible
            if (Core.PersistentUIManager.Exists)
            {
                Core.PersistentUIManager.Show(animated: true);
            }

            // Finally, show the main menu panel
            ShowPanel("Main Menu", instant: true);
        }

        /// <summary>
        /// Show settings panel
        /// </summary>
        public void ShowSettings() => ShowPanel("Settings");

        public void ShowLevelComplete() => ShowPanel("Level Complete");

        /// <summary>
        /// Show level selection panel
        /// </summary>
        public void ShowLevelSelection()
        {
            ShowPanel("Level Selection Menu");
            
            // Ensure DynamicLevelSelector is initialized if it hasn't been already
            // This handles cases where the panel was hidden/inactive and Start() hasn't run or needs a refresh
            if (panelLookup.TryGetValue("Level Selection Menu", out var panel) && panel.canvasGroup != null)
            {
                var selector = panel.canvasGroup.GetComponentInChildren<DynamicLevelSelector>(true);
                if (selector != null)
                {
                    // Force a refresh if needed, or ensure it's active
                    if (!selector.gameObject.activeSelf) selector.gameObject.SetActive(true);
                }
            }
        }

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
        public void ShowAccessoriesManager() => ShowPanel("Accessories Manager");

        /// <summary>
        /// Show pause panel
        /// </summary>
        public void ShowPause() => ShowPanel("Pause");

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
                
                // Consistently manage GameObject active state
                if (visible && !panel.canvasGroup.gameObject.activeSelf)
                {
                    panel.canvasGroup.gameObject.SetActive(true);
                }
                else if (!visible && panel.canvasGroup.gameObject.activeSelf)
                {
                    // Deactivate hidden panels to save resources
                    panel.canvasGroup.gameObject.SetActive(false);
                }
                
                Debug.Log($"[MenuNavigator]: Instant visibility applied to '{panel.panelName}' alpha={panel.canvasGroup.alpha} interactable={panel.canvasGroup.interactable} active={panel.canvasGroup.gameObject.activeSelf}");
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
                // Use unscaled time so fades still complete when the game is paused
                elapsed += Time.unscaledDeltaTime;
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
                
                // Deactivate hidden panels to save performance
                // Safe because this only happens after fade completes
                canvasGroup.gameObject.SetActive(false);
                
                Debug.Log($"[MenuNavigator]: Panel hidden and deactivated");
            }
            else
            {
                // Re-confirm interactivity at the end of show fade for safety
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                Debug.Log($"[MenuNavigator]: Confirming interactivity for fully visible panel");
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

        private void HandleUICancel(InputAction.CallbackContext context)
        {
            // Ignore cancel in gameplay scenes; pause logic handles Escape/Start there
            if (Core.SceneInfo.IsGameplayScene(SceneManager.GetActiveScene()))
            {
                return;
            }

            if (currentPanel == null)
            {
                return;
            }

            var panelName = currentPanel.panelName;

            // Ignore cancel when already on main menu or pause panel (pause menu handles its own input)
            if (string.Equals(panelName, "Main Menu", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(panelName, "Pause", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            ShowMainMenu();
        }
    }
}
