using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Pushing state: Player is pushing a crate or pushable object.
    /// Handles push animation and completion logic.
    /// Transitions back to Idle when push completes.
    /// </summary>
    public class PushingState : IPlayerState
    {
        private readonly PlayerController _controller;
        private readonly PlayerStateMachine _stateMachine;

        public PushingState(PlayerController controller, PlayerStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;
        }

        public void Enter()
        {
            // TODO: Start push animation
            // TODO: Apply push force to crate
            _controller.DisableDirectionChange();
        }

        public void Exit()
        {
            // Clean up push state
        }

        public void HandleInput()
        {
            // Input is blocked during push
        }

        public void Tick()
        {
            // TODO: Monitor push progress
            // For now, transition immediately back to Idle
            // In future implementation, wait for push animation/physics to complete
        }

        /// <summary>
        /// Called when push animation/physics completes.
        /// </summary>
        public void OnPushComplete()
        {
            _stateMachine.ChangeState(PlayerStateType.Idle);
        }
    }
}
