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
            if (moveText == null)
            {
                // Look for a Text component with "move" in its name or parent name
                var allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
                foreach (var text in allTexts)
                {
                    if (text.name.ToLower().Contains("move") || text.transform.parent?.name.ToLower().Contains("move") == true)
                    {
                        moveText = text;
                        Debug.Log($"[MoveCounter]: Auto-assigned moveText to '{text.name}' on '{text.transform.parent?.name}'");
                        break;
                    }
                }
            }

            if (timerText == null)
            {
                // Look for a Text component with "timer" or "time" in its name or parent name
                var allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
                foreach (var text in allTexts)
                {
                    if (text.name.ToLower().Contains("time") || text.transform.parent?.name.ToLower().Contains("time") == true)
                    {
                        timerText = text;
                        Debug.Log($"[MoveCounter]: Auto-assigned timerText to '{text.name}' on '{text.transform.parent?.name}'");
                        break;
                    }
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
