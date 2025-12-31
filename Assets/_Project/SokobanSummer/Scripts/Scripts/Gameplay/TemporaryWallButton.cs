using UnityEngine;
using System.Collections.Generic;

namespace Gameplay
{
    /// <summary>
    /// A button sprite that the player steps on to activate temporary walls.
    /// When the player moves onto this sprite, all registered walls disappear for a specified number of moves.
    /// </summary>
    public class TemporaryWallButton : MonoBehaviour
    {
        [Header("Button Settings")]
        [Tooltip("Number of player moves before the walls reappear")]
        [SerializeField] private int movesDuration = 3;
        
        [Tooltip("Can the button be pressed multiple times?")]
        [SerializeField] private bool canBeReused = true;
        
        [Header("Walls")]
        [Tooltip("The walls controlled by this button. Add walls manually or they will auto-register.")]
        [SerializeField] private List<TemporaryWall> controlledWalls = new List<TemporaryWall>();
        
        [Header("Visual Feedback")]
        [Tooltip("The sprite when button is unpressed")]
        [SerializeField] private Sprite unpressedSprite;
        
        [Tooltip("The sprite when button is pressed")]
        [SerializeField] private Sprite pressedSprite;
        
        [SerializeField] private SpriteRenderer buttonRenderer;
        
        [Tooltip("Optional: Particle effect when button is pressed")]
        [SerializeField] private ParticleSystem pressEffect;
        
        [Tooltip("Optional: Audio clip to play when button is pressed")]
        [SerializeField] private AudioClip pressSound;
        
        [Header("Cooldown Settings")]
        [Tooltip("If reusable, how many moves must pass before the button can be pressed again?")]
        [SerializeField] private int cooldownMoves = 0;
        
        private bool isPressed = false;
        private bool isOnCooldown = false;
        private int cooldownMovesRemaining = 0;
        
        private void Awake()
        {
            // Auto-find sprite renderer if not assigned
            // The sprite might be on a child object (sprite on top of button)
            if (buttonRenderer == null)
            {
                buttonRenderer = GetComponent<SpriteRenderer>();
                if (buttonRenderer == null)
                    buttonRenderer = GetComponentInChildren<SpriteRenderer>();
            }
            
            // Set initial sprite
            if (buttonRenderer != null && unpressedSprite != null)
                buttonRenderer.sprite = unpressedSprite;
        }
        
        /// <summary>
        /// Register a wall to be controlled by this button
        /// </summary>
        public void RegisterWall(TemporaryWall wall)
        {
            if (wall != null && !controlledWalls.Contains(wall))
            {
                controlledWalls.Add(wall);
            }
        }
        
