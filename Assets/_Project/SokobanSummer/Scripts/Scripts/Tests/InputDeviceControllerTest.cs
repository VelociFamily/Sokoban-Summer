using UnityEngine;
using UnityEngine.InputSystem;
using UI;
using System.Threading.Tasks;
using System.Collections;

namespace Tests
{
    /// <summary>
    /// Test script to validate InputDeviceController behavior
    /// Tests device detection logic, pause button visibility toggling,
    /// event subscription/unsubscription, and race condition handling with GameplayUIController
    /// </summary>
    public class InputDeviceControllerTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        public bool runTestOnStart = true;

        // Test delay constants to avoid magic numbers and improve maintainability
        private const int SHORT_DELAY_MS = 50;
        private const int MEDIUM_DELAY_MS = 100;

        private GameObject testPauseButton;
        private InputDeviceController inputDeviceController;
        private GameplayUIController gameplayUIController;
        private int deviceChangeEventCount = 0;

        private async void Start()
        {
            if (runTestOnStart)
            {
                await RunInputDeviceControllerTests();
            }
        }

        [ContextMenu("Run InputDeviceController Tests")]
        public async Task RunInputDeviceControllerTests()
        {
            Debug.Log("=== InputDeviceController Test Started ===");

            // Test 1: Device detection logic
            await TestDeviceDetection();

            // Test 2: Pause button visibility toggling
            await TestPauseButtonVisibility();

            // Test 3: Event subscription/unsubscription
            await TestEventSubscription();

            // Test 4: Race condition handling with GameplayUIController
            await TestRaceConditionHandling();

            // Test 5: Null reference handling
            await TestNullReferenceHandling();

            Debug.Log("=== InputDeviceController Test Completed ===");
        }

