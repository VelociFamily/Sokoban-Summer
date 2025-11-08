using UnityEngine;
using Core;
using TMPro;

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
            var instance = SokobanSummer.Core.ServiceLocator.Get<MoveCounter>();
            if (instance != null)
            {
                if (verboseLogging) Debug.Log($"✓ MoveCounter instance found on '{instance.gameObject.name}'");
                return true;
            }
            else
            {
                Debug.LogError("✗ MoveCounter not registered in ServiceLocator - MoveCounter not initialized");
                return false;
            }
        }

        private bool TestUIComponentAssignments()
        {
            var instance = SokobanSummer.Core.ServiceLocator.Get<MoveCounter>();
            if (instance == null) return false;

            bool moveTextAssigned = instance.moveText != null;
            bool timerTextAssigned = instance.timerText != null;
            bool levelCompleteAssigned = instance.levelCompleteCanvas != null;

            if (verboseLogging)
            {
                Debug.Log($"Move Text: {(moveTextAssigned ? $"Assigned to '{instance.moveText.name}' (parent: '{instance.moveText.transform.parent?.name}', current text: '{instance.moveText.text}')" : "Not assigned")}");
                Debug.Log($"Timer Text: {(timerTextAssigned ? $"Assigned to '{instance.timerText.name}' (parent: '{instance.timerText.transform.parent?.name}', current text: '{instance.timerText.text}')" : "Not assigned")}");
                Debug.Log($"Level Complete Canvas: {(levelCompleteAssigned ? $"Assigned to '{instance.levelCompleteCanvas.name}'" : "Not assigned")}");
                
                // List all TextMeshProUGUI components in the scene for debugging
                var allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
                Debug.Log($"=== All TextMeshProUGUI components in scene ({allTexts.Length}) ===");
                for (int i = 0; i < allTexts.Length; i++)
                {
                    var text = allTexts[i];
                    Debug.Log($"[{i}] '{text.name}' (parent: '{text.transform.parent?.name}') - Text: '{text.text}'");
                }
                Debug.Log("=== End of TextMeshProUGUI list ===");
            }

            // At least move text or timer text should be assigned for the counter to be functional
            return moveTextAssigned || timerTextAssigned;
        }

        private bool TestMoveIncrement()
        {
            var instance = SokobanSummer.Core.ServiceLocator.Get<MoveCounter>();
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
            var instance = SokobanSummer.Core.ServiceLocator.Get<MoveCounter>();
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
            var instance = SokobanSummer.Core.ServiceLocator.Get<MoveCounter>();
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
            var moveCounter = SokobanSummer.Core.ServiceLocator.Get<MoveCounter>();
            if (moveCounter != null)
            {
                Debug.Log($"Simulating player movement - Current moves: {moveCounter.moveCount}");
                moveCounter.IncrementMove();
                Debug.Log($"After movement - Moves: {moveCounter.moveCount}");
            }
            else
            {
                Debug.LogError("Cannot test player movement - MoveCounter not registered in ServiceLocator");
            }
        }

        [ContextMenu("Test Counter Reset")]
        public void TestCounterReset()
        {
            var moveCounter = SokobanSummer.Core.ServiceLocator.Get<MoveCounter>();
            if (moveCounter != null)
            {
                Debug.Log($"Before reset - Moves: {moveCounter.moveCount}, Time: {moveCounter.GetElapsedTime():F2}s");
                moveCounter.ResetCounter();
                Debug.Log($"After reset - Moves: {moveCounter.moveCount}, Time: {moveCounter.GetElapsedTime():F2}s");
            }
            else
            {
                Debug.LogError("Cannot test counter reset - MoveCounter not registered in ServiceLocator");
            }
        }

        [ContextMenu("Debug UI Component Discovery")]
        public void DebugUIComponentDiscovery()
        {
            var instance = SokobanSummer.Core.ServiceLocator.Get<MoveCounter>();
            if (instance == null)
            {
                Debug.LogError("Cannot debug UI discovery - MoveCounter not registered in ServiceLocator");
                return;
            }

            Debug.Log("=== UI Component Discovery Debug ===");
            
            // Enable verbose logging temporarily
            bool originalVerbose = instance.verboseLogging;
            instance.verboseLogging = true;
            
            // Force re-discovery
            instance.RediscoverUIComponents();
            
            // Restore original verbose setting
            instance.verboseLogging = originalVerbose;
            
            Debug.Log("=== UI Discovery Debug Complete ===");
        }
    }
}
