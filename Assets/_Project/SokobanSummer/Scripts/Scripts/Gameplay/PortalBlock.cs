using UnityEngine;
using Core;
using System.Collections;

namespace Gameplay
{
    /// <summary>
    /// Represents a portal block that teleports the player to a linked portal.
    /// When the player moves over the portal, they are instantly teleported to the exit portal
    /// with their momentum preserved in the original direction.
    /// </summary>
    public class PortalBlock : MonoBehaviour
    {
        [SerializeField] private PortalBlock linkedPortal;
        [SerializeField] private float teleportDuration = 0.3f;
        [SerializeField] private float teleportCooldown = 3f;

        private Collider2D _collider;
        private static float _lastTeleportTime = -10f;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            if (_collider == null)
            {
                Debug.LogError($"[PortalBlock] {gameObject.name} requires a Collider2D component (set as IsTrigger = true)");
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Only process teleportation if the other collider is the player
            if (!other.CompareTag("Player"))
                return;

            // Check global cooldown to prevent instant re-teleportation
            if (Time.time - _lastTeleportTime < teleportCooldown)
                return;

            // Don't teleport if no linked portal is assigned
            if (linkedPortal == null)
                return;

            var playerController = other.GetComponent<PlayerController>();
            if (playerController == null)
                return;

            // Get the player's current movement direction (momentum)
            Vector2 playerMomentum = playerController.GetMoveDirection();
            
            // If player isn't moving but was trying to move into a wall, use last attempted direction
            if (playerMomentum.magnitude < 0.1f)
            {
                playerMomentum = playerController.GetLastAttemptedDirection();
            }

            // Only trigger teleport if player has attempted movement
            // This prevents accidental triggers from just standing on portal
            if (playerMomentum.magnitude < 0.1f)
                return;

            // Perform the teleport immediately on contact
            StartCoroutine(TeleportPlayer(playerController, playerMomentum));
        }

        /// <summary>
        /// Teleports the player to the linked portal while preserving their momentum direction.
        /// </summary>
        private IEnumerator TeleportPlayer(PlayerController playerController, Vector2 momentum)
        {
            // Mark teleport time globally to prevent re-triggering
            _lastTeleportTime = Time.time;
            
            // Get the player's state machine
            var stateMachine = playerController.GetStateMachine();
            if (stateMachine == null)
            {
                yield break;
            }
            
            // Transition to Teleporting state
            stateMachine.ChangeState(PlayerStateType.Teleporting);
            
            // Get the TeleportingState to access teleport sequence
            var teleportingState = stateMachine.GetState(PlayerStateType.Teleporting) as TeleportingState;
            
            // Start the teleport sequence (plays effects/sounds)
            if (teleportingState != null)
            {
                teleportingState.StartTeleport(teleportDuration);
            }
            
            // Wait for teleport animation to complete
            yield return new WaitForSeconds(teleportDuration);
            
            // Verify linked portal still exists
            if (linkedPortal == null)
            {
                Debug.LogError($"[PortalBlock] Linked portal was destroyed during teleport!");
                yield break;
            }
            
            // Move player to the exit portal's position
            // Snap to grid to ensure proper positioning
            Vector3 exitPos = linkedPortal.transform.position;
            exitPos = new Vector3(
                Mathf.Round(exitPos.x * 2f) / 2f,
                Mathf.Round(exitPos.y * 2f) / 2f,
                exitPos.z
            );
            playerController.transform.position = exitPos;
            
            // Preserve the player's momentum direction
            // The player will continue moving in the same direction they were heading
            playerController.SetMoveDirection(momentum);
            
            // Return to Idle state so player can continue with their momentum
            stateMachine.ChangeState(PlayerStateType.Idle);
            
            // Re-enable direction changes
            playerController.EnableDirectionChange();
        }

        /// <summary>
        /// Debug visualization to show portal connections in the editor.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (linkedPortal == null)
                return;

            // Draw a line from this portal to the linked portal
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, linkedPortal.transform.position);
            
            // Draw a sphere at the linked portal
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(linkedPortal.transform.position, 0.3f);
        }
    }
}
