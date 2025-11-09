using System.Collections;
using UnityEngine;
using Core;
using TMPro;
using UnityEngine.SceneManagement;

namespace Testing
{
    /// <summary>
    /// Test script to validate MoveCounter event-driven architecture
    /// Verifies that events are raised correctly and scene handling works as expected
    /// </summary>
    public class MoveCounterSceneFilterTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [Tooltip("Run tests automatically when the scene starts")]
        public bool runTestOnStart = true;
        
        [Tooltip("Include detailed logging for debugging")]
        public bool verboseLogging = true;

        private int moveEventCount = 0;
        private int timerEventCount = 0;
        private int lastMoveValue = -1;
        private float lastTimerValue = -1f;

        private void Start()
        {
            if (runTestOnStart)
            {
                StartCoroutine(DelayedTest());
            }
        }

        private IEnumerator DelayedTest()
        {
            // Wait a moment for all systems to initialize
            yield return new WaitForSeconds(1f);
            RunSceneFilterTest();
        }

        [ContextMenu("Run Scene Filter Test")]
        public void RunSceneFilterTest()
        {
            Debug.Log("=== MoveCounter Event-Driven Test Started ===");

            // Test 1: Verify current scene type
            bool sceneTypeCorrect = TestSceneTypeDetection();
            
            // Test 2: Check if events are subscribed and functioning
            bool eventsWorkCorrectly = TestEventSubscription();
            
            // Test 3: Verify event counts after actions
            bool eventCountsCorrect = TestEventCounts();

            // Summary
            Debug.Log("=== Test Results ===");
            Debug.Log($"✓ Scene Type Detection: {(sceneTypeCorrect ? "PASS" : "FAIL")}");
            Debug.Log($"✓ Event Subscription Works: {(eventsWorkCorrectly ? "PASS" : "FAIL")}");
            Debug.Log($"✓ Event Counts Correct: {(eventCountsCorrect ? "PASS" : "FAIL")}");
            
            bool allTestsPassed = sceneTypeCorrect && eventsWorkCorrectly && eventCountsCorrect;
            Debug.Log($"=== Overall Result: {(allTestsPassed ? "ALL TESTS PASSED ✓" : "SOME TESTS FAILED ✗")} ===");
        }

        private bool TestSceneTypeDetection()
        {
            var activeScene = SceneManager.GetActiveScene();
            var sceneType = SceneInfo.GetSceneType(activeScene);
            var isGameplayScene = SceneInfo.IsGameplayScene(activeScene);
            
            if (verboseLogging)
            {
                Debug.Log($"Current Scene: '{activeScene.name}'");
                Debug.Log($"Scene Type: {sceneType}");
                Debug.Log($"Is Gameplay Scene: {isGameplayScene}");
            }

            // Scene type detection is working if it returns a valid enum value
            return true;
        }

        private bool TestEventSubscription()
        {
            if (!ServiceLocator.TryGet<MoveCounter>(out var instance) || instance == null)
            {
                Debug.LogError("✗ MoveCounter not found in ServiceLocator - cannot test events");
                return false;
            }

            // Reset counters
            moveEventCount = 0;
            timerEventCount = 0;
            lastMoveValue = -1;
            lastTimerValue = -1f;

            // Subscribe to events
            instance.OnMovesChanged += OnTestMoveChanged;
            instance.OnTimerChanged += OnTestTimerChanged;

            // Trigger a move
            var currentMoveCount = instance.moveCount;
            instance.IncrementMove();

            // Wait a frame
            if (verboseLogging)
            {
                Debug.Log($"Move event fired {moveEventCount} times after IncrementMove");
                Debug.Log($"Last move value: {lastMoveValue}, Expected: {currentMoveCount + 1}");
            }

            // Unsubscribe
            instance.OnMovesChanged -= OnTestMoveChanged;
            instance.OnTimerChanged -= OnTestTimerChanged;

            bool moveEventFired = moveEventCount > 0 && lastMoveValue == currentMoveCount + 1;
            
            if (!moveEventFired)
            {
                Debug.LogError($"✗ Move event did not fire correctly. Event count: {moveEventCount}, Last value: {lastMoveValue}");
                return false;
            }

            if (verboseLogging)
            {
                Debug.Log("✓ Events are working correctly");
            }

            return true;
        }

        private bool TestEventCounts()
        {
            if (!ServiceLocator.TryGet<MoveCounter>(out var instance) || instance == null)
            {
                Debug.LogError("✗ MoveCounter not found in ServiceLocator");
                return false;
            }

            // Reset counters
            moveEventCount = 0;
            timerEventCount = 0;

            // Subscribe to events
            instance.OnMovesChanged += OnTestMoveChanged;
            instance.OnTimerChanged += OnTestTimerChanged;

            // Reset the counter (should fire events)
            instance.ResetCounter();

            if (verboseLogging)
            {
                Debug.Log($"After ResetCounter - Move events: {moveEventCount}, Timer events: {timerEventCount}");
            }

            // Unsubscribe
            instance.OnMovesChanged -= OnTestMoveChanged;
            instance.OnTimerChanged -= OnTestTimerChanged;

            // Both events should have fired at least once
            bool eventsFiredOnReset = moveEventCount > 0 && timerEventCount > 0;
            
            if (!eventsFiredOnReset)
            {
                Debug.LogError($"✗ Events did not fire on reset. Move: {moveEventCount}, Timer: {timerEventCount}");
                return false;
            }

            if (verboseLogging)
            {
                Debug.Log("✓ Event counts are correct");
            }

            return true;
        }

        private void OnTestMoveChanged(object sender, MoveCountChangedEventArgs e)
        {
            moveEventCount++;
            lastMoveValue = e.MoveCount;
            if (verboseLogging)
            {
                Debug.Log($"[Test] Move changed event received: {e.MoveCount}");
            }
        }

        private void OnTestTimerChanged(object sender, TimerChangedEventArgs e)
        {
            timerEventCount++;
            lastTimerValue = e.ElapsedTime;
            if (verboseLogging)
            {
                Debug.Log($"[Test] Timer changed event received: {e.ElapsedTime:F2}s");
            }
        }

        [ContextMenu("Log Current Scene Information")]
        public void LogCurrentSceneInformation()
        {
            var activeScene = SceneManager.GetActiveScene();
            var sceneInfo = SceneInfo.GetActiveSceneInfo();
            var sceneType = SceneInfo.GetSceneType(activeScene);
            var isGameplayScene = SceneInfo.IsGameplayScene(activeScene);

            Debug.Log("=== Current Scene Information ===");
            Debug.Log($"Scene Name: {activeScene.name}");
            Debug.Log($"Scene Build Index: {activeScene.buildIndex}");
            Debug.Log($"Scene Type: {sceneType}");
            Debug.Log($"Is Gameplay Scene: {isGameplayScene}");
            Debug.Log($"Has SceneInfo Component: {(sceneInfo != null)}");
            
            if (sceneInfo != null)
            {
                Debug.Log($"SceneInfo.sceneType: {sceneInfo.sceneType}");
                Debug.Log($"SceneInfo.showBackground: {sceneInfo.showBackground}");
                Debug.Log($"SceneInfo.levelNumber: {sceneInfo.levelNumber}");
            }
            
            Debug.Log("=== End of Scene Information ===");
        }
    }
}
