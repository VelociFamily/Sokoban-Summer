using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    public class MoveCounter : MonoBehaviour
    {
        public static MoveCounter Instance;

        [Header("Move Counter")]
        public int moveCount;
        public TextMeshProUGUI moveText;

        [Header("Timer")]
        public TextMeshProUGUI timerText;
        public GameObject levelCompleteCanvas; // Canvas that ends the timer

        private float timer;
        private bool timerRunning = true;

        /// <summary>
        /// Initialize the MoveCounter - can be called by other systems
        /// </summary>
        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                timer = 0f;
                timerRunning = true;
                // Subscribe to scene changes to reset UI references
                SceneManager.sceneLoaded += OnSceneLoaded;
                Debug.Log("[MoveCounter]: Instance initialized and persisted across scenes");
            }
            else
            {
                Debug.LogWarning($"[MoveCounter]: Duplicate instance detected on '{gameObject.name}' - destroying");
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Try to find UI components if not assigned
            TryFindUIComponents();
        }

        /// <summary>
        /// Attempt to find and assign UI components if they're not already assigned
        /// </summary>
        private void TryFindUIComponents()
        {
            var allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);

            // First attempt: look for exact named fields (case-insensitive) - faster and predictable
            if (moveText == null)
            {
                foreach (var t in allTexts)
                {
                    if (t.name.Equals("Moves", StringComparison.OrdinalIgnoreCase))
                    {
                        moveText = t;
                        Debug.Log($"[MoveCounter]: Found exact-named moveText by name '{t.name}' on '{t.transform.parent?.name}'");
                        break;
                    }
                }
            }

            if (timerText == null)
            {
                foreach (var t in allTexts)
                {
                    if (t.name.Equals("Timer", StringComparison.OrdinalIgnoreCase))
                    {
                        timerText = t;
                        Debug.Log($"[MoveCounter]: Found exact-named timerText by name '{t.name}' on '{t.transform.parent?.name}'");
                        break;
                    }
                }
            }

            // Fallback: fuzzy discovery if exact names weren't found
            if (moveText == null)
            {
                moveText = FindBestMatchingText(allTexts, "move", new[] { "move", "moves", "step", "steps" }, new[] { "time", "timer", "second" });
                if (moveText != null)
                {
                    Debug.Log($"[MoveCounter]: Auto-assigned moveText to '{moveText.name}' on '{moveText.transform.parent?.name}'");
                }
            }

            if (timerText == null)
            {
                timerText = FindBestMatchingText(allTexts, "time", new[] { "time", "timer", "clock", "duration" }, new[] { "move", "step", "count" });
                if (timerText != null)
                {
                    Debug.Log($"[MoveCounter]: Auto-assigned timerText to '{timerText.name}' on '{timerText.transform.parent?.name}'");
                }
            }

            if (levelCompleteCanvas == null)
            {
                // Look for a Canvas with "complete" in its name
                var allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
                foreach (var canvas in allCanvases)
                {
                    if (canvas.name.ToLower().Contains("complete"))
                    {
                        levelCompleteCanvas = canvas.gameObject;
                        Debug.Log($"[MoveCounter]: Auto-assigned levelCompleteCanvas to '{canvas.name}'");
                        break;
                    }
                }
            }

            // Update display with current values
            RefreshDisplay();
        }

        /// <summary>
        /// Find the best matching TextMeshProUGUI component using prioritized search terms and exclusions
        /// </summary>
        private TextMeshProUGUI FindBestMatchingText(TextMeshProUGUI[] allTexts, string logName, string[] includeTerms, string[] excludeTerms)
        {
            TextMeshProUGUI bestMatch = null;
            int bestScore = -1;

            if (verboseLogging)
            {
                Debug.Log($"[MoveCounter]: Searching for {logName} text among {allTexts.Length} TextMeshProUGUI components");
            }

            foreach (var text in allTexts)
            {
                var textName = text.name.ToLower();
                var parentName = text.transform.parent?.name.ToLower() ?? "";
                var currentText = text.text.ToLower();
                
                // Skip if it contains exclude terms
                bool shouldExclude = false;
                foreach (var excludeTerm in excludeTerms)
                {
                    if (textName.Contains(excludeTerm) || parentName.Contains(excludeTerm) || currentText.Contains(excludeTerm))
                    {
                        shouldExclude = true;
                        if (verboseLogging) Debug.Log($"[MoveCounter]: Excluding '{text.name}' (parent: '{text.transform.parent?.name}') - contains exclude term '{excludeTerm}'");
                        break;
                    }
                }
                if (shouldExclude) continue;

                // Calculate score based on include terms
                int score = 0;
                foreach (var includeTerm in includeTerms)
                {
                    if (textName.Contains(includeTerm)) score += 3; // Name match is highest priority
                    else if (parentName.Contains(includeTerm)) score += 2; // Parent name match is second priority
                    else if (currentText.Contains(includeTerm)) score += 1; // Current text content is lowest priority
                }

                // Prefer exact matches
                if (textName == logName || parentName == logName) score += 5;

                if (verboseLogging && score > 0)
                {
                    Debug.Log($"[MoveCounter]: Text '{text.name}' (parent: '{text.transform.parent?.name}', content: '{text.text}') scored {score} for {logName}");
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMatch = text;
                }
            }

            if (verboseLogging)
            {
                if (bestMatch != null)
                    Debug.Log($"[MoveCounter]: Best match for {logName}: '{bestMatch.name}' with score {bestScore}");
                else
                    Debug.Log($"[MoveCounter]: No suitable match found for {logName}");
            }

            return bestMatch;
        }

        [Header("Debug")]
        [Tooltip("Enable verbose logging for UI component discovery")]
        public bool verboseLogging = false;

        /// <summary>
        /// Manually assign UI components - useful for debugging or when auto-discovery fails
        /// </summary>
        public void ManuallyAssignUIComponents(TextMeshProUGUI moveTextComponent, TextMeshProUGUI timerTextComponent, GameObject levelCompleteCanvasComponent)
        {
            if (moveTextComponent != null)
            {
                moveText = moveTextComponent;
                Debug.Log($"[MoveCounter]: Manually assigned moveText to '{moveTextComponent.name}'");
            }

            if (timerTextComponent != null)
            {
                timerText = timerTextComponent;
                Debug.Log($"[MoveCounter]: Manually assigned timerText to '{timerTextComponent.name}'");
            }

            if (levelCompleteCanvasComponent != null)
            {
                levelCompleteCanvas = levelCompleteCanvasComponent;
                Debug.Log($"[MoveCounter]: Manually assigned levelCompleteCanvas to '{levelCompleteCanvasComponent.name}'");
            }

            RefreshDisplay();
        }

        /// <summary>
        /// Force re-discovery of UI components - useful when scene layout changes
        /// </summary>
        [ContextMenu("Re-discover UI Components")]
        public void RediscoverUIComponents()
        {
            moveText = null;
            timerText = null;
            levelCompleteCanvas = null;
            Debug.Log("[MoveCounter]: Cleared UI component assignments - re-discovering...");
            TryFindUIComponents();
        }

        /// <summary>
        /// Force refresh the display with current counter values
        /// </summary>
        public void RefreshDisplay()
        {
            if (moveText != null)
                moveText.text = "Moves: " + moveCount;

            if (timerText != null)
            {
                var minutes = Mathf.FloorToInt(timer / 60f);
                var seconds = Mathf.FloorToInt(timer % 60f);
                var milliseconds = Mathf.FloorToInt((timer * 100f) % 100f);
                timerText.text = $"Time: {minutes:00}:{seconds:00}.{milliseconds:00}";
            }
        }

        private void Update()
        {
            // Timer logic
            if (timerRunning)
            {
                timer += Time.deltaTime;

                if (timerText != null)
                {
                    var minutes = Mathf.FloorToInt(timer / 60f);
                    var seconds = Mathf.FloorToInt(timer % 60f);
                    var milliseconds = Mathf.FloorToInt((timer * 100f) % 100f); // Get hundredths

                    timerText.text = $"Time: {minutes:00}:{seconds:00}.{milliseconds:00}";
                }

                // Stop timer if levelCompleteCanvas is active
                if (levelCompleteCanvas != null && levelCompleteCanvas.activeSelf)
                {
                    timerRunning = false;
                }
            }
        }

        public void IncrementMove()
        {
            moveCount++;
        
            if (moveText != null)
                moveText.text = "Moves: " + moveCount;
            else
                Debug.LogWarning("[MoveCounter]: Move text UI component not assigned - cannot update display");
        }

        public void ResetCounter()
        {
            moveCount = 0;
            timer = 0f;
            timerRunning = true;
        
            Debug.Log("[MoveCounter]: Game counters reset for new attempt");
        
            if (moveText != null)
                moveText.text = "Moves: 0";
            else
                Debug.LogWarning("[MoveCounter]: Move text UI component not assigned - cannot update display");

            if (timerText != null)
                timerText.text = "Time: 00:00.00";
            else
                Debug.LogWarning("[MoveCounter]: Timer text UI component not assigned - cannot update display");
        }
        public float GetElapsedTime()
        {
            return timer;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                Debug.Log("[MoveCounter]: Primary instance destroyed - clearing static reference");
                Instance = null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Clear UI references when a new scene loads since they're no longer valid
            moveText = null;
            timerText = null;
            levelCompleteCanvas = null;
            
            // Reset counters for new level (only if it's a gameplay scene)
            if (SceneInfo.IsGameplayScene(scene))
            {
                ResetCounter();
            }
            
            // Try to find new UI components in the loaded scene
            // Use a coroutine to delay this until after the scene is fully loaded
            StartCoroutine(DelayedUISearch());
        }

        private System.Collections.IEnumerator DelayedUISearch()
        {
            // Wait a frame for the scene to be fully loaded
            yield return null;
            TryFindUIComponents();
        }
    }
}
