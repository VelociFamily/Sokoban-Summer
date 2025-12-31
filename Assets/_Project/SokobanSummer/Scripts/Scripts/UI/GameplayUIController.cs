using System.Collections.Generic;
using Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Manages gameplay-specific UI elements that live in the Gameplay Canvas of the PersistentUI scene.
    /// Handles pause panel, level complete panel, and move/timer/achievement stats display.
    /// This is separate from MenuNavigator which manages the menu-only panels.
    /// </summary>
    public class GameplayUIController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup gameplayCanvasGroup; // The Gameplay Canvas CanvasGroup
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject levelCompletePanel;
        [SerializeField] private GameObject gameStatsPanel; // Contains Moves, Timer, Achievements
        [SerializeField] private Selectable firstSelectedButton;

        private CanvasGroup pausePanelCanvasGroup;
        private CanvasGroup levelCompletePanelCanvasGroup;
        private CanvasGroup gameStatsPanelCanvasGroup;

        // Track running fades so a previous hide/show can't override a new state after scene changes
        private readonly Dictionary<CanvasGroup, Coroutine> activeFades = new();

        private bool initialized;

        private bool isPaused = false;

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }

        private void Start()
        {
            InitializeUI();
            // Start with gameplay canvas hidden - will be shown when a gameplay scene loads
            if (gameplayCanvasGroup != null)
            {
                gameplayCanvasGroup.alpha = 0f;
                gameplayCanvasGroup.interactable = false;
                gameplayCanvasGroup.blocksRaycasts = false;
            }
            HideAllGameplayPanels(instant: true);
            Debug.Log("[GameplayUIController]: Initialized and hidden all gameplay panels");
        }

        /// <summary>
        /// Initialize and cache all gameplay UI components
        /// </summary>
        private void InitializeUI()
        {
            // Cache CanvasGroups for fade transitions
            if (gameplayCanvasGroup == null)
            {
                gameplayCanvasGroup = GetComponent<CanvasGroup>();
                if (gameplayCanvasGroup == null)
                {
                    Debug.LogError("[GameplayUIController]: No CanvasGroup found on GameObject - cannot manage visibility");
                    return;
                }
            }

            // Find panels if not assigned
            if (pausePanel == null)
                pausePanel = FindPanelByName("Pause Panel", "Pause");
            
            if (levelCompletePanel == null)
                levelCompletePanel = FindPanelByName("Level Complete");

            if (gameStatsPanel == null)
                gameStatsPanel = FindPanelByName("Game Stats");

            // Cache CanvasGroups
            if (pausePanel != null)
            {
                pausePanelCanvasGroup = pausePanel.GetComponent<CanvasGroup>();
                if (pausePanelCanvasGroup == null)
                    pausePanelCanvasGroup = pausePanel.AddComponent<CanvasGroup>();
            }

            if (levelCompletePanel != null)
            {
                levelCompletePanelCanvasGroup = levelCompletePanel.GetComponent<CanvasGroup>();
                if (levelCompletePanelCanvasGroup == null)
                    levelCompletePanelCanvasGroup = levelCompletePanel.AddComponent<CanvasGroup>();
            }

            if (gameStatsPanel != null)
            {
                gameStatsPanelCanvasGroup = gameStatsPanel.GetComponent<CanvasGroup>();
                if (gameStatsPanelCanvasGroup == null)
                    gameStatsPanelCanvasGroup = gameStatsPanel.AddComponent<CanvasGroup>();
            }

            Debug.Log($"[GameplayUIController]: Initialized UI panels - Pause: {pausePanel != null}, LevelComplete: {levelCompletePanel != null}, GameStats: {gameStatsPanel != null}");
            initialized = true;
        }



        /// <summary>
        /// Find a panel by name with fallback candidates
        /// </summary>
        private GameObject FindPanelByName(params string[] candidates)
        {
            foreach (var candidate in candidates)
            {
                var panel = transform.Find(candidate);
                if (panel != null)
                    return panel.gameObject;
            }
            return null;
        }

        /// <summary>
        /// Called when a scene is loaded - show gameplay UI only for gameplay scenes
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!initialized) InitializeUI();

            // Only react to actual level scenes, not the PersistentUI scene itself
            if (scene.name == "PersistentUI" || scene.name == "Game") return;

            if (SceneInfo.IsGameplayScene(scene))
            {
                ShowGameplayUI();
                Debug.Log($"[GameplayUIController]: Gameplay scene loaded '{scene.name}' - showing gameplay UI");
            }
            else
            {
                HideAllGameplayPanels(instant: true);
                Debug.Log($"[GameplayUIController]: Non-gameplay scene loaded '{scene.name}' - hiding gameplay UI");
            }
        }

        /// <summary>
        /// Ensure gameplay UI hides when the active scene switches to a non-gameplay scene (e.g., returning to menu).
        /// </summary>
        private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            if (!initialized) InitializeUI();

            if (SceneInfo.IsGameplayScene(newScene))
            {
                ShowGameplayUI();
                Debug.Log($"[GameplayUIController]: Active scene changed to gameplay '{newScene.name}' - showing gameplay UI");
            }
            else
            {
                HideAllGameplayPanels(instant: true);
                Debug.Log($"[GameplayUIController]: Active scene changed to non-gameplay '{newScene.name}' - hiding gameplay UI");
            }
        }

        /// <summary>
        /// Show gameplay UI canvas (stats, pause button interactive)
        /// </summary>
        public void ShowGameplayUI()
        {
            if (gameplayCanvasGroup == null) return;

            EnsureGameplayCanvasVisible();

            // Clear any leftover panels (pause or level complete) when entering a new gameplay scene
            SetPanelVisible(pausePanel, pausePanelCanvasGroup, false, instant: true);
            SetPanelVisible(levelCompletePanel, levelCompletePanelCanvasGroup, false, instant: true);

            // Show game stats panel immediately
            if (gameStatsPanel != null)
            {
                EnsureCenteredInCanvas(gameStatsPanel.GetComponent<RectTransform>());
                SetPanelVisible(gameStatsPanel, gameStatsPanelCanvasGroup, true, instant: true);
            }

            isPaused = false;
            ApplyFirstSelectedIfAvailable();
            Debug.Log("[GameplayUIController]: Gameplay UI shown");
        }

        /// <summary>
        /// Hide all gameplay UI panels
        /// </summary>
        public void HideAllGameplayPanels(bool instant = false)
        {
            if (gameplayCanvasGroup == null) return;

            gameplayCanvasGroup.alpha = 0f;
            gameplayCanvasGroup.interactable = false;
            gameplayCanvasGroup.blocksRaycasts = false;

            SetPanelVisible(pausePanel, pausePanelCanvasGroup, false, instant);
            SetPanelVisible(levelCompletePanel, levelCompletePanelCanvasGroup, false, instant);
            SetPanelVisible(gameStatsPanel, gameStatsPanelCanvasGroup, false, instant);

            isPaused = false;
            Debug.Log("[GameplayUIController]: All gameplay panels hidden");
        }

        /// <summary>
        /// Show pause panel
        /// </summary>
        public void ShowPausePanel(bool instant = false)
        {
            if (pausePanel == null)
            {
                Debug.LogWarning("[GameplayUIController]: Pause panel not assigned");
                return;
            }

            EnsureGameplayCanvasVisible();
            EnsureCenteredInCanvas(pausePanel.GetComponent<RectTransform>());
            
            // Ensure panel is enabled before fading - critical after DestroyPausePanel sets it inactive
            if (!pausePanel.activeSelf)
            {
                pausePanel.SetActive(true);
            }
            
            // Reset alpha to ensure visibility even if a previous fade left it at 0
            if (pausePanelCanvasGroup != null && pausePanelCanvasGroup.alpha < 0.1f)
            {
                pausePanelCanvasGroup.alpha = instant ? 1f : 0.1f; // Start fade from small value if animating
            }
            
            SetPanelVisible(pausePanel, pausePanelCanvasGroup, true, instant);

            // Keep game stats visible while pause overlay is shown
            if (gameStatsPanel != null)
            {
                SetPanelVisible(gameStatsPanel, gameStatsPanelCanvasGroup, true, instant: true);
            }

            isPaused = true;
            ApplyFirstSelectedIfAvailable();
            Debug.Log("[GameplayUIController]: Pause panel shown");
        }

        /// <summary>
        /// Hide pause panel
        /// </summary>
        public void HidePausePanel(bool instant = false)
        {
            if (pausePanel == null) return;

            SetPanelVisible(pausePanel, pausePanelCanvasGroup, false, instant);
            isPaused = false;
            Debug.Log("[GameplayUIController]: Pause panel hidden");
        }

        // Immediate, non-animated show to avoid flicker and inactive state races
        public void ShowPausePanelImmediate()
        {
            if (pausePanel == null || pausePanelCanvasGroup == null) return;

            // Cancel any running fade on the pause panel
            if (activeFades.TryGetValue(pausePanelCanvasGroup, out var running))
            {
                if (running != null) StopCoroutine(running);
                activeFades.Remove(pausePanelCanvasGroup);
            }

            EnsureGameplayCanvasVisible();
            EnsureCenteredInCanvas(pausePanel.GetComponent<RectTransform>());

            // Ensure active and fully visible immediately
            pausePanel.SetActive(true);
            pausePanelCanvasGroup.alpha = 1f;
            pausePanelCanvasGroup.interactable = true;
            pausePanelCanvasGroup.blocksRaycasts = true;

            // Keep stats visible alongside the pause overlay
            if (gameStatsPanel != null && gameStatsPanelCanvasGroup != null)
            {
                gameStatsPanel.SetActive(true);
                gameStatsPanelCanvasGroup.alpha = 1f;
                gameStatsPanelCanvasGroup.interactable = true;
                gameStatsPanelCanvasGroup.blocksRaycasts = true;
            }

            isPaused = true;
            ApplyFirstSelectedIfAvailable();
            Debug.Log("[GameplayUIController]: Pause panel shown (immediate)");
        }

        // Immediate, non-animated hide for symmetry and stability
        public void HidePausePanelImmediate()
        {
            if (pausePanel == null || pausePanelCanvasGroup == null) return;

            if (activeFades.TryGetValue(pausePanelCanvasGroup, out var running))
            {
                if (running != null) StopCoroutine(running);
                activeFades.Remove(pausePanelCanvasGroup);
            }

            pausePanelCanvasGroup.interactable = false;
            pausePanelCanvasGroup.blocksRaycasts = false;
            pausePanelCanvasGroup.alpha = 0f;
            pausePanel.SetActive(false);

            isPaused = false;
            Debug.Log("[GameplayUIController]: Pause panel hidden (immediate)");
        }

        /// <summary>
        /// Deactivate pause panel GameObject
        /// </summary>
        public void DestroyPausePanel()
        {
            if (pausePanel == null)
            {
                Debug.LogWarning("[GameplayUIController]: Pause panel is null, cannot deactivate");
                return;
            }

            pausePanel.SetActive(false);
            isPaused = false;
            Debug.Log("[GameplayUIController]: Pause panel deactivated and isPaused set to false");
        }

        /// <summary>
        /// Show level complete panel
        /// </summary>
        public void ShowLevelCompletePanel(bool instant = false)
        {
            if (levelCompletePanel == null)
            {
                Debug.LogWarning("[GameplayUIController]: Level complete panel not assigned");
                return;
            }

            EnsureGameplayCanvasVisible();
            EnsureCenteredInCanvas(levelCompletePanel.GetComponent<RectTransform>());
            SetPanelVisible(levelCompletePanel, levelCompletePanelCanvasGroup, true, instant);

            // Keep game stats visible while level complete overlay is shown
            if (gameStatsPanel != null)
            {
                SetPanelVisible(gameStatsPanel, gameStatsPanelCanvasGroup, true, instant: true);
            }

            ApplyFirstSelectedIfAvailable();
            Debug.Log("[GameplayUIController]: Level complete panel shown");
        }

        /// <summary>
        /// Show game stats (moves/timer/achievements)
        /// </summary>
        public void ShowGameStatsPanel(bool instant = false)
        {
            if (gameStatsPanel == null)
            {
                Debug.LogWarning("[GameplayUIController]: Game stats panel not assigned");
                return;
            }

            EnsureGameplayCanvasVisible();
            SetPanelVisible(gameStatsPanel, gameStatsPanelCanvasGroup, true, instant);
            ApplyFirstSelectedIfAvailable();
            Debug.Log("[GameplayUIController]: Game stats panel shown");
        }

        /// <summary>
        /// Helper to set panel visibility with optional fade
        /// </summary>
        private void SetPanelVisible(GameObject panel, CanvasGroup canvasGroup, bool visible, bool instant = false)
        {
            if (panel == null || canvasGroup == null) return;

            // Cancel any previous fade on this canvas to avoid stale fades hiding newly shown UI
            if (activeFades.TryGetValue(canvasGroup, out var running))
            {
                if (running != null) StopCoroutine(running);
                activeFades.Remove(canvasGroup);
            }

            if (visible)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                panel.SetActive(true);
                if (instant)
                {
                    canvasGroup.alpha = 1f;
                }
                else
                {
                    activeFades[canvasGroup] = StartCoroutine(FadePanel(canvasGroup, 1f, 0.3f));
                }
            }
            else
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                if (instant)
                {
                    canvasGroup.alpha = 0f;
                    panel.SetActive(false);
                }
                else
                {
                    activeFades[canvasGroup] = StartCoroutine(FadePanelAndDeactivate(canvasGroup, panel, 0f, 0.3f));
                }
            }
        }

        /// <summary>
        /// Fade panel alpha
        /// </summary>
        private System.Collections.IEnumerator FadePanel(CanvasGroup canvasGroup, float targetAlpha, float duration)
        {
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
        }

        /// <summary>
        /// Fade panel and deactivate when done
        /// </summary>
        private System.Collections.IEnumerator FadePanelAndDeactivate(CanvasGroup canvasGroup, GameObject panel, float targetAlpha, float duration)
        {
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
            panel.SetActive(false);
        }

        /// <summary>
        /// Ensure a RectTransform is centered within its parent canvas, preserving size.
        /// </summary>
        private void EnsureCenteredInCanvas(RectTransform rect)
        {
            if (rect == null) return;
            var size = rect.sizeDelta;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;
        }

        /// <summary>
        /// Check if paused
        /// </summary>
        public bool IsPaused() => isPaused;

        /// <summary>
        /// Make sure gameplay canvas is visible and interactive (used when external callers show a panel).
        /// </summary>
        private void EnsureGameplayCanvasVisible()
        {
            if (gameplayCanvasGroup == null) return;

            gameplayCanvasGroup.alpha = 1f;
            gameplayCanvasGroup.interactable = true;
            gameplayCanvasGroup.blocksRaycasts = true;
        }

        private void ApplyFirstSelectedIfAvailable()
        {
            var es = EventSystem.current;
            if (es == null) return;
            if (firstSelectedButton == null) return;

            var go = firstSelectedButton.gameObject;
            if (!go.activeInHierarchy) return;
            if (!firstSelectedButton.interactable) return;

            es.SetSelectedGameObject(go);
        }
    }
}