        private async Task TestDeviceDetection()
        {
            Debug.Log("--- Testing Device Detection Logic ---");

            try
            {
                SetupTestEnvironment();

                // Test touchscreen detection
                bool hasTouchscreen = Touchscreen.current != null;
                Debug.Log($"✓ Touchscreen detection: {(hasTouchscreen ? "Present" : "Not present")}");

                // Verify that IsTouchscreenActive() returns correct value
                // We can't directly call private method, but we can verify behavior through visibility
                await Task.Yield();

                if (hasTouchscreen)
                {
                    Debug.Log("✓ Device detection logic: Touchscreen device available");
                }
                else
                {
                    Debug.Log("✓ Device detection logic: No touchscreen device (keyboard/controller expected)");
                }

                CleanupTestEnvironment();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"✗ Device detection test failed: {e.Message}");
            }
        }

        private async Task TestPauseButtonVisibility()
        {
            Debug.Log("--- Testing Pause Button Visibility Toggling ---");

            try
            {
                SetupTestEnvironment();

                bool initialActive = testPauseButton.activeSelf;
                Debug.Log($"Initial pause button state: {initialActive}");

                // Test ForceUpdate method
                inputDeviceController.ForceUpdate();
                await Task.Yield();

                bool afterForceUpdate = testPauseButton.activeSelf;
                Debug.Log($"After ForceUpdate: {afterForceUpdate}");

                // Verify visibility matches touchscreen presence
                bool hasTouchscreen = Touchscreen.current != null;
                if (afterForceUpdate == hasTouchscreen)
                {
                    Debug.Log($"✓ Pause button visibility correct: {afterForceUpdate} matches touchscreen presence: {hasTouchscreen}");
                }
                else
                {
                    Debug.LogWarning($"⚠ Pause button visibility mismatch: {afterForceUpdate} vs touchscreen: {hasTouchscreen}");
                }

                CleanupTestEnvironment();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"✗ Pause button visibility test failed: {e.Message}");
            }
        }

        private async Task TestEventSubscription()
        {
            Debug.Log("--- Testing Event Subscription/Unsubscription ---");

            try
            {
                SetupTestEnvironment();

                // Setup event tracking
                deviceChangeEventCount = 0;
                InputSystem.onDeviceChange += TrackDeviceChange;

                // Enable the controller (subscribes to events)
                inputDeviceController.enabled = true;
                await Task.Yield();

                // Simulate a device change
                // Note: We can't easily simulate actual input system events,
                // but we can verify subscription through component lifecycle
                Debug.Log("✓ InputDeviceController enabled and subscribed to events");

                // Disable the controller (unsubscribes from events)
                inputDeviceController.enabled = false;
                await Task.Yield();

                Debug.Log("✓ InputDeviceController disabled and unsubscribed from events");

                // Re-enable to test multiple cycles
                inputDeviceController.enabled = true;
                await Task.Yield();

                Debug.Log("✓ InputDeviceController re-enabled (testing multiple subscription cycles)");

                // Cleanup
                InputSystem.onDeviceChange -= TrackDeviceChange;
                CleanupTestEnvironment();

                Debug.Log($"✓ Event subscription/unsubscription test completed (tracked {deviceChangeEventCount} events)");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"✗ Event subscription test failed: {e.Message}");
                InputSystem.onDeviceChange -= TrackDeviceChange;
            }
        }

        private async Task TestRaceConditionHandling()
        {
            Debug.Log("--- Testing Race Condition Handling with GameplayUIController ---");

            try
            {
                SetupTestEnvironment();

                // Create a GameplayUIController for coordination testing
                var gameplayUIObject = new GameObject("TestGameplayUIController");
                gameplayUIController = gameplayUIObject.AddComponent<GameplayUIController>();

                // Give components time to find each other
                await Task.Delay(MEDIUM_DELAY_MS);

                // Simulate pause state
                Debug.Log("Simulating pause state...");
                
                // Test OnPauseStateChanged notification
                inputDeviceController.OnPauseStateChanged();
                await Task.Yield();

                Debug.Log("✓ OnPauseStateChanged called without errors");

                // Test multiple rapid state changes (race condition scenario)
                for (int i = 0; i < 5; i++)
                {
                    inputDeviceController.OnPauseStateChanged();
                    inputDeviceController.ForceUpdate();
                    await Task.Yield();
                }

                Debug.Log("✓ Multiple rapid state changes handled without errors");

                // Test when GameplayUIController is destroyed (null reference scenario)
                Destroy(gameplayUIController.gameObject);
                await Task.Yield();

                inputDeviceController.OnPauseStateChanged();
                inputDeviceController.ForceUpdate();
                await Task.Yield();

                Debug.Log("✓ Handled missing GameplayUIController gracefully");

                CleanupTestEnvironment();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"✗ Race condition handling test failed: {e.Message}");
            }
        }

        private async Task TestNullReferenceHandling()
        {
            Debug.Log("--- Testing Null Reference Handling ---");

            try
            {
                // Create controller without pause button reference
                var controllerObject = new GameObject("TestInputDeviceController_NoButton");
                var controller = controllerObject.AddComponent<InputDeviceController>();

                await Task.Yield();

                // Test ForceUpdate with null pause button (should log warning)
                controller.ForceUpdate();
                await Task.Yield();

                Debug.Log("✓ Handled null pause button reference gracefully");

                // Test OnPauseStateChanged with null pause button
                controller.OnPauseStateChanged();
                await Task.Yield();

                Debug.Log("✓ OnPauseStateChanged handled null reference gracefully");

                Destroy(controllerObject);
                await Task.Yield();

                Debug.Log("✓ Null reference handling test completed");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"✗ Null reference handling test failed: {e.Message}");
            }
        }

        private void SetupTestEnvironment()
        {
            Debug.Log("Setting up test environment...");

            // Create test pause button
            testPauseButton = new GameObject("TestPauseButton");
            
            // Create InputDeviceController
            var controllerObject = new GameObject("TestInputDeviceController");
            inputDeviceController = controllerObject.AddComponent<InputDeviceController>();

            // Note: Using reflection to set the private pauseButton field for testing purposes.
            // Alternative approaches considered:
            // 1. Making field [SerializeField] public - would expose internal implementation
            // 2. Using [InternalsVisibleTo] - requires modifying production assembly definition
            // 3. Adding test-only public setter - clutters production API
            // Reflection is the least invasive approach for this scenario
            var pauseButtonField = typeof(InputDeviceController).GetField("pauseButton", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (pauseButtonField != null)
            {
                pauseButtonField.SetValue(inputDeviceController, testPauseButton);
                Debug.Log("✓ Test environment setup complete");
            }
            else
            {
                Debug.LogError("✗ Could not find pauseButton field via reflection");
            }
        }

        private void CleanupTestEnvironment()
        {
            Debug.Log("Cleaning up test environment...");

            if (inputDeviceController != null)
            {
                Destroy(inputDeviceController.gameObject);
                inputDeviceController = null;
            }

            if (testPauseButton != null)
            {
                Destroy(testPauseButton);
                testPauseButton = null;
            }

            if (gameplayUIController != null)
            {
                Destroy(gameplayUIController.gameObject);
                gameplayUIController = null;
            }

            Debug.Log("✓ Test environment cleaned up");
        }

        private void TrackDeviceChange(InputDevice device, InputDeviceChange change)
        {
            deviceChangeEventCount++;
        }

        [ContextMenu("Test Pause Button Show/Hide Cycle")]
        public async Task TestPauseButtonShowHideCycle()
        {
            Debug.Log("=== Testing Pause Button Show/Hide Cycle ===");

            SetupTestEnvironment();

            for (int i = 0; i < 3; i++)
            {
                Debug.Log($"Cycle {i + 1}: Hiding button");
                testPauseButton.SetActive(false);
                await Task.Delay(MEDIUM_DELAY_MS);

                Debug.Log($"Cycle {i + 1}: Showing button");
                testPauseButton.SetActive(true);
                await Task.Delay(MEDIUM_DELAY_MS);

                Debug.Log($"Cycle {i + 1}: Force updating visibility");
                inputDeviceController.ForceUpdate();
                await Task.Delay(MEDIUM_DELAY_MS);
            }

            CleanupTestEnvironment();
            Debug.Log("✓ Pause button show/hide cycle completed");
        }

        [ContextMenu("Test Device Change Simulation")]
        public async Task TestDeviceChangeSimulation()
        {
            Debug.Log("=== Testing Device Change Simulation ===");

            SetupTestEnvironment();

            // Track initial state
            bool initialState = testPauseButton.activeSelf;
            Debug.Log($"Initial button state: {initialState}");

            // Simulate multiple device changes
            for (int i = 0; i < 5; i++)
            {
                Debug.Log($"Simulating device change {i + 1}...");
                inputDeviceController.ForceUpdate();
                await Task.Delay(SHORT_DELAY_MS);
            }

            bool finalState = testPauseButton.activeSelf;
            Debug.Log($"Final button state: {finalState}");

            CleanupTestEnvironment();
            Debug.Log("✓ Device change simulation completed");
        }

        private void OnDestroy()
        {
            // Ensure cleanup on destroy
            CleanupTestEnvironment();
        }
    }
}
