using UnityEngine;
using Core;

namespace Gameplay
{
    /// <summary>
    /// A wall that disappears temporarily when activated by a button.
    /// The wall reappears after the player makes a specified number of moves.
    /// </summary>
    public class TemporaryWall : MonoBehaviour
    {
        [Header("Wall Settings")]
        [Tooltip("The button that controls this wall")]
        [SerializeField] private TemporaryWallButton controlButton;
        
        [Header("Visual Components")]
        [Tooltip("The visual renderer for the wall (sprite on top, optional)")]
        [SerializeField] private SpriteRenderer wallRenderer;
        
        [Tooltip("The 3D mesh renderer for the cube wall (auto-detected)")]
        [SerializeField] private MeshRenderer meshRenderer;
        
        [Tooltip("Primary collider for the wall (optional if colliders live on children)")]
        [SerializeField] private Collider2D wallCollider;
        
        // When walls are 3D cubes with 2D colliders on children, collect all of them
        private Collider2D[] wallColliders;
        
        [Header("Visual Feedback")]
        [Tooltip("Optional: Particle effect when wall disappears")]
        [SerializeField] private ParticleSystem disappearEffect;
        
        [Tooltip("Optional: Particle effect when wall reappears")]
        [SerializeField] private ParticleSystem reappearEffect;
        
        [Tooltip("Fade animation duration in seconds")]
        [SerializeField] private float fadeDuration = 0.3f;
        
        private bool isActive = true;
        private int movesRemaining = 0;
        private MoveCounter moveCounter;
        private int lastMoveCount = 0;
        private bool isSubscribedToMoves = false;
        
        private void Awake()
        {
            // Auto-find components if not assigned
            // For 3D cubes, the sprite renderer might be on a child object
            if (wallRenderer == null)
            {
                wallRenderer = GetComponent<SpriteRenderer>();
                if (wallRenderer == null)
                    wallRenderer = GetComponentInChildren<SpriteRenderer>();
            }
            
            // Auto-find mesh renderer for 3D cube walls
            if (meshRenderer == null)
                meshRenderer = GetComponent<MeshRenderer>();
            
            if (wallCollider == null)
                wallCollider = GetComponent<Collider2D>();
            
            // Also gather any child 2D colliders to ensure we disable them too
            wallColliders = GetComponentsInChildren<Collider2D>(true);
            
            // Register with button if assigned
            if (controlButton != null)
            {
                controlButton.RegisterWall(this);
            }
        }
        
        private void Start()
        {
            // Get MoveCounter reference
            moveCounter = ServiceLocator.Get<MoveCounter>();
            if (moveCounter != null)
            {
                lastMoveCount = moveCounter.moveCount;
            }
        }
        
        /// <summary>
        /// Activates the wall (makes it disappear temporarily)
        /// </summary>
        /// <param name="movesDuration">Number of moves before the wall reappears</param>
        public void Activate(int movesDuration)
        {
            if (!isActive) return; // Already deactivated
            
            movesRemaining = movesDuration;
            SetWallState(false);
            
            // Subscribe to move counter events if not already subscribed
            if (!isSubscribedToMoves && moveCounter != null)
            {
                moveCounter.OnMovesChanged += OnMoveCountChanged;
                isSubscribedToMoves = true;
                lastMoveCount = moveCounter.moveCount;
            }
        }
        
        /// <summary>
        /// Manually deactivate the wall (for external control)
        /// </summary>
        public void Deactivate(int movesDuration)
        {
            Activate(movesDuration);
        }
        
        /// <summary>
        /// Manually reactivate the wall immediately
        /// </summary>
        public void Reactivate()
        {
            movesRemaining = 0;
            SetWallState(true);
            
            // Unsubscribe from move events
            if (isSubscribedToMoves && moveCounter != null)
            {
                moveCounter.OnMovesChanged -= OnMoveCountChanged;
                isSubscribedToMoves = false;
            }
        }
        
        private void OnMoveCountChanged(object sender, MoveCountChangedEventArgs e)
        {
            // Only count moves that actually increase (not resets)
            if (e.MoveCount > lastMoveCount)
            {
                movesRemaining--;
                
                if (movesRemaining <= 0)
                {
                    // Wall reappears
                    SetWallState(true);
                    
                    // Unsubscribe from move events
                    if (moveCounter != null)
                    {
                        moveCounter.OnMovesChanged -= OnMoveCountChanged;
                        isSubscribedToMoves = false;
                    }
                }
            }
            
            lastMoveCount = e.MoveCount;
        }
        
        private void SetWallState(bool active)
        {
            isActive = active;
            
            if (active)
            {
                // Wall reappears
                if (reappearEffect != null)
                    reappearEffect.Play();
                
                // Only fade if we have a renderer (for 3D cubes, this might not exist)
                if (wallRenderer != null)
                    StartCoroutine(FadeWall(0f, 1f));
            }
            else
            {
                // Wall disappears
                if (disappearEffect != null)
                    disappearEffect.Play();
                
                // Only fade if we have a renderer (for 3D cubes, this might not exist)
                if (wallRenderer != null)
                    StartCoroutine(FadeWall(1f, 0f));
            }
            
            // Update colliders (root and any children)
            if (wallCollider != null)
                wallCollider.enabled = active;
            if (wallColliders != null)
            {
                for (int i = 0; i < wallColliders.Length; i++)
                {
                    if (wallColliders[i] != null)
                        wallColliders[i].enabled = active;
                }
            }
            
            // Update mesh renderer for 3D cube walls
            if (meshRenderer != null)
                meshRenderer.enabled = active;
        }
        
        private System.Collections.IEnumerator FadeWall(float fromAlpha, float toAlpha)
        {
            if (wallRenderer == null) yield break;
            
            float elapsed = 0f;
            Color color = wallRenderer.color;
            
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                color.a = Mathf.Lerp(fromAlpha, toAlpha, t);
                wallRenderer.color = color;
                yield return null;
            }
            
            // Ensure final alpha is exact
            color.a = toAlpha;
            wallRenderer.color = color;
        }
        
        /// <summary>
        /// Get the current number of moves remaining before wall reappears
        /// </summary>
        public int GetMovesRemaining()
        {
            return movesRemaining;
        }
        
        /// <summary>
        /// Check if the wall is currently active (visible and solid)
        /// </summary>
        public bool IsActive()
        {
            return isActive;
        }
        
        private void OnDestroy()
        {
            // Clean up event subscription
            if (isSubscribedToMoves && moveCounter != null)
            {
                moveCounter.OnMovesChanged -= OnMoveCountChanged;
            }
        }
        
        private void OnValidate()
        {
            // Auto-assign components in editor
            // For 3D cubes, the sprite renderer might be on a child object
            if (wallRenderer == null)
            {
                wallRenderer = GetComponent<SpriteRenderer>();
                if (wallRenderer == null)
                    wallRenderer = GetComponentInChildren<SpriteRenderer>();
            }
            
            // Auto-find mesh renderer for 3D cube walls
            if (meshRenderer == null)
                meshRenderer = GetComponent<MeshRenderer>();
            
            if (wallCollider == null)
                wallCollider = GetComponent<Collider2D>();
            
            // Refresh child collider cache in editor
            wallColliders = GetComponentsInChildren<Collider2D>(true);
        }
    }
}