        /// <summary>
        /// Unregister a wall from this button
        /// </summary>
        public void UnregisterWall(TemporaryWall wall)
        {
            if (wall != null && controlledWalls.Contains(wall))
            {
                controlledWalls.Remove(wall);
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Check if player stepped on the button
            if (!other.CompareTag("Player")) return;
            
            // Check if button can be pressed
            if (!canBeReused && isPressed)
            {
                Debug.Log($"[TemporaryWallButton]: Button cannot be reused");
                return;
            }
            
            if (isOnCooldown)
            {
                Debug.Log($"[TemporaryWallButton]: Button is on cooldown for {cooldownMovesRemaining} more moves");
                return;
            }
            
            PressButton();
        }
        
        private void PressButton()
        {
            isPressed = true;
            
            // Visual feedback
            if (buttonRenderer != null && pressedSprite != null)
                buttonRenderer.sprite = pressedSprite;
            
            // Particle effect
            if (pressEffect != null)
                pressEffect.Play();
            
            // Audio feedback
            if (pressSound != null)
            {
                var audioService = Core.ServiceLocator.Get<Core.ModernAudioService>();
                if (audioService != null)
                {
                    audioService.PlaySFX(pressSound);
                }
                else
                {
                    // Fallback to simple audio source
                    AudioSource.PlayClipAtPoint(pressSound, transform.position);
                }
            }
            
            // Activate all controlled walls
            foreach (var wall in controlledWalls)
            {
                if (wall != null)
                {
                    wall.Activate(movesDuration);
                }
            }
            
            Debug.Log($"[TemporaryWallButton]: Button pressed! Walls will disappear for {movesDuration} moves");
            
            // Start cooldown if button is reusable
            if (canBeReused && cooldownMoves > 0)
            {
                StartCooldown();
            }
        }
        
        private void StartCooldown()
        {
            isOnCooldown = true;
            cooldownMovesRemaining = cooldownMoves;
            
            // Subscribe to move counter to track cooldown
            var moveCounter = Core.ServiceLocator.Get<Core.MoveCounter>();
            if (moveCounter != null)
            {
                moveCounter.OnMovesChanged += OnMoveForCooldown;
            }
        }
        
        private int lastMoveCountForCooldown = 0;
        
        private void OnMoveForCooldown(object sender, Core.MoveCountChangedEventArgs e)
        {
            // Only count moves that actually increase (not resets)
            if (e.MoveCount > lastMoveCountForCooldown)
            {
                cooldownMovesRemaining--;
                
                if (cooldownMovesRemaining <= 0)
                {
                    // Cooldown finished
                    isOnCooldown = false;
                    isPressed = false;
                    
                    // Reset visual
                    if (buttonRenderer != null && unpressedSprite != null)
                        buttonRenderer.sprite = unpressedSprite;
                    
                    // Unsubscribe
                    var moveCounter = Core.ServiceLocator.Get<Core.MoveCounter>();
                    if (moveCounter != null)
                    {
                        moveCounter.OnMovesChanged -= OnMoveForCooldown;
                    }
                    
                    Debug.Log($"[TemporaryWallButton]: Cooldown complete, button can be pressed again");
                }
            }
            
            lastMoveCountForCooldown = e.MoveCount;
        }
        
        /// <summary>
        /// Get the number of moves the walls will disappear for
        /// </summary>
        public int GetMovesDuration()
        {
            return movesDuration;
        }
        
        /// <summary>
        /// Set the number of moves the walls will disappear for (useful for dynamic level design)
        /// </summary>
        public void SetMovesDuration(int moves)
        {
            movesDuration = Mathf.Max(1, moves); // At least 1 move
        }
        
        /// <summary>
        /// Check if the button is currently pressed
        /// </summary>
        public bool IsPressed()
        {
            return isPressed;
        }
        
        /// <summary>
        /// Check if the button is on cooldown
        /// </summary>
        public bool IsOnCooldown()
        {
            return isOnCooldown;
        }
        
        /// <summary>
        /// Get remaining cooldown moves
        /// </summary>
        public int GetCooldownMovesRemaining()
        {
            return cooldownMovesRemaining;
        }
        
        /// <summary>
        /// Manually reset the button (for level resets)
        /// </summary>
        public void ResetButton()
        {
            isPressed = false;
            isOnCooldown = false;
            cooldownMovesRemaining = 0;
            
            // Reset visual
            if (buttonRenderer != null && unpressedSprite != null)
                buttonRenderer.sprite = unpressedSprite;
            
            // Unsubscribe from move counter if subscribed
            var moveCounter = Core.ServiceLocator.Get<Core.MoveCounter>();
            if (moveCounter != null)
            {
                moveCounter.OnMovesChanged -= OnMoveForCooldown;
            }
        }
        
        private void OnDestroy()
        {
            // Clean up event subscription
            var moveCounter = Core.ServiceLocator.Get<Core.MoveCounter>();
            if (moveCounter != null)
            {
                moveCounter.OnMovesChanged -= OnMoveForCooldown;
            }
        }
        
        private void OnValidate()
        {
            // Ensure movesDuration is at least 1
            if (movesDuration < 1)
                movesDuration = 1;
            
            // Ensure cooldownMoves is non-negative
            if (cooldownMoves < 0)
                cooldownMoves = 0;
            
            // Auto-assign sprite renderer in editor
            // The sprite might be on a child object (sprite on top of button)
            if (buttonRenderer == null)
            {
                buttonRenderer = GetComponent<SpriteRenderer>();
                if (buttonRenderer == null)
                    buttonRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }
    }
}
