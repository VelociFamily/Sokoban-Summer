using System;
using Core;
using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Implementation of the teleport power-up that provides speed boost and special effects
    /// </summary>
    public class TeleportationPowerUp : IPowerUp
    {
        private int _teleportTimes;

        public int RemainingUses => _teleportTimes;
        public bool IsActive => _teleportTimes > 0;
        public string PowerUpName => "TeleportationPowerUp";
    
        /// <summary>
        /// Event raised when the power-up state changes
        /// </summary>
        public event EventHandler<PowerUpEventArgs> OnStateChanged;

        // Static accessor for backward compatibility - marked as obsolete
        [Obsolete("Use PowerUpManager events instead of accessing static fields directly")]
        public static int TeleportTimes { get; set; }

        public void Activate(int uses)
        {
            _teleportTimes = uses;
            TeleportTimes = uses; // Keep static field in sync for backward compatibility
            
            OnStateChanged?.Invoke(this, new PowerUpEventArgs
            {
                PowerUpName = PowerUpName,
                RemainingUses = _teleportTimes,
                IsActive = true,
                EventType = PowerUpEventType.Activated
            });
        }
    
        public bool ConsumeUse()
        {
            if (_teleportTimes > 0)
            {
                _teleportTimes--;
                TeleportTimes = _teleportTimes; // Keep static field in sync
                
                var eventType = _teleportTimes > 0 ? PowerUpEventType.Consumed : PowerUpEventType.Deactivated;
                
                OnStateChanged?.Invoke(this, new PowerUpEventArgs
                {
                    PowerUpName = PowerUpName,
                    RemainingUses = _teleportTimes,
                    IsActive = _teleportTimes > 0,
                    EventType = eventType
                });
                
                return _teleportTimes > 0;
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
                var audioService = ServiceLocator.Get<ModernAudioService>();
                audioService.PlaySFX(playerController.teleportSound);
            }
            else
            {
                Debug.LogWarning("[TeleportationPowerUp]: Teleport sound not assigned - cannot play audio feedback");
            }
        }
    }
}
