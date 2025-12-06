using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Core.Events;

namespace Core
{
    /// <summary>
    /// Event args for move count changes
    /// </summary>
    public class MoveCountChangedEventArgs : EventArgs
    {
        public int MoveCount { get; }
        public MoveCountChangedEventArgs(int moveCount) => MoveCount = moveCount;
    }

    /// <summary>
    /// Event args for timer changes
    /// </summary>
    public class TimerChangedEventArgs : EventArgs
    {
        public float ElapsedTime { get; }
        public TimerChangedEventArgs(float elapsedTime) => ElapsedTime = elapsedTime;
    }

    /// <summary>
    /// Manages move counting and timer for gameplay levels
    /// Access via ServiceLocator.Get&lt;MoveCounter&gt;()
    /// UI components should subscribe to OnMovesChanged and OnTimerChanged events
    /// </summary>
    public class MoveCounter : MonoBehaviour
    {
        /// <summary>
        /// Event raised when the move count changes
        /// </summary>
        public event EventHandler<MoveCountChangedEventArgs> OnMovesChanged;
        
        /// <summary>
        /// Event raised when the timer updates (approximately 10 times per second to reduce overhead)
        /// </summary>
        public event EventHandler<TimerChangedEventArgs> OnTimerChanged;

        /// <summary>
        /// Event raised when the level is completed (timer stops due to completion canvas)
        /// </summary>
        public event EventHandler OnLevelCompleted;

        [Header("Event Channels")]
        [Tooltip("ScriptableObject event channel raised when moves change (optional, complements C# events)")]
        [SerializeField] private IntGameEvent movesChangedEvent;
        
        [Tooltip("ScriptableObject event channel raised when timer changes (optional, complements C# events)")]
        [SerializeField] private FloatGameEvent timerChangedEvent;

        [Header("Move Counter")]
        public int moveCount;
        
        [Obsolete("Direct UI references are deprecated. Use OnMovesChanged event instead.")]
        public TextMeshProUGUI moveText;

        [Header("Timer")]
        [Obsolete("Direct UI references are deprecated. Use OnTimerChanged event instead.")]
        public TextMeshProUGUI timerText;
        public GameObject levelCompleteCanvas; // Canvas that ends the timer

        private float timer;
        private bool timerRunning = true;
        private bool isExternallyPaused = false;
        private float lastTimerUpdate = 0f;
        private const float TIMER_UPDATE_INTERVAL = 0.1f; // Update timer 10 times per second

        /// <summary>
        /// Initialize the MoveCounter - can be called by other systems
        /// </summary>
        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
            timer = 0f;
            timerRunning = true;
            // Subscribe to scene changes to notify subscribers of resets
            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log("[MoveCounter]: Instance initialized and persisted across scenes");
        }

        private void Update()
        {
            if (!timerRunning || isExternallyPaused)
            {
                return;
            }

            timer += Time.deltaTime;

            // Only update timer event periodically to reduce overhead
            if (timer - lastTimerUpdate >= TIMER_UPDATE_INTERVAL)
            {
                lastTimerUpdate = timer;
                OnTimerChanged?.Invoke(this, new TimerChangedEventArgs(timer));
                timerChangedEvent?.Raise(timer);
            }

            // Stop timer if levelCompleteCanvas is active
            if (levelCompleteCanvas != null && levelCompleteCanvas.activeSelf)
            {
                timerRunning = false;
                isExternallyPaused = false;
                // Final timer update
                OnTimerChanged?.Invoke(this, new TimerChangedEventArgs(timer));
                timerChangedEvent?.Raise(timer);
                // Notify level completion
                OnLevelCompleted?.Invoke(this, EventArgs.Empty);
                Debug.Log($"[MoveCounter]: Level completed - Moves: {moveCount}, Time: {timer:F2}s");
            }
        }

        public void IncrementMove()
        {
            moveCount++;
            OnMovesChanged?.Invoke(this, new MoveCountChangedEventArgs(moveCount));
            movesChangedEvent?.Raise(moveCount);
        }

        public void ResetCounter()
        {
            moveCount = 0;
            timer = 0f;
            lastTimerUpdate = 0f;
            timerRunning = true;
            isExternallyPaused = false;
        
            Debug.Log("[MoveCounter]: Game counters reset for new attempt");
        
            // Notify subscribers of the reset
            OnMovesChanged?.Invoke(this, new MoveCountChangedEventArgs(moveCount));
            OnTimerChanged?.Invoke(this, new TimerChangedEventArgs(timer));
            
            // Raise ScriptableObject events
            movesChangedEvent?.Raise(moveCount);
            timerChangedEvent?.Raise(timer);
        }

        public void PauseTimer()
        {
            if (!timerRunning) return;
            isExternallyPaused = true;
        }

        public void ResumeTimer()
        {
            if (!timerRunning) return;
            if (levelCompleteCanvas != null && levelCompleteCanvas.activeSelf) return;
            isExternallyPaused = false;
        }

        public float GetElapsedTime()
        {
            return timer;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Debug.Log("[MoveCounter]: Primary instance destroyed");
        }

        /// <summary>
        /// Coordinated shutdown for MoveCounter: unsubscribe and destroy the GameObject.
        /// </summary>
        public void Shutdown()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (this != null && gameObject != null)
            {
                Destroy(gameObject);
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Reset counters for new level (only if it's a gameplay scene)
            if (SceneInfo.IsGameplayScene(scene))
            {
                ResetCounter();
                Debug.Log($"[MoveCounter]: Counters reset for gameplay scene '{scene.name}'");
            }
        }

        private System.Collections.IEnumerator DelayedUISearch()
        {
            // DEPRECATED: This method is no longer used with event-driven UI
            // Kept for backward compatibility but does nothing
            yield return null;
        }
    }
}
