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
        [Tooltip("CanvasGroups for UI elements that should persist across scenes (menus, etc)")]
        [SerializeField] private List<CanvasGroup> persistentUIGroups = new List<CanvasGroup>();

        [Tooltip("CanvasGroups that should ONLY show during gameplay (like move/timer stats)")]
        [SerializeField] private List<CanvasGroup> gameplayOnlyUIGroups = new List<CanvasGroup>();

        [Header("System Components")]
        [Tooltip("The EventSystem for this persistent UI (should be the only one)")]
        [SerializeField] private EventSystem eventSystem;

        [Tooltip("The AudioListener for this persistent UI (should be the only one)")]
        [SerializeField] private AudioListener audioListener;

        [Header("Visibility Settings")]
        [Tooltip("Show UI in main menu scenes")]
        [SerializeField] private bool showInMainMenu = true;

        [Tooltip("Show UI in gameplay scenes")]
        [SerializeField] private bool showInGameplay = true;

        [Tooltip("Fade duration when showing/hiding UI (seconds)")]
        [SerializeField] private float fadeDuration = 0.3f;

        private static PersistentUIManager _instance;
        private Dictionary<CanvasGroup, Coroutine> activeTransitions = new Dictionary<CanvasGroup, Coroutine>();

        private void Awake()
        {
            // Singleton pattern
            if (_instance != null && _instance != this)
            {
                // Keep the first instance alive; disable this duplicate component but leave its GameObject intact to avoid
                // deleting other UI elements that may be on the same object.
                Debug.LogWarning($"[PersistentUIManager]: Duplicate instance detected on '{gameObject.name}' - disabling this component and keeping the original");
                enabled = false;
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize menu UI as visible by default
            foreach (var canvasGroup in persistentUIGroups)
            {
                if (canvasGroup != null)
                {
                    SetCanvasGroupVisibility(canvasGroup, true);
                }
            }

            // DO NOT initialize gameplayOnlyUIGroups - GameplayUIController has exclusive control

            // Subscribe to scene events
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            Debug.Log("[PersistentUIManager]: Initialized successfully");
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

            // Always update visibility so returning to menus after pausing/gameplay restores the correct state
            Debug.Log($"[PersistentUIManager]: Updating UI visibility for scene '{scene.name}'");
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
            bool isGameplay = SceneInfo.IsGameplayScene(scene);
            bool isMainMenu = SceneInfo.IsMainMenuScene(scene);
            bool isGameInit = scene.name == "Game" || scene.name == "game";

            Debug.Log($"[PersistentUIManager]: UpdateUIVisibility('{scene.name}') - IsGameplay={isGameplay}, IsMainMenu={isMainMenu}, IsGameInit={isGameInit}");

            // Menu UI: show in menus and game init, hide in gameplay
            bool showMenuUI = (isMainMenu && showInMainMenu) || isGameInit;

            Debug.Log($"[PersistentUIManager]: MenuUI should be {(showMenuUI ? "visible" : "hidden")}");

            // Update menu UI visibility
            foreach (var canvasGroup in persistentUIGroups)
            {
                if (canvasGroup == null) continue;
                SetCanvasGroupVisibility(canvasGroup, showMenuUI);
            }

            // DO NOT manage gameplayOnlyUIGroups here - GameplayUIController has sole authority over gameplay canvas and its children
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
                    Debug.Log($"[PersistentUIManager]: Starting fade transition for '{canvasGroup.name}' to alpha 1.0");
                    StartTransition(canvasGroup, 1f);
                }
                else
                {
                    Debug.Log($"[PersistentUIManager]: Setting '{canvasGroup.name}' visible immediately (no animation)");
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
                    Debug.Log($"[PersistentUIManager]: Starting fade transition for '{canvasGroup.name}' to alpha 0.0");
                    StartTransition(canvasGroup, 0f);
                }
                else
                {
                    Debug.Log($"[PersistentUIManager]: Setting '{canvasGroup.name}' hidden immediately (no animation)");
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

            // If we don't have a reference, try to find one on this object
            if (eventSystem == null) eventSystem = GetComponent<EventSystem>();

            // If still null, pick the first one found as the "keeper" to avoid destroying all of them
            if (eventSystem == null && allEventSystems.Length > 0)
            {
                eventSystem = allEventSystems[0];
                Debug.Log($"[PersistentUIManager]: No EventSystem assigned, adopting '{eventSystem.gameObject.name}' as the persistent one");
            }

            foreach (var es in allEventSystems)
            {
                // Keep our persistent EventSystem, destroy others
                if (es != eventSystem && es != null)
                {
                    Debug.Log($"[PersistentUIManager]: Removing duplicate EventSystem component from '{es.gameObject.name}'");
                    Destroy(es);
                    
                    // Also destroy the InputSystemUIInputModule if present
                    var inputModule = es.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                    if (inputModule != null)
                    {
                        Destroy(inputModule);
                    }
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

            // If we don't have a reference, try to find one on this object
            if (audioListener == null) audioListener = GetComponent<AudioListener>();

            // If still null, pick the first one found as the "keeper"
            if (audioListener == null && allListeners.Length > 0)
            {
                audioListener = allListeners[0];
                Debug.Log($"[PersistentUIManager]: No AudioListener assigned, adopting '{audioListener.gameObject.name}' as the persistent one");
            }

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
