using UnityEngine;
using System.Threading.Tasks;
using System;

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
            
            // Set up common event forwarding to reduce coupling
            InputActions.Player.Move.performed += ctx => OnPlayerMove?.Invoke(ctx);
            InputActions.Player.Move.canceled += ctx => OnPlayerMove?.Invoke(ctx);
            InputActions.UI.Cancel.performed += ctx => OnUICancel?.Invoke(ctx);
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
            InputActions.Player.Move.performed -= ctx => OnPlayerMove?.Invoke(ctx);
            InputActions.Player.Move.canceled -= ctx => OnPlayerMove?.Invoke(ctx);
            InputActions.UI.Cancel.performed -= ctx => OnUICancel?.Invoke(ctx);
            
            InputActions.Dispose();
            InputActions = null;
        }
    }
}
