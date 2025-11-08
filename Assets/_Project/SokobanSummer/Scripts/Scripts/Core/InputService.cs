using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Centralized input service to manage input actions across the game
    /// Reduces duplication of InputSystem_Actions creation in multiple classes
    /// </summary>
    public class InputService
    {
        private static InputService _instance;
        public static InputService Instance => _instance ??= new InputService();

    public InputSystem_Actions InputActions { get; private set; }

        // Events for common input actions to reduce coupling
        public event Action<UnityEngine.InputSystem.InputAction.CallbackContext> OnPlayerMove;
        public event Action<UnityEngine.InputSystem.InputAction.CallbackContext> OnUICancel;

        // Cached delegates for proper unsubscription
        private Action<UnityEngine.InputSystem.InputAction.CallbackContext> _movePerformedHandler;
        private Action<UnityEngine.InputSystem.InputAction.CallbackContext> _moveCanceledHandler;
        private Action<UnityEngine.InputSystem.InputAction.CallbackContext> _uiCancelPerformedHandler;

        private InputService() { }

        public async Task InitializeAsync()
        {
            Debug.Log("[InputService]: Initializing input systems...");

            await InitializeInputActions();

            Debug.Log("[InputService]: Input systems initialized successfully");
        }

        private async Task InitializeInputActions()
        {
            if (InputActions == null)
            {
                InputActions = new InputSystem_Actions();

                // Initialize cached delegates
                _movePerformedHandler = ctx => OnPlayerMove?.Invoke(ctx);
                _moveCanceledHandler = ctx => OnPlayerMove?.Invoke(ctx);
                _uiCancelPerformedHandler = ctx => OnUICancel?.Invoke(ctx);

                // Set up common event forwarding to reduce coupling using cached delegates
                InputActions.Player.Move.performed += _movePerformedHandler;
                InputActions.Player.Move.canceled += _moveCanceledHandler;
                InputActions.UI.Cancel.performed += _uiCancelPerformedHandler;
            }

            await Task.Yield(); // Ensure async behavior
        }

        public void EnablePlayerInput()
        {
            InputActions?.Player.Enable();
        }

        public void DisablePlayerInput()
        {
            InputActions?.Player.Disable();
        }

        public void EnableUIInput()
        {
            InputActions?.UI.Enable();
        }

        public void DisableUIInput()
        {
            InputActions?.UI.Disable();
        }

        public void Dispose()
        {
            if (InputActions != null)
            {
                // Unsubscribe using the same cached delegate instances
                if (_movePerformedHandler != null)
                {
                    InputActions.Player.Move.performed -= _movePerformedHandler;
                }
                if (_moveCanceledHandler != null)
                {
                    InputActions.Player.Move.canceled -= _moveCanceledHandler;
                }
                if (_uiCancelPerformedHandler != null)
                {
                    InputActions.UI.Cancel.performed -= _uiCancelPerformedHandler;
                }

                InputActions.Dispose();
                InputActions = null;

                // Clear cached delegates
                _movePerformedHandler = null;
                _moveCanceledHandler = null;
                _uiCancelPerformedHandler = null;
            }
        }
    }
}
