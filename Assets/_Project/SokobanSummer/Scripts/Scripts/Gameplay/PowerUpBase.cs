using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Base class for all power-ups, implementing common collection and animation logic
    /// </summary>
    public abstract class PowerUpBase : MonoBehaviour
    {
        [Header("Power-Up Configuration")]
        [Tooltip("Number of uses/turns this power-up grants when collected")]
        public int usesGranted = 1;
    
        [Tooltip("Optional GameObject to disable when collected (e.g., visual item)")]
        public GameObject itemToDisable;
    
        /// <summary>
        /// Gets the power-up implementation for this specific type
        /// </summary>
        protected abstract IPowerUp PowerUpImplementation { get; }
    
        /// <summary>
        /// Called when player collects this power-up
        /// </summary>
        /// <param name="other">The collider that triggered collection (should be player)</param>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                CollectPowerUp(other);
            }
        }
    
        /// <summary>
        /// Handles the power-up collection process
        /// </summary>
        /// <param name="playerCollider">The player's collider</param>
        protected virtual void CollectPowerUp(Collider2D playerCollider)
        {
            var powerUp = PowerUpImplementation;
        
            // Activate the power-up
            powerUp.Activate(usesGranted);
        
            // Disable the visual item if specified
            if (itemToDisable != null)
            {
                itemToDisable.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        
            Debug.Log($"[{powerUp.PowerUpName}]: Power-up collected - {usesGranted} uses granted");
        
            // Start the animation immediately
            var playerController = playerCollider.GetComponent<PlayerController>();
            if (playerController != null)
            {
                powerUp.StartEffect(playerController);
                Debug.Log($"[{powerUp.PowerUpName}]: Animation started immediately");
            }
        }
    }
}
