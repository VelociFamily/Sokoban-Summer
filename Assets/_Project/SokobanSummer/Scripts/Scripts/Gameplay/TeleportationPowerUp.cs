using Core;
using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Implementation of the teleport power-up that provides speed boost and special effects
    /// </summary>
    public class TeleportationPowerUp : IPowerUp
    {
        public int RemainingUses => TeleportTimes;
        public bool IsActive => TeleportTimes > 0;
        public string PowerUpName => "TeleportationPowerUp";
    
        // Static accessor for backward compatibility
        public static int TeleportTimes { get; set; }

        public void Activate(int uses)
        {
            TeleportTimes = uses;
        }
    
        public bool ConsumeUse()
        {
            if (TeleportTimes > 0)
            {
                TeleportTimes--;
                return TeleportTimes > 0;
            }
            return false;
        }
    
        public void StartEffect(PlayerController playerController)
        {
            if (playerController.teleportEffect != null && !playerController.teleportEffect.isPlaying)
            {
                playerController.teleportEffect.Play();
            }
        }
    
        public void StopEffect(PlayerController playerController)
        {
            if (playerController.teleportEffect != null && playerController.teleportEffect.isPlaying)
            {
                playerController.teleportEffect.Stop();
            }
        }
    
        public Vector2 ApplyEffect(Vector2 inputDirection, PlayerController playerController)
        {
            // Apply speed boost by setting the move speed
            playerController.moveSpeed = playerController.teleportSpeed;
        
            return inputDirection; // Direction is not modified by teleport
        }
    
        /// <summary>
        /// Plays the teleport sound effect
        /// </summary>
        /// <param name="playerController">The player controller to play sound on</param>
        public void PlayTeleportSound(PlayerController playerController)
        {
            if (playerController.teleportSound != null)
            {
                // Use centralized AudioService instead of player's individual audioSource
                ModernAudioService.Instance.PlaySFX(playerController.teleportSound);
            }
            else
            {
                Debug.LogWarning("[TeleportationPowerUp]: Teleport sound not assigned - cannot play audio feedback");
            }
        }
    }
}
