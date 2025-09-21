using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Implementation of the confusion power-up that reverses player controls
    /// </summary>
    public class ConfusionPowerUp : IPowerUp
    {
        public int RemainingUses => ConfuseTurns;
        public bool IsActive => ConfuseTurns > 0;
        public string PowerUpName => "ConfusionPowerUp";
    
        // Static accessor for backward compatibility
        public static int ConfuseTurns { get; set; }

        public void Activate(int uses)
        {
            ConfuseTurns = uses;
        }
    
        public bool ConsumeUse()
        {
            if (ConfuseTurns > 0)
            {
                ConfuseTurns--;
                return ConfuseTurns > 0;
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
