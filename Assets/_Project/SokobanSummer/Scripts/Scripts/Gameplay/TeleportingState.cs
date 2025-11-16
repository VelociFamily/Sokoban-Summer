using System.Collections;
using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Teleporting state: Player is teleporting between portals.
    /// Suspends input during teleport effect and animation.
    /// Transitions back to Idle after teleport completes.
    /// </summary>
    public class TeleportingState : IPlayerState
    {
        private readonly PlayerController _controller;
        private readonly PlayerStateMachine _stateMachine;
        private Coroutine _teleportCoroutine;

        public TeleportingState(PlayerController controller, PlayerStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;
        }

        public void Enter()
        {
            _controller.DisableDirectionChange();
            _controller.StopMovement();
            
            // Play teleport effect
            if (_controller.teleportEffect != null)
            {
                _controller.teleportEffect.Play();
            }

            // Play teleport sound via centralized audio
            if (_controller.teleportSound != null)
            {
                var audioService = Core.ServiceLocator.Get<Core.ModernAudioService>();
                audioService?.PlaySFX(_controller.teleportSound);
            }
        }

        public void Exit()
        {
            // Clean up teleport state
            if (_teleportCoroutine != null)
            {
                _controller.StopCoroutine(_teleportCoroutine);
                _teleportCoroutine = null;
            }
        }

        public void HandleInput()
        {
            // Input is blocked during teleport
        }

        public void Tick()
        {
            // Teleport logic is handled by coroutine or external trigger
        }

        /// <summary>
        /// Called when teleport animation/effect completes.
        /// </summary>
        public void OnTeleportComplete()
        {
            _stateMachine.ChangeState(PlayerStateType.Idle);
        }

        /// <summary>
        /// Start teleport sequence with specified duration.
        /// </summary>
        public void StartTeleport(float duration)
        {
            if (_teleportCoroutine != null)
            {
                _controller.StopCoroutine(_teleportCoroutine);
            }
            _teleportCoroutine = _controller.StartCoroutine(TeleportSequence(duration));
        }

        private IEnumerator TeleportSequence(float duration)
        {
            yield return new UnityEngine.WaitForSeconds(duration);
            OnTeleportComplete();
        }
    }
}
