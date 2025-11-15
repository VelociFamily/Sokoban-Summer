using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Move state: Player is actively moving in a direction.
    /// Handles continuous movement and collision detection.
    /// Transitions back to Idle when collision occurs or movement completes.
    /// </summary>
    public class MoveState : IPlayerState
    {
        private readonly PlayerController _controller;
        private readonly PlayerStateMachine _stateMachine;

        public MoveState(PlayerController controller, PlayerStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;
        }

        public void Enter()
        {
            // Movement direction is already set by PlayerController before state change
            _controller.DisableDirectionChange();
        }

        public void Exit()
        {
            // Clean up movement state
        }

        public void HandleInput()
        {
            // Input is handled by PlayerController callbacks
            // New input during movement is blocked by canChangeDirection flag
        }

        public void Tick()
        {
            // Movement is handled in PlayerController.Update via moveDirection
            // This maintains current smooth movement behavior
        }

        /// <summary>
        /// Called by PlayerController when collision occurs.
        /// </summary>
        public void OnCollision()
        {
            _stateMachine.ChangeState(PlayerStateType.Idle);
        }
    }
}
