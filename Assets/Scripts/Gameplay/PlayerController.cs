using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public AudioSource audioSource;
    private bool canChangeDirection = true;
    private Collider2D col;

    public ParticleSystem confuseEffect;
    public float controllerDeadZone = 0.2f;

    private InputSystem_Actions inputActions;

    private Vector2 moveDirection = Vector2.zero;
    public float moveSpeed = 5f;
    private readonly float normalMoveSpeed = 5f;
    private Rigidbody2D rb;

    public ParticleSystem teleportEffect;
    public AudioClip teleportSound;
    private readonly float teleportSpeed = 20f;

    public LayerMask wallLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        inputActions = new InputSystem_Actions();
        inputActions.Player.Move.performed += OnMovePerformed;
        inputActions.Player.Move.canceled += OnMoveCanceled;
    }

    private void OnEnable()
    {
        inputActions?.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions?.Player.Disable();
    }

    private void OnDestroy()
    {
        if (inputActions == null) return;
        inputActions.Player.Move.performed -= OnMovePerformed;
        inputActions.Player.Move.canceled -= OnMoveCanceled;
        inputActions.Player.Disable();
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

        // Confusion handling
        var isConfused = ConfusePowerDown.confuseTurns > 0;
        if (isConfused)
        {
            if (confuseEffect != null && !confuseEffect.isPlaying)
            {
                confuseEffect.Play();
                Debug.Log($"[PlayerController]: Confusion effect activated - {ConfusePowerDown.confuseTurns} turns remaining");
            }

            var originalDirection = inputDirection;
            if (inputDirection == Vector2.up) inputDirection = Vector2.right;
            else if (inputDirection == Vector2.right) inputDirection = Vector2.up;
            else if (inputDirection == Vector2.down) inputDirection = Vector2.left;
            else if (inputDirection == Vector2.left) inputDirection = Vector2.down;
        }
        // Stop confusion animation if this is the last confused turn
        else if (confuseEffect != null && confuseEffect.isPlaying)
        {
            confuseEffect.Stop();
            Debug.Log("[PlayerController]: Confusion effect ended");
        }

        // Teleport handling
        var teleporting = TeleportPowerUp.teleportTimes > 0;
        moveSpeed = teleporting ? teleportSpeed : normalMoveSpeed;

        if (teleporting)
        {
            if (teleportEffect != null && !teleportEffect.isPlaying)
            {
                teleportEffect.Play();
                Debug.Log($"[PlayerController]: Teleport mode activated - {TeleportPowerUp.teleportTimes} uses remaining");
            }
        }
        // Stop teleport animation if this is the last teleport turn  
        else if (teleportEffect != null && teleportEffect.isPlaying)
        {
            teleportEffect.Stop();
            Debug.Log("[PlayerController]: Teleport mode deactivated");
        }

        if (!TryMove(inputDirection)) return;
        
        // Decrement power-up counters after successful move
        if (isConfused) 
        {
            ConfusePowerDown.confuseTurns--;
            // Stop confusion animation if we just used the last turn
            if (ConfusePowerDown.confuseTurns == 0 && confuseEffect != null && confuseEffect.isPlaying)
            {
                confuseEffect.Stop();
                Debug.Log("[PlayerController]: Confusion effect ended");
            }
        }
        
        if (teleporting)
        {
            TeleportPowerUp.teleportTimes--;
            if (TeleportPowerUp.teleportTimes == 0)
            {
                Debug.Log("[PlayerController]: All teleport uses consumed");
                // Stop teleport animation when all uses are consumed
                if (teleportEffect != null && teleportEffect.isPlaying)
                {
                    teleportEffect.Stop();
                    Debug.Log("[PlayerController]: Teleport mode deactivated");
                }
            }
                
            if (audioSource != null && teleportSound != null)
                audioSource.PlayOneShot(teleportSound);
            else if (teleportSound == null)
                Debug.LogWarning("[PlayerController]: Teleport sound not assigned - cannot play audio feedback");
        }
    }

    private static void OnMoveCanceled(InputAction.CallbackContext context)
    {
    }

    private void Update()
    {
        transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);
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

        MoveCounter.Instance?.IncrementMove();

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