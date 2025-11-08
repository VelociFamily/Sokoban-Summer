using System.Linq;
using Core;
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

        private Vector2 moveDirection = Vector2.zero;
        public float moveSpeed = 5f;
        public readonly float normalMoveSpeed = 5f;
        private Rigidbody2D rb;

        public ParticleSystem teleportEffect;
        public AudioClip teleportSound;
        public readonly float teleportSpeed = 20f;

        public LayerMask wallLayer;

        private PowerUpManager _powerUpManager;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            _powerUpManager = new PowerUpManager(this);
        
            // Use InputService instead of creating our own InputSystem_Actions
            InitializeInput();
            
            // Subscribe AchievementManager to power-up events
            SubscribeAchievementManager();
        }

        private void SubscribeAchievementManager()
        {
            if (ServiceLocator.TryGet<AchievementManager>(out var achievementManager))
            {
                achievementManager.SubscribeToPowerUpEvents(_powerUpManager);
            }
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
            if (!canChangeDirection) return;
            var inputDirection = context.ReadValue<Vector2>();

            if (!(inputDirection.magnitude > controllerDeadZone)) return;
            if (Mathf.Abs(inputDirection.x) > Mathf.Abs(inputDirection.y))
                inputDirection = inputDirection.x > 0 ? Vector2.right : Vector2.left;
            else
                inputDirection = inputDirection.y > 0 ? Vector2.up : Vector2.down;

            // Process power-ups and get modified input direction
            inputDirection = _powerUpManager.ProcessMovement(inputDirection);

            if (!TryMove(inputDirection)) return;
        
            // Process power-up consumption after successful move
            _powerUpManager.ProcessPostMove();
        }

        private static void OnMoveCanceled(InputAction.CallbackContext context)
        {
        }

        private void Update()
        {
            transform.position += (Vector3)(moveDirection * (moveSpeed * Time.deltaTime));
        }

        private bool TryMove(Vector2 dir)
        {
            if (IsTouchingWall(dir))
                return false;

            var hit = Physics2D.Raycast(transform.position, dir, 0.4f, wallLayer);
            if (hit.collider != null)
                return false;
        
            moveDirection = dir;
            canChangeDirection = false;

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

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.collider.CompareTag("Wall")) return;
            var contact = collision.GetContact(0);
            var collisionNormal = contact.normal;
            var impactDirection = -collisionNormal;

            if (!(Vector2.Dot(impactDirection.normalized, moveDirection.normalized) > 0.9f)) return;
            moveDirection = Vector2.zero;
            canChangeDirection = true;
        }
    }
}
