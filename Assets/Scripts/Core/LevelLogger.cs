using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    public class LevelLogger : MonoBehaviour
    {
        public static LevelLogger Instance;

        public class LevelResult
        {
            public int bestMoves = int.MaxValue;
            public float bestTime = float.MaxValue;
        }

        private Dictionary<int, LevelResult> bestResults = new Dictionary<int, LevelResult>();
        private bool hasLogged;

        private void Update()
        {
            if (SceneInfo.IsMainMenuScene())
                return;

            var moveCounter = MoveCounter.Instance;
            if (moveCounter == null || hasLogged)
                return;

            if (moveCounter.levelCompleteCanvas != null && moveCounter.levelCompleteCanvas.activeSelf)
            {
                var sceneIndex = SceneManager.GetActiveScene().buildIndex;
                var moves = moveCounter.moveCount;
                var time = moveCounter.GetElapsedTime();

                LogLevel(sceneIndex, moves, time);
                hasLogged = true;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            hasLogged = false;
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
    }
}