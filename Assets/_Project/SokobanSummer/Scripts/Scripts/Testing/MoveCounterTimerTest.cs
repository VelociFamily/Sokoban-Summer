using UnityEngine;
using Core;
using TMPro;

namespace Testing
{
    /// <summary>
    /// Test script to validate MoveCounter event-driven functionality
    /// Attach to any GameObject in a test scene and run tests via context menu or on Start
    /// </summary>
    public class MoveCounterTimerTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [Tooltip("Run tests automatically when the scene starts")]
        public bool runTestOnStart = true;
        
        [Tooltip("Include detailed logging for debugging")]
        public bool verboseLogging = true;

        private int testMoveEventCount = 0;
        private int testTimerEventCount = 0;

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
            Debug.Log("=== MoveCounter Event-Driven Test Started ===");

            // Test 1: Check if MoveCounter instance exists
            bool instanceExists = TestMoveCounterInstance();
            
            // Test 2: Test event subscription and firing
            bool eventsWork = TestEventFiring();
            
            // Test 3: Test move increment functionality
            bool moveIncrementWorks = TestMoveIncrement();
            
            // Test 4: Test timer functionality
            bool timerWorks = TestTimer();
            
            // Test 5: Test reset functionality
            bool resetWorks = TestReset();

            // Summary
            Debug.Log("=== Test Results ===");
            Debug.Log($"✓ MoveCounter Instance: {(instanceExists ? "PASS" : "FAIL")}");
            Debug.Log($"✓ Event Firing: {(eventsWork ? "PASS" : "FAIL")}");
            Debug.Log($"✓ Move Increment: {(moveIncrementWorks ? "PASS" : "FAIL")}");
            Debug.Log($"✓ Timer Functionality: {(timerWorks ? "PASS" : "FAIL")}");
            Debug.Log($"✓ Reset Functionality: {(resetWorks ? "PASS" : "FAIL")}");
            
            bool allTestsPassed = instanceExists && eventsWork && moveIncrementWorks && timerWorks && resetWorks;
            Debug.Log($"=== Overall Result: {(allTestsPassed ? "ALL TESTS PASSED ✓" : "SOME TESTS FAILED ✗")} ===");
        }

        private bool TestMoveCounterInstance()
        {
            var instance = ServiceLocator.Get<MoveCounter>();
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

        private bool TestEventFiring()
        {
            var instance = ServiceLocator.Get<MoveCounter>();
            if (instance == null) return false;

            // Reset test counters
            testMoveEventCount = 0;
            testTimerEventCount = 0;

            // Subscribe to events
            instance.OnMovesChanged += OnTestMoveChanged;
            instance.OnTimerChanged += OnTestTimerChanged;

            // Trigger move event
            var currentMoveCount = instance.moveCount;
            instance.IncrementMove();

            // Wait for timer event (it fires every 0.1s)
            var timeout = Time.time + 1f;
            while (testTimerEventCount == 0 && Time.time < timeout)
            {
                // Waiting...
            }

            // Unsubscribe
            instance.OnMovesChanged -= OnTestMoveChanged;
            instance.OnTimerChanged -= OnTestTimerChanged;

            bool moveEventFired = testMoveEventCount > 0;
            bool timerEventFired = testTimerEventCount > 0;

            if (verboseLogging)
            {
                Debug.Log($"Move events fired: {testMoveEventCount}");
                Debug.Log($"Timer events fired: {testTimerEventCount}");
            }

            if (!moveEventFired)
            {
                Debug.LogError("✗ OnMovesChanged event did not fire");
            }

            // Timer events should fire automatically
            return moveEventFired && timerEventFired;
        }

        private void OnTestMoveChanged(object sender, MoveCountChangedEventArgs e)
        {
            testMoveEventCount++;
            if (verboseLogging)
            {
                Debug.Log($"[Test Event] Move changed: {e.MoveCount}");
            }
        }

        private void OnTestTimerChanged(object sender, TimerChangedEventArgs e)
        {
            testTimerEventCount++;
            if (verboseLogging)
            {
                Debug.Log($"[Test Event] Timer changed: {e.ElapsedTime:F2}s");
            }
        }

        private bool TestMoveIncrement()
        {
            var instance = ServiceLocator.Get<MoveCounter>();
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
            var instance = ServiceLocator.Get<MoveCounter>();
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
            var instance = ServiceLocator.Get<MoveCounter>();
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
            var moveCounter = ServiceLocator.Get<MoveCounter>();
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
            var moveCounter = ServiceLocator.Get<MoveCounter>();
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

        [ContextMenu("Test Event Subscription")]
        public void TestEventSubscription()
        {
            var instance = ServiceLocator.Get<MoveCounter>();
            if (instance == null)
            {
                Debug.LogError("Cannot test events - MoveCounter not registered in ServiceLocator");
                return;
            }

            Debug.Log("=== Event Subscription Test ===");
            
            testMoveEventCount = 0;
            testTimerEventCount = 0;

            // Subscribe
            instance.OnMovesChanged += OnTestMoveChanged;
            instance.OnTimerChanged += OnTestTimerChanged;

            Debug.Log("Subscribed to events. Incrementing move...");
            instance.IncrementMove();

            // Wait a moment for timer events
            StartCoroutine(WaitAndReportEvents(instance));
        }

        private System.Collections.IEnumerator WaitAndReportEvents(MoveCounter instance)
        {
            yield return new WaitForSeconds(0.5f);
            
            Debug.Log($"Event counts - Moves: {testMoveEventCount}, Timer: {testTimerEventCount}");
            
            // Unsubscribe
            instance.OnMovesChanged -= OnTestMoveChanged;
            instance.OnTimerChanged -= OnTestTimerChanged;
            
            Debug.Log("=== Event Test Complete ===");
        }
    }
}
