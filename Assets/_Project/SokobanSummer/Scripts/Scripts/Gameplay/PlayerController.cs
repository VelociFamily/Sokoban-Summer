using System.Linq;
using Core;
using Core.Events;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay
{
    public class PlayerController : MonoBehaviour
    {
        // Removed: public AudioSource audioSource; - now using centralized AudioService
        private bool canChangeDirection = true;
        private Collider2D col;

        public ParticleSystem confuseEffect;
        public float controllerDeadZone = 0.2f;

        private InputSystem_Actions inputActions;

        // Grid-step movement configuration
        [SerializeField] private float stepDuration = 0.12f;
        [SerializeField] private float gridSize = 1f;
        [SerializeField] private LayerMask pushableLayer;

        private Vector2 queuedDirection = Vector2.zero;
        private bool isMoving = false;

        // Legacy fields retained for compatibility (no longer used for continuous move)
        private Vector2 moveDirection = Vector2.zero;
        public float moveSpeed = 5f;
        public readonly float normalMoveSpeed = 5f;
        private Rigidbody2D rb;

        public ParticleSystem teleportEffect;
        public AudioClip teleportSound;
        public readonly float teleportSpeed = 20f;

        public LayerMask wallLayer;

        [Header("Event Channels")]
        [Tooltip("Optional ScriptableObject event raised when power-ups are consumed")]
        [SerializeField] private StringGameEvent powerUpConsumedEvent;

        private PowerUpManager _powerUpManager;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            _powerUpManager = new PowerUpManager(this, powerUpConsumedEvent);
        
            // Use InputService instead of creating our own InputSystem_Actions
            InitializeInput();

            // Auto-bind pushable layer if unset and layer exists
            if (pushableLayer.value == 0)
            {
                int layerIndex = LayerMask.NameToLayer("Pushable");
                if (layerIndex != -1)
                {
                    pushableLayer = 1 << layerIndex;
                    Debug.Log("[PlayerController]: Assigned Pushable layer automatically.");
                }
            }
            
            // Power-up achievement tracking is now handled directly by PowerUpManager
            // via ServiceLocator when conditions are met (no event subscription needed)
        }

        private void Start()
        {
            // Check if power-ups are already active when level starts (e.g., from previous level)
            // and start animations immediately to provide visual feedback
            _powerUpManager.InitializeLevelStart();
        }

        private void InitializeInput()
        {
            // Use centralized input service instead of creating our own
            var inputService = ServiceLocator.Get<InputService>();
            if (inputService.InputActions == null) return;
            inputActions = inputService.InputActions;
            inputActions.Player.Move.performed += OnMovePerformed;
            inputActions.Player.Move.canceled += OnMoveCanceled;
        }

        private void OnEnable()
        {
            var inputService = ServiceLocator.Get<InputService>();
            inputService?.EnablePlayerInput();
        }

        private void OnDisable()
        {
            var inputService = ServiceLocator.Get<InputService>();
            inputService?.DisablePlayerInput();
        }

        private void OnDestroy()
        {
            if (inputActions == null) return;
            inputActions.Player.Move.performed -= OnMovePerformed;
            inputActions.Player.Move.canceled -= OnMoveCanceled;
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            var inputDirection = context.ReadValue<Vector2>();
            if (!(inputDirection.magnitude > controllerDeadZone)) return;
            if (Mathf.Abs(inputDirection.x) > Mathf.Abs(inputDirection.y))
                inputDirection = inputDirection.x > 0 ? Vector2.right : Vector2.left;
            else
                inputDirection = inputDirection.y > 0 ? Vector2.up : Vector2.down;

            // Process power-ups and get modified input direction
            inputDirection = _powerUpManager.ProcessMovement(inputDirection);

            // Queue if currently moving; otherwise try immediately
            if (isMoving || !canChangeDirection)
            {
                queuedDirection = inputDirection;
                return;
            }

            TryStartMove(inputDirection);
        }

        private static void OnMoveCanceled(InputAction.CallbackContext context)
        {
        }

        private void Update() { }

        private bool TryStartMove(Vector2 dir)
        {
            if (isMoving) return false;

            // Compute target tile position
            var startPos = SnapToGrid(transform.position);
            var targetPos = startPos + (Vector3)(dir.normalized * gridSize);

            // If a wall blocks the target tile, cancel
            if (IsBlockedAt(targetPos, wallLayer)) return false;

            // Check for pushable at target; if present, ensure next tile free and schedule push
            Transform pushable = GetBlockingAt(targetPos, pushableLayer);
            Vector3 pushableTarget = Vector3.zero;
            if (pushable != null)
            {
                var nextPos = targetPos + (Vector3)(dir.normalized * gridSize);
                if (IsBlockedAt(nextPos, wallLayer) || GetBlockingAt(nextPos, pushableLayer) != null)
                    return false; // cannot push multiple or into wall
                pushableTarget = nextPos;
            }

            // Start step coroutine (moves player, optionally a crate)
            StartCoroutine(MoveStep(startPos, targetPos, pushable, pushableTarget));
            canChangeDirection = false;
            isMoving = true;

            var moveCounter = ServiceLocator.Get<MoveCounter>();
            moveCounter?.IncrementMove();
            return true;
        }

        private bool IsTouchingWall(Vector2 dir)
        {
            const float moveDistance = 0.4f;
            var targetPos = (Vector2)transform.position + dir * moveDistance;

            var boxCol = col as BoxCollider2D;
            if (boxCol == null)
            {
                Debug.LogError("[PlayerController]: Player must have a BoxCollider2D component for collision detection");
                return true;
            }

            var size = boxCol.size * 0.55f;
            var offset = boxCol.offset;
            var boxCenter = targetPos + offset;

            var hits = Physics2D.OverlapBoxAll(boxCenter, size, 0f, wallLayer);

            return hits.Any(hit => hit != col);
        }

        private bool IsBlockedAt(Vector3 worldPos, LayerMask layer)
        {
            var boxCol = col as BoxCollider2D;
            if (boxCol == null) return true;
            var size = boxCol.size * 0.55f;
            var offset = boxCol.offset;
            var boxCenter = (Vector2)worldPos + offset;
            var hits = Physics2D.OverlapBoxAll(boxCenter, size, 0f, layer);
            return hits.Any(h => h != col);
        }

        private Transform GetBlockingAt(Vector3 worldPos, LayerMask layer)
        {
            var boxCol = col as BoxCollider2D;
            if (boxCol == null) return null;
            var size = boxCol.size * 0.55f;
            var offset = boxCol.offset;
            var boxCenter = (Vector2)worldPos + offset;
            var hits = Physics2D.OverlapBoxAll(boxCenter, size, 0f, layer);
            return hits.FirstOrDefault(h => h != col)?.transform;
        }

        private System.Collections.IEnumerator MoveStep(Vector3 startPos, Vector3 targetPos, Transform pushable, Vector3 pushableTarget)
        {
            // Snap start to grid to prevent drift
            if (rb != null)
                rb.MovePosition((Vector2)startPos);
            else
                transform.position = startPos;

            float elapsed = 0f;
            while (elapsed < stepDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / stepDuration);
                var newPos = Vector3.Lerp(startPos, targetPos, t);
                if (rb != null)
                    rb.MovePosition((Vector2)newPos);
                else
                    transform.position = newPos;

                if (pushable != null)
                {
                    var pushStart = SnapToGrid(pushable.position);
                    var pushPos = Vector3.Lerp(pushStart, pushableTarget, t);
                    var pushRb = pushable.GetComponent<Rigidbody2D>();
                    if (pushRb != null) pushRb.MovePosition((Vector2)pushPos); else pushable.position = pushPos;
                }

                yield return null;
            }

            // Final snap to eliminate float error
            if (rb != null)
                rb.MovePosition((Vector2)targetPos);
            else
                transform.position = targetPos;

            if (pushable != null)
            {
                var pushRb = pushable.GetComponent<Rigidbody2D>();
                if (pushRb != null) pushRb.MovePosition((Vector2)pushableTarget); else pushable.position = pushableTarget;
            }

            isMoving = false;
            canChangeDirection = true;

            // Process power-up consumption after successful move
            _powerUpManager.ProcessPostMove();

            // If there is a queued direction, consume it
            if (queuedDirection != Vector2.zero)
            {
                var next = queuedDirection;
                queuedDirection = Vector2.zero;
                TryStartMove(next);
            }
        }

        private static Vector3 SnapToGrid(Vector3 pos)
        {
            return new Vector3(Mathf.Round(pos.x), Mathf.Round(pos.y), pos.z);
        }

        private void OnCollisionEnter2D(Collision2D collision) { }
    }
}
