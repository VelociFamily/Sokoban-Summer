using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Core
{
    /// <summary>
    /// Manages persistent UI elements across scene transitions using CanvasGroup-based visibility.
    /// Replaces MenuPersistence's SetActive toggling approach with alpha-based show/hide.
    /// Ensures single EventSystem and AudioListener throughout the session.
    /// </summary>
    public class PersistentUIManager : MonoBehaviour
    {
        [Header("UI Management")]
        [Tooltip("CanvasGroups for UI elements that should persist across scenes")]
        [SerializeField] private List<CanvasGroup> persistentUIGroups = new List<CanvasGroup>();

        [Header("System Components")]
        [Tooltip("The EventSystem for this persistent UI (should be the only one)")]
        [SerializeField] private EventSystem eventSystem;

        [Tooltip("The AudioListener for this persistent UI (should be the only one)")]
        [SerializeField] private AudioListener audioListener;

        [Header("Visibility Settings")]
        [Tooltip("Show UI in main menu scenes")]
        [SerializeField] private bool showInMainMenu = true;

        [Tooltip("Show UI in gameplay scenes")]
        [SerializeField] private bool showInGameplay = false;

        [Tooltip("Fade duration when showing/hiding UI (seconds)")]
        [SerializeField] private float fadeDuration = 0.3f;

        private static PersistentUIManager _instance;
        private Dictionary<CanvasGroup, Coroutine> activeTransitions = new Dictionary<CanvasGroup, Coroutine>();

        private void Awake()
        {
            // Singleton pattern
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning($"[PersistentUIManager]: Duplicate instance detected on '{gameObject.name}' - destroying");
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Validate EventSystem
            if (eventSystem == null)
            {
                eventSystem = GetComponentInChildren<EventSystem>();
                if (eventSystem == null)
                {
                    Debug.LogWarning("[PersistentUIManager]: No EventSystem assigned or found in children - creating one");
                    var esObj = new GameObject("EventSystem");
                    esObj.transform.SetParent(transform);
                    eventSystem = esObj.AddComponent<EventSystem>();
                    esObj.AddComponent<StandaloneInputModule>();
                }
            }

            // Validate AudioListener
            if (audioListener == null)
            {
                audioListener = GetComponentInChildren<AudioListener>();
            }

            // Auto-discover CanvasGroups if none assigned
            if (persistentUIGroups.Count == 0)
            {
                persistentUIGroups.AddRange(GetComponentsInChildren<CanvasGroup>());
                Debug.Log($"[PersistentUIManager]: Auto-discovered {persistentUIGroups.Count} CanvasGroups");
            }

            // Initialize UI as visible by default (will be hidden when gameplay scenes load)
            foreach (var canvasGroup in persistentUIGroups)
            {
                if (canvasGroup != null)
                {
                    SetCanvasGroupVisibility(canvasGroup, true);
                }
            }

            // Subscribe to scene events
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            Debug.Log("[PersistentUIManager]: Initialized successfully");
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                SceneManager.sceneLoaded -= OnSceneLoaded;
                SceneManager.sceneUnloaded -= OnSceneUnloaded;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"[PersistentUIManager]: OnSceneLoaded - Scene: '{scene.name}', Mode: {mode}, BuildIndex: {scene.buildIndex}, MyBuildIndex: {gameObject.scene.buildIndex}");
            
            // Remove any duplicate EventSystems or AudioListeners from newly loaded scenes
            if (mode == LoadSceneMode.Additive)
            {
                RemoveDuplicateEventSystems();
                RemoveDuplicateAudioListeners();
            }

            // Don't update visibility when the PersistentUI scene itself is loaded
            if (scene.buildIndex == gameObject.scene.buildIndex)
            {
                Debug.Log($"[PersistentUIManager]: Skipping visibility update for own scene '{scene.name}'");
                return;
            }

            // Update UI visibility based on scene type
            UpdateUIVisibility(scene);
        }

        private void OnSceneUnloaded(Scene scene)
        {
            // Could add cleanup logic here if needed
            Debug.Log($"[PersistentUIManager]: Scene '{scene.name}' unloaded");
        }

        /// <summary>
        /// Updates UI visibility based on the current scene type
        /// </summary>
        private void UpdateUIVisibility(Scene scene)
        {
            bool shouldShow = false;

            // Always show in the Game initialization scene
            if (scene.name == "Game" || scene.name == "game")
            {
                shouldShow = true;
                Debug.Log($"[PersistentUIManager]: Game initialization scene detected - keeping UI visible");
            }
            else if (SceneInfo.IsMainMenuScene(scene))
            {
                shouldShow = showInMainMenu;
            }
            else if (SceneInfo.IsGameplayScene(scene))
            {
                shouldShow = showInGameplay;
            }

            Debug.Log($"[PersistentUIManager]: Scene '{scene.name}' loaded - UI should be {(shouldShow ? "visible" : "hidden")}");

            if (shouldShow)
            {
                ShowUI();
            }
            else
            {
                HideUI();
            }
        }

        /// <summary>
        /// Shows all persistent UI elements with optional fade
        /// </summary>
        public void ShowUI(bool animated = true)
        {
            Debug.Log($"[PersistentUIManager]: ShowUI called - CanvasGroups count: {persistentUIGroups.Count}, Animated: {animated}");
            
            foreach (var canvasGroup in persistentUIGroups)
            {
                if (canvasGroup == null)
                {
                    Debug.LogWarning("[PersistentUIManager]: Null CanvasGroup in list");
                    continue;
                }

                Debug.Log($"[PersistentUIManager]: Setting CanvasGroup '{canvasGroup.name}' to visible");
                
                if (animated && fadeDuration > 0)
                {
                    StartTransition(canvasGroup, 1f);
                }
                else
                {
                    SetCanvasGroupVisibility(canvasGroup, true);
                }
            }

            Debug.Log("[PersistentUIManager]: ShowUI completed");
        }

        /// <summary>
        /// Hides all persistent UI elements with optional fade
        /// </summary>
        public void HideUI(bool animated = true)
        {
            Debug.Log($"[PersistentUIManager]: HideUI called - CanvasGroups count: {persistentUIGroups.Count}, Animated: {animated}");
            
            foreach (var canvasGroup in persistentUIGroups)
            {
                if (canvasGroup == null) continue;

                Debug.Log($"[PersistentUIManager]: Setting CanvasGroup '{canvasGroup.name}' to hidden");
                
                if (animated && fadeDuration > 0)
                {
                    StartTransition(canvasGroup, 0f);
                }
                else
                {
                    SetCanvasGroupVisibility(canvasGroup, false);
                }
            }

            Debug.Log("[PersistentUIManager]: HideUI completed");
        }

        /// <summary>
        /// Starts a fade transition for a CanvasGroup
        /// </summary>
        private void StartTransition(CanvasGroup canvasGroup, float targetAlpha)
        {
            // Stop any existing transition for this CanvasGroup
            if (activeTransitions.TryGetValue(canvasGroup, out var existingCoroutine))
            {
                StopCoroutine(existingCoroutine);
            }

            // Start new transition
            var coroutine = StartCoroutine(FadeCanvasGroup(canvasGroup, targetAlpha));
            activeTransitions[canvasGroup] = coroutine;
        }

        /// <summary>
        /// Coroutine to fade a CanvasGroup to target alpha
        /// </summary>
        private System.Collections.IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha)
        {
            float startAlpha = canvasGroup.alpha;
            float elapsed = 0f;

            // Enable interaction immediately when fading in
            if (targetAlpha > 0.5f)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
            }

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / fadeDuration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;

            // Disable interaction when fully hidden
            if (targetAlpha < 0.1f)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            activeTransitions.Remove(canvasGroup);
        }

        /// <summary>
        /// Sets CanvasGroup visibility immediately without animation
        /// </summary>
        private void SetCanvasGroupVisibility(CanvasGroup canvasGroup, bool visible)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.blocksRaycasts = visible;
            canvasGroup.interactable = visible;
        }

        /// <summary>
        /// Removes duplicate EventSystems from the scene, keeping only the persistent one
        /// </summary>
        private void RemoveDuplicateEventSystems()
        {
            var allEventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);

            if (allEventSystems.Length <= 1) return;

            Debug.LogWarning($"[PersistentUIManager]: Found {allEventSystems.Length} EventSystems - removing duplicates");

            foreach (var es in allEventSystems)
            {
                // Keep our persistent EventSystem, destroy others
                if (es != eventSystem && es != null)
                {
                    Debug.Log($"[PersistentUIManager]: Removing duplicate EventSystem from '{es.gameObject.name}'");
                    Destroy(es.gameObject);
                }
            }
        }

        /// <summary>
        /// Removes or disables duplicate AudioListeners from the scene, keeping only the persistent one
        /// </summary>
        private void RemoveDuplicateAudioListeners()
        {
            var allListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);

            if (allListeners.Length <= 1) return;

            Debug.LogWarning($"[PersistentUIManager]: Found {allListeners.Length} AudioListeners - disabling duplicates");

            foreach (var listener in allListeners)
            {
                // Keep our persistent AudioListener (if we have one), disable others
                if (listener != audioListener && listener != null)
                {
                    Debug.Log($"[PersistentUIManager]: Disabling duplicate AudioListener on '{listener.gameObject.name}'");
                    listener.enabled = false;
                }
            }
        }

        /// <summary>
        /// Public API to manually show UI from external scripts
        /// </summary>
        public static void Show(bool animated = true)
        {
            _instance?.ShowUI(animated);
        }

        /// <summary>
        /// Public API to manually hide UI from external scripts
        /// </summary>
        public static void Hide(bool animated = true)
        {
            _instance?.HideUI(animated);
        }

        /// <summary>
        /// Check if the PersistentUIManager instance exists
        /// </summary>
        public static bool Exists => _instance != null;
    }
}
