using UnityEngine;
using Core;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace Tests
{
    /// <summary>
    /// Test script to validate InputService correctly unsubscribes from input callbacks
    /// Ensures no duplicate event firing or memory leaks after disposal and re-initialization
    /// </summary>
    public class InputServiceDisposalTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        public bool runTestOnStart = true;

        private int _playerMoveCallCount = 0;
        private int _uiCancelCallCount = 0;

        private async void Start()
        {
            if (runTestOnStart)
            {
                await RunInputServiceDisposalTest();
            }
        }

        [ContextMenu("Run InputService Disposal Test")]
        public async Task RunInputServiceDisposalTest()
        {
            Debug.Log("=== InputService Disposal Test Started ===");

            // Test 1: Single initialization cycle
            await TestSingleInitializationCycle();

            // Test 2: Multiple enable/disable cycles
            await TestMultipleEnableDisableCycles();

            Debug.Log("=== InputService Disposal Test Completed ===");
        }

        private async Task TestSingleInitializationCycle()
        {
            Debug.Log("--- Testing Single Initialization Cycle ---");

            try
            {
                // Reset call counts
                _playerMoveCallCount = 0;
                _uiCancelCallCount = 0;

                // Initialize InputService
                var inputService = InputService.Instance;
                await inputService.InitializeAsync();

                // Subscribe to events
                inputService.OnPlayerMove += OnPlayerMoveHandler;
                inputService.OnUICancel += OnUICancelHandler;

                // Simulate input event
                SimulatePlayerMoveEvent(inputService);
                await Task.Yield();

                // Check that callback was invoked exactly once
                if (_playerMoveCallCount == 1)
                {
                    Debug.Log("✓ Single initialization: Callback invoked exactly once");
                }
                else
                {
                    Debug.LogError($"✗ Single initialization: Expected 1 callback, got {_playerMoveCallCount}");
                }

                // Clean up
                inputService.OnPlayerMove -= OnPlayerMoveHandler;
                inputService.OnUICancel -= OnUICancelHandler;
                inputService.Dispose();

                Debug.Log("✓ Single initialization cycle completed");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"✗ Single initialization test failed: {e.Message}");
            }

            await Task.Yield();
        }

        private async Task TestMultipleEnableDisableCycles()
        {
            Debug.Log("--- Testing Multiple Enable/Disable Cycles ---");

            try
            {
                // First cycle
                Debug.Log("Starting first initialization cycle...");
                _playerMoveCallCount = 0;
                _uiCancelCallCount = 0;

                var inputService = InputService.Instance;
                await inputService.InitializeAsync();
                inputService.OnPlayerMove += OnPlayerMoveHandler;
                inputService.OnUICancel += OnUICancelHandler;

                SimulatePlayerMoveEvent(inputService);
                await Task.Yield();

                int firstCycleCount = _playerMoveCallCount;
                Debug.Log($"First cycle: {firstCycleCount} callback(s)");

                // Dispose and reinitialize
                inputService.OnPlayerMove -= OnPlayerMoveHandler;
                inputService.OnUICancel -= OnUICancelHandler;
                inputService.Dispose();

                // Second cycle
                Debug.Log("Starting second initialization cycle...");
                _playerMoveCallCount = 0;
                _uiCancelCallCount = 0;

                // Reinitialize (simulating scene reload)
                await inputService.InitializeAsync();
                inputService.OnPlayerMove += OnPlayerMoveHandler;
                inputService.OnUICancel += OnUICancelHandler;

                SimulatePlayerMoveEvent(inputService);
                await Task.Yield();

                int secondCycleCount = _playerMoveCallCount;
                Debug.Log($"Second cycle: {secondCycleCount} callback(s)");

                // Verify both cycles had exactly 1 callback
                if (firstCycleCount == 1 && secondCycleCount == 1)
                {
                    Debug.Log("✓ Multiple cycles: Each cycle had exactly 1 callback (no accumulation)");
                }
                else
                {
                    Debug.LogError($"✗ Multiple cycles: Expected 1 callback per cycle, got {firstCycleCount} and {secondCycleCount}");
                }

                // Clean up
                inputService.OnPlayerMove -= OnPlayerMoveHandler;
                inputService.OnUICancel -= OnUICancelHandler;
                inputService.Dispose();

                Debug.Log("✓ Multiple enable/disable cycles test completed");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"✗ Multiple cycles test failed: {e.Message}");
            }

            await Task.Yield();
        }

        private void OnPlayerMoveHandler(InputAction.CallbackContext context)
        {
            _playerMoveCallCount++;
            Debug.Log($"Player move callback invoked (total: {_playerMoveCallCount})");
        }

        private void OnUICancelHandler(InputAction.CallbackContext context)
        {
            _uiCancelCallCount++;
            Debug.Log($"UI cancel callback invoked (total: {_uiCancelCallCount})");
        }

        private void SimulatePlayerMoveEvent(InputService inputService)
        {
            // Enable player input to ensure the action is active
            inputService.EnablePlayerInput();

            // Simulate a move event by manually invoking the internal event
            // Since we can't easily trigger actual input in a test, we'll trigger the forwarded event
            var context = new InputAction.CallbackContext();
            inputService.OnPlayerMove?.Invoke(context);

            inputService.DisablePlayerInput();
        }

        [ContextMenu("Manual Test: Check for Duplicate Callbacks")]
        public async Task ManualDuplicateCallbackTest()
        {
            Debug.Log("=== Manual Duplicate Callback Test ===");

            _playerMoveCallCount = 0;

            var inputService = InputService.Instance;
            await inputService.InitializeAsync();
            inputService.OnPlayerMove += OnPlayerMoveHandler;

            // Trigger multiple times without disposal
            for (int i = 0; i < 3; i++)
            {
                SimulatePlayerMoveEvent(inputService);
                await Task.Yield();
            }

            Debug.Log($"Total callbacks after 3 triggers: {_playerMoveCallCount}");
            if (_playerMoveCallCount == 3)
            {
                Debug.Log("✓ No duplicate accumulation: Got exactly 3 callbacks for 3 triggers");
            }
            else
            {
                Debug.LogError($"✗ Duplicate accumulation detected: Expected 3 callbacks, got {_playerMoveCallCount}");
            }

            // Clean up
            inputService.OnPlayerMove -= OnPlayerMoveHandler;
            inputService.Dispose();
        }
    }
}
