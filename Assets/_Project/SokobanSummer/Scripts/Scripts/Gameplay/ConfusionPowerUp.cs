using System;
using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Implementation of the confusion power-up that reverses player controls
    /// </summary>
    public class ConfusionPowerUp : IPowerUp
    {
        private int _confuseTurns;

        public int RemainingUses => _confuseTurns;
        public bool IsActive => _confuseTurns > 0;
        public string PowerUpName => "ConfusionPowerUp";
    
        /// <summary>
        /// Event raised when the power-up state changes
        /// </summary>
        public event EventHandler<PowerUpEventArgs> OnStateChanged;

        // Static accessor for backward compatibility - marked as obsolete
        [Obsolete("Use PowerUpManager events instead of accessing static fields directly")]
        public static int ConfuseTurns { get; set; }

        public void Activate(int uses)
        {
            _confuseTurns = uses;
            ConfuseTurns = uses; // Keep static field in sync for backward compatibility
            
            OnStateChanged?.Invoke(this, new PowerUpEventArgs
            {
                PowerUpName = PowerUpName,
                RemainingUses = _confuseTurns,
                IsActive = true,
                EventType = PowerUpEventType.Activated
            });
        }
    
        public bool ConsumeUse()
        {
            if (_confuseTurns > 0)
            {
                _confuseTurns--;
                ConfuseTurns = _confuseTurns; // Keep static field in sync
                
                var eventType = _confuseTurns > 0 ? PowerUpEventType.Consumed : PowerUpEventType.Deactivated;
                
                OnStateChanged?.Invoke(this, new PowerUpEventArgs
                {
                    PowerUpName = PowerUpName,
                    RemainingUses = _confuseTurns,
                    IsActive = _confuseTurns > 0,
                    EventType = eventType
                });
                
                return _confuseTurns > 0;
            }
            return false;
        }
    
        public void StartEffect(PlayerController playerController)
        {
            if (playerController.confuseEffect != null && !playerController.confuseEffect.isPlaying)
            {
                playerController.confuseEffect.Play();
            }
        }
    
        public void StopEffect(PlayerController playerController)
        {
            if (playerController.confuseEffect != null && playerController.confuseEffect.isPlaying)
            {
                playerController.confuseEffect.Stop();
            }
        }
    
        public Vector2 ApplyEffect(Vector2 inputDirection, PlayerController playerController)
        {
            // Reverse the input direction (confusion effect)
            if (inputDirection == Vector2.up) return Vector2.right;
            if (inputDirection == Vector2.right) return Vector2.up;
            if (inputDirection == Vector2.down) return Vector2.left;
            if (inputDirection == Vector2.left) return Vector2.down;
        
            return inputDirection;
        }
    }
}
