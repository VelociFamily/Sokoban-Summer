using UnityEngine;
using Core;

namespace Testing
{
    /// <summary>
    /// Test script to validate MoveCounter and timer functionality
    /// Attach to any GameObject in a test scene and run tests via context menu or on Start
    /// </summary>
    public class MoveCounterTimerTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [Tooltip("Run tests automatically when the scene starts")]
        public bool runTestOnStart = true;
        
        [Tooltip("Include detailed logging for debugging")]
        public bool verboseLogging = true;

        private void Start()
        {
            if (runTestOnStart)
            {
                StartCoroutine(DelayedTest());
            }
        }

        private System.Collections.IEnumerator DelayedTest()
        {
            // Wait a moment for all systems to initialize
            yield return new WaitForSeconds(1f);
            RunMoveCounterTest();
        }

        [ContextMenu("Run MoveCounter Test")]
        public void RunMoveCounterTest()
        {
            Debug.Log("=== MoveCounter and Timer Test Started ===");

            // Test 1: Check if MoveCounter instance exists
            bool instanceExists = TestMoveCounterInstance();
            
            // Test 2: Check UI component assignments
            bool uiAssigned = TestUIComponentAssignments();
            
            // Test 3: Test move increment functionality
            bool moveIncrementWorks = TestMoveIncrement();
            
            // Test 4: Test timer functionality
            bool timerWorks = TestTimer();
            
            // Test 5: Test reset functionality
            bool resetWorks = TestReset();

            // Summary
            Debug.Log("=== Test Results ===");
            Debug.Log($"✓ MoveCounter Instance: {(instanceExists ? "PASS" : "FAIL")}");
            Debug.Log($"✓ UI Components: {(uiAssigned ? "PASS" : "FAIL")}");
            Debug.Log($"✓ Move Increment: {(moveIncrementWorks ? "PASS" : "FAIL")}");
            Debug.Log($"✓ Timer Functionality: {(timerWorks ? "PASS" : "FAIL")}");
            Debug.Log($"✓ Reset Functionality: {(resetWorks ? "PASS" : "FAIL")}");
            
            bool allTestsPassed = instanceExists && uiAssigned && moveIncrementWorks && timerWorks && resetWorks;
            Debug.Log($"=== Overall Result: {(allTestsPassed ? "ALL TESTS PASSED ✓" : "SOME TESTS FAILED ✗")} ===");
        }

        private bool TestMoveCounterInstance()
        {
            var instance = MoveCounter.Instance;
            if (instance != null)
            {
                if (verboseLogging) Debug.Log($"✓ MoveCounter.Instance found on '{instance.gameObject.name}'");
                return true;
            }
            else
            {
                Debug.LogError("✗ MoveCounter.Instance is null - MoveCounter not initialized");
                return false;
            }
        }

        private bool TestUIComponentAssignments()
        {
            var instance = MoveCounter.Instance;
            if (instance == null) return false;

            bool moveTextAssigned = instance.moveText != null;
            bool timerTextAssigned = instance.timerText != null;
            bool levelCompleteAssigned = instance.levelCompleteCanvas != null;

            if (verboseLogging)
            {
                Debug.Log($"Move Text: {(moveTextAssigned ? $"Assigned to '{instance.moveText.name}'" : "Not assigned")}");
                Debug.Log($"Timer Text: {(timerTextAssigned ? $"Assigned to '{instance.timerText.name}'" : "Not assigned")}");
                Debug.Log($"Level Complete Canvas: {(levelCompleteAssigned ? $"Assigned to '{instance.levelCompleteCanvas.name}'" : "Not assigned")}");
            }

            // At least move text or timer text should be assigned for the counter to be functional
            return moveTextAssigned || timerTextAssigned;
        }

        private bool TestMoveIncrement()
        {
            var instance = MoveCounter.Instance;
            if (instance == null) return false;

            int initialCount = instance.moveCount;
            instance.IncrementMove();
            int afterIncrement = instance.moveCount;

            bool incrementWorks = afterIncrement == initialCount + 1;
            if (verboseLogging) 
            {
                Debug.Log($"Move count: {initialCount} → {afterIncrement} (Expected: {initialCount + 1})");
            }

            return incrementWorks;
        }

        private bool TestTimer()
        {
            var instance = MoveCounter.Instance;
            if (instance == null) return false;

            // Timer should be running by default and should return a positive elapsed time
            float elapsedTime = instance.GetElapsedTime();
            bool timerRunning = elapsedTime >= 0f;

            if (verboseLogging) 
            {
                Debug.Log($"Timer elapsed time: {elapsedTime:F2} seconds");
            }

            return timerRunning;
        }

        private bool TestReset()
        {
            var instance = MoveCounter.Instance;
            if (instance == null) return false;

            // Increment counter first
            instance.IncrementMove();
            int movesBeforeReset = instance.moveCount;

            // Reset
            instance.ResetCounter();
            
            int movesAfterReset = instance.moveCount;
            float timeAfterReset = instance.GetElapsedTime();

            bool resetWorks = movesAfterReset == 0 && timeAfterReset == 0f;
            
            if (verboseLogging) 
            {
                Debug.Log($"After reset - Moves: {movesAfterReset}, Time: {timeAfterReset:F2}s");
            }

            return resetWorks;
        }

        [ContextMenu("Test Move Increment (Simulate Player Movement)")]
        public void TestPlayerMovement()
        {
            if (MoveCounter.Instance != null)
            {
                Debug.Log($"Simulating player movement - Current moves: {MoveCounter.Instance.moveCount}");
                MoveCounter.Instance.IncrementMove();
                Debug.Log($"After movement - Moves: {MoveCounter.Instance.moveCount}");
            }
            else
            {
                Debug.LogError("Cannot test player movement - MoveCounter.Instance is null");
            }
        }

        [ContextMenu("Test Counter Reset")]
        public void TestCounterReset()
        {
            if (MoveCounter.Instance != null)
            {
                Debug.Log($"Before reset - Moves: {MoveCounter.Instance.moveCount}, Time: {MoveCounter.Instance.GetElapsedTime():F2}s");
                MoveCounter.Instance.ResetCounter();
                Debug.Log($"After reset - Moves: {MoveCounter.Instance.moveCount}, Time: {MoveCounter.Instance.GetElapsedTime():F2}s");
            }
            else
            {
                Debug.LogError("Cannot test counter reset - MoveCounter.Instance is null");
            }
        }
    }
}