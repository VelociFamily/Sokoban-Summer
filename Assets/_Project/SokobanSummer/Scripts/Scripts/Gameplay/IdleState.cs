using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Idle state: Player is stationary and waiting for input.
    /// Transitions to Move state when movement input is received.
    /// </summary>
    public class IdleState : IPlayerState
    {
        private readonly PlayerController _controller;
        private readonly PlayerStateMachine _stateMachine;

        public IdleState(PlayerController controller, PlayerStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;
        }

        public void Enter()
        {
            // Stop any residual movement
            _controller.StopMovement();
            _controller.EnableDirectionChange();
        }

        public void Exit()
        {
            // Clean up if needed
        }

        public void HandleInput()
        {
            // Input handling is done via InputService callbacks in PlayerController
            // State transitions are triggered from those callbacks
        }

        public void Tick()
        {
            // Idle state has no per-frame logic
        }
    }
}
