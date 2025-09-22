using UnityEngine;
using Core;

namespace Tests
{
    /// <summary>
    /// Simple test script to validate the dynamic level system
    /// Attach to a GameObject in a test scene to verify functionality
    /// </summary>
    public class LevelSystemTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        public bool runTestOnStart = true;
        public bool debugOutput = true;

        private void Start()
        {
            if (runTestOnStart)
            {
                RunLevelSystemTest();
            }
        }

        [ContextMenu("Run Level System Test")]
        public void RunLevelSystemTest()
        {
            Debug.Log("=== Level System Test Started ===");

            // Test 1: Check if LevelManager exists
            if (LevelManager.Instance == null)
            {
                Debug.LogError("TEST FAILED: LevelManager.Instance is null");
                return;
            }
            Debug.Log("✓ LevelManager instance found");

            // Test 2: Check level discovery
            var allLevels = LevelManager.Instance.GetAllLevels();
            Debug.Log($"✓ Found {allLevels.Count} total levels");

            var tutorials = LevelManager.Instance.GetLevels(SceneType.TutorialLevel);
            Debug.Log($"✓ Found {tutorials.Count} tutorial levels");

            var gameplayLevels = LevelManager.Instance.GetLevels(SceneType.GameplayLevel);
            Debug.Log($"✓ Found {gameplayLevels.Count} gameplay levels");

            // Test 3: List all discovered levels
            if (debugOutput)
            {
                Debug.Log("--- Discovered Levels ---");
                foreach (var level in allLevels)
                {
                    string canLoad = LevelManager.Instance.CanLoadLevel(level) ? "UNLOCKED" : "LOCKED";
                    Debug.Log($"  {level.displayName} [{level.sceneType}] - Build Index: {level.buildIndex} - {canLoad}");
                }
            }

            // Test 4: Test level data if available
            foreach (var level in allLevels)
            {
                if (level.parMoves > 0 || level.parTime > 0f)
                {
                    Debug.Log($"  {level.displayName} has goals: {level.parMoves} moves, {level.parTime}s");
                }
            }

            Debug.Log("=== Level System Test Completed ===");
        }

        /// <summary>
        /// Test level progression simulation
        /// </summary>
        [ContextMenu("Test Level Progression")]
        public void TestLevelProgression()
        {
            Debug.Log("=== Testing Level Progression ===");

            var tutorials = LevelManager.Instance.GetLevels(SceneType.TutorialLevel);
            if (tutorials.Count > 0)
            {
                var firstTutorial = tutorials[0];
                Debug.Log($"First tutorial: {firstTutorial.displayName} - Can load: {LevelManager.Instance.CanLoadLevel(firstTutorial)}");

                var nextLevel = LevelManager.Instance.GetNextLevel(firstTutorial);
                if (nextLevel != null)
                {
                    Debug.Log($"Next level after first tutorial: {nextLevel.displayName}");
                }
            }

            var gameplayLevels = LevelManager.Instance.GetLevels(SceneType.GameplayLevel);
            if (gameplayLevels.Count > 0)
            {
                var firstLevel = gameplayLevels[0];
                Debug.Log($"First gameplay level: {firstLevel.displayName} - Can load: {LevelManager.Instance.CanLoadLevel(firstLevel)}");
            }

            Debug.Log("=== Level Progression Test Completed ===");
        }
    }
}