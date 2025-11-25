using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    /// <summary>
    /// Logs level completion and best scores.
    /// Access via ServiceLocator.Get<LevelLogger>()
    /// </summary>
    public class LevelLogger : MonoBehaviour
    {
        public class LevelResult
        {
            public int bestMoves = int.MaxValue;
            public float bestTime = float.MaxValue;
        }

        private Dictionary<int, LevelResult> bestResults = new Dictionary<int, LevelResult>();
        private bool hasLogged;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log("[LevelLogger]: Initialized and persisted across scenes");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            hasLogged = false;
            
            // Subscribe to MoveCounter's completion event when it becomes available
            if (ServiceLocator.TryGet<MoveCounter>(out var moveCounter) && moveCounter != null)
            {
                // Unsubscribe first to prevent duplicate subscriptions
                moveCounter.OnLevelCompleted -= OnLevelCompleted;
                moveCounter.OnLevelCompleted += OnLevelCompleted;
                Debug.Log("[LevelLogger]: Subscribed to MoveCounter.OnLevelCompleted");
            }
        }

        private void OnLevelCompleted(object sender, System.EventArgs e)
        {
            if (hasLogged)
            {
                Debug.Log("[LevelLogger]: Level already logged, skipping duplicate");
                return;
            }

            if (!ServiceLocator.TryGet<MoveCounter>(out var moveCounter) || moveCounter == null)
            {
                Debug.LogWarning("[LevelLogger]: MoveCounter not available when level completed");
                return;
            }

            var sceneIndex = SceneManager.GetActiveScene().buildIndex;
            var moves = moveCounter.moveCount;
            var time = moveCounter.GetElapsedTime();

            LogLevel(sceneIndex, moves, time);
            hasLogged = true;
        }

        private void LogLevel(int sceneIndex, int moves, float time)
        {
            if (!bestResults.ContainsKey(sceneIndex))
                bestResults[sceneIndex] = new LevelResult();

            var result = bestResults[sceneIndex];
            var newBest = false;

            if (moves < result.bestMoves)
            {
                result.bestMoves = moves;
                newBest = true;
            }

            if (time < result.bestTime)
            {
                result.bestTime = time;
                newBest = true;
            }

            Debug.Log($"[LevelLogger]: Level '{GetLevelName(sceneIndex)}' completed - Moves: {moves}, Time: {FormatTime(time)}{(newBest ? " (New Best!)" : "")}");

            // Notify LevelManager about completion to unlock next level
            if (ServiceLocator.TryGet<LevelManager>(out var levelManager) && levelManager != null)
            {
                levelManager.MarkLevelCompleted(sceneIndex);
            }

            if (bestResults.Count > 1)
            {
                Debug.Log("[LevelLogger]: Updated level progress summary:");
                foreach (var kvp in bestResults)
                {
                    Debug.Log($"  • {GetLevelName(kvp.Key)}: Best Moves: {kvp.Value.bestMoves}, Best Time: {FormatTime(kvp.Value.bestTime)}");
                }
            }
        }

        // 🔓 Public accessors for UI scripts (like BestScoresDisplay)
        public Dictionary<int, LevelResult> GetAllResults()
        {
            return bestResults;
        }

        public string FormatTime(float time)
        {
            var minutes = Mathf.FloorToInt(time / 60f);
            var seconds = Mathf.FloorToInt(time % 60f);
            var milliseconds = Mathf.FloorToInt((time * 100f) % 100f);
            return $"{minutes:00}:{seconds:00}.{milliseconds:00}";
        }

        public string GetLevelName(int index)
        {
            // Try to get level name from LevelManager first
            if (ServiceLocator.TryGet<LevelManager>(out var levelManager) && levelManager != null)
            {
                var levelInfo = levelManager.GetLevelByBuildIndex(index);
                if (levelInfo != null)
                {
                    return levelInfo.displayName;
                }
            }

            // Fallback to hardcoded names for backward compatibility
            return index switch
            {
                1 => "Moving Tutorial",
                2 => "Button Tutorial",
                3 => "Speed Tutorial",
                4 => "Confuse Tutorial",
                5 => "Level One",
                6 => "Level Two",
                _ => $"Scene {index}"
            };
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            
            // Unsubscribe from MoveCounter if it exists
            if (ServiceLocator.TryGet<MoveCounter>(out var moveCounter) && moveCounter != null)
            {
                moveCounter.OnLevelCompleted -= OnLevelCompleted;
            }
            
            Debug.Log("[LevelLogger]: Instance destroyed - unsubscribing from scene events");
        }
    }
}
