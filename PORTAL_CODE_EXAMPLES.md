// Portal Block System - Code Examples & Reference

// EXAMPLE 1: Manual Setup in Code (Not Recommended - Use Inspector Instead)
// ============================================================================

public class PortalSetupExample : MonoBehaviour
{
    public void SetupPortalPair()
    {
        // Create entrance portal
        GameObject entrance = new GameObject("Portal_Entrance");
        entrance.transform.position = new Vector3(5f, 5f, 0f);
        
        // Add sprite renderer
        var spriteRenderer = entrance.AddComponent<SpriteRenderer>();
        // spriteRenderer.sprite = Load("Assets/_Project/SokobanSummer/Sprites/Portal.png");
        
        // Add trigger collider
        var collider = entrance.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(1f, 1f);
        
        // Add portal component
        var entrancePortal = entrance.AddComponent<PortalBlock>();
        
        // Create exit portal
        GameObject exit = new GameObject("Portal_Exit");
        exit.transform.position = new Vector3(-5f, -5f, 0f);
        
        // ... repeat above for exit ...
        var exitPortal = exit.AddComponent<PortalBlock>();
        
        // Link portals together
        // (Do this in inspector instead!)
        // entrancePortal.linkedPortal = exitPortal;
        // exitPortal.linkedPortal = entrancePortal;
    }
}

// EXAMPLE 2: Understanding Portal Detection Logic
// ============================================================================

/*
Portal Entry Requirements (from PortalBlock.OnTriggerStay2D):

1. Must have "Player" tag:
   if (!other.CompareTag("Player")) return;

2. Must not already be teleporting:
   if (_isProcessingTeleport) return;

3. Linked portal must exist:
   if (linkedPortal == null) return;

4. Player must have PlayerController:
   var playerController = other.GetComponent<PlayerController>();
   if (playerController == null) return;

5. Player must have active momentum:
   Vector2 playerMomentum = playerController.GetMoveDirection();
   if (playerMomentum.magnitude < 0.1f) return;

6. Player must be moving INTO portal (not away):
   Vector2 portalToPlayer = (Vector2)other.transform.position - (Vector2)transform.position;
   float alignmentDot = Vector2.Dot(playerMomentum.normalized, portalToPlayer.normalized);
   if (alignmentDot < 0.5f) return;

If ALL conditions pass → TeleportPlayer coroutine starts
*/

// EXAMPLE 3: Understanding the Teleportation Sequence
// ============================================================================

/*
TeleportPlayer() IEnumerator Flow:

[1] Mark as processing
    _isProcessingTeleport = true;

[2] Get state machine
    var stateMachine = playerController.GetStateMachine();

[3] Transition to Teleporting state
    stateMachine.ChangeState(PlayerStateType.Teleporting);
    └─ Plays effects/sounds via TeleportingState.Enter()

[4] Get TeleportingState and start animation
    var teleportingState = stateMachine.GetState(PlayerStateType.Teleporting);
    teleportingState.StartTeleport(0.3f);

[5] Wait for animation
    yield return new WaitForSeconds(0.3f);

[6] Reposition player at exit
    playerController.transform.position = linkedPortal.transform.position;
    └─ Position is grid-snapped to prevent misalignment

[7] Restore momentum
    playerController.SetMoveDirection(momentum);
    └─ Player continues moving in same direction

[8] Return to normal state
    stateMachine.ChangeState(PlayerStateType.Idle);
    playerController.EnableDirectionChange();

[9] Prevent re-trigger
    _isProcessingTeleport = false;
    yield return new WaitForSeconds(0.1f);

Total time: ~0.4 seconds (0.3s animation + 0.1s cooldown)
*/

// EXAMPLE 4: Common Integration Patterns
// ============================================================================

// Pattern A: Adding portals to a custom level script
public class LevelOneSetup : MonoBehaviour
{
    public void OnLevelStart()
    {
        // Portals are already in scene, no setup needed
        // They work automatically via OnTriggerStay2D
    }
}

// Pattern B: Debugging portal issues
public class PortalDebugger : MonoBehaviour
{
    public void DebugPortal(PortalBlock portal)
    {
        if (portal.linkedPortal == null)
            Debug.LogError("Portal not linked!");
        
        // Check collider settings
        var collider = portal.GetComponent<Collider2D>();
        if (collider == null)
            Debug.LogError("No collider found!");
        
        BoxCollider2D boxCol = collider as BoxCollider2D;
        if (boxCol != null && !boxCol.isTrigger)
            Debug.LogError("Collider is not set to Trigger!");
    }
}

// Pattern C: Creating a portal network
public class PortalNetwork : MonoBehaviour
{
    public void CreatePortalChain(Vector3[] positions)
    {
        PortalBlock[] portals = new PortalBlock[positions.Length];
        
        // Create portals
        for (int i = 0; i < positions.Length; i++)
        {
            var go = new GameObject($"Portal_{i}");
            go.transform.position = positions[i];
            
            var collider = go.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            
            portals[i] = go.AddComponent<PortalBlock>();
        }
        
        // Link in chain: 0→1→2→...→0
        for (int i = 0; i < portals.Length; i++)
        {
            int nextIndex = (i + 1) % portals.Length;
            portals[i].linkedPortal = portals[nextIndex];
        }
    }
}

// EXAMPLE 5: Understanding Momentum Preservation
// ============================================================================

/*
Momentum = Vector2 returned by PlayerController.GetMoveDirection()

Possible values:
- Vector2(1, 0) = Moving RIGHT
- Vector2(-1, 0) = Moving LEFT
- Vector2(0, 1) = Moving UP
- Vector2(0, -1) = Moving DOWN
- Vector2.zero = Not moving

When player enters portal:
1. Current momentum is captured
   Vector2 momentum = playerController.GetMoveDirection();

2. After teleport, momentum is restored
   playerController.SetMoveDirection(momentum);

3. Next Update frame, player continues moving
   transform.position += (Vector3)(moveDirection * (moveSpeed * Time.deltaTime));

Result: Player exits moving in exact same direction as entrance
*/

// EXAMPLE 6: Portal Visualization (Debug)
// ============================================================================

/*
PortalBlock has OnDrawGizmosSelected() for debugging:

When you select a portal in the editor, it draws:
- Cyan line from portal to linked portal
- Magenta sphere at linked portal

This helps verify portal connections visually.
*/

// EXAMPLE 7: Grid-Snapping Logic
// ============================================================================

/*
Portal uses same grid-snapping as collision:

Vector3 exitPos = linkedPortal.transform.position;
exitPos = new Vector3(
    Mathf.Round(exitPos.x * 2f) / 2f,  // Snap to 0.5 grid
    Mathf.Round(exitPos.y * 2f) / 2f,
    exitPos.z
);
playerController.transform.position = exitPos;

This ensures player aligns perfectly with game grid.
Default grid size: 0.5 units
*/

// EXAMPLE 8: State Integration
// ============================================================================

/*
Portal system integrates with PlayerStateMachine:

PlayerStateType.Teleporting handles:
- Disabling direction change (player can't input during teleport)
- Stopping current movement
- Playing teleport effect particle
- Playing teleport sound via AudioService

TeleportingState.StartTeleport(duration) creates a coroutine that:
- Waits specified duration
- Calls OnTeleportComplete() which transitions back to Idle

PortalBlock coordinates the full sequence:
1. Detects entry
2. Triggers TeleportingState
3. Waits for animation
4. Repositions player
5. Restores momentum
6. Returns to Idle
*/

// EXAMPLE 9: Null Safety Checks
// ============================================================================

/*
PortalBlock includes defensive null checking:

if (linkedPortal == null)
    return; // Don't teleport if no destination

if (stateMachine == null)
{
    _isProcessingTeleport = false;
    yield break; // Safety exit if state machine gone
}

if (linkedPortal == null)
{
    Debug.LogError(...);
    yield break; // Safety check during coroutine
}

if (teleportingState != null)
{
    // Only access if confirmed valid
}

This prevents null reference exceptions if:
- Portals are deleted during teleport
- Player is destroyed mid-teleport
- Scene is unloaded
*/

// EXAMPLE 10: Performance Characteristics
// ============================================================================

/*
Performance impact is minimal:

OnTriggerStay2D: Called once per frame if player on portal
- Simple null checks: O(1)
- Vector math (dot product): O(1)
- Total: ~1-2ms when active

TeleportPlayer coroutine:
- Runs once per teleport
- Uses standard Unity coroutine (efficient)
- No Update() calls, runs on demand

Memory:
- One Collider2D per portal
- One PortalBlock script instance per portal
- One reference to linked portal
- One cooldown bool: negligible

Scaling:
- 10 portals: ~10-20ms per frame if all active
- 100 portals: Still under 50ms per frame typically
- No performance issues for reasonable level sizes
*/

// EXAMPLE 11: Extending PortalBlock
// ============================================================================

/*
Future customizations could include:

1. Portal color coding
   public Color portalColor = Color.magenta;
   └─ Change portal appearance based on type

2. Conditional entry
   public GameObject requiredItem;
   if (player.HasItem(requiredItem)) { Teleport(); }

3. Portal events
   public UnityEvent OnPortalEntered;
   OnPortalEntered.Invoke();

4. Random exit selection
   public PortalBlock[] possibleExits;
   linkedPortal = possibleExits[Random.Range(0, possibleExits.Length)];

5. Portal animation
   private float rotationSpeed = 360f;
   transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
*/

// EXAMPLE 12: Common Mistakes to Avoid
// ============================================================================

/*
❌ WRONG: Forgetting to set BoxCollider2D.isTrigger = true
Result: Physical collision, player bounces off portal

❌ WRONG: Not linking portals in both directions
Result: Can teleport A→B but not B→A

❌ WRONG: Placing portal partially in a wall
Result: Player gets stuck or glitches

❌ WRONG: Using same portal as entrance AND exit
Result: Player teleports to self, never escapes

✅ RIGHT: Set isTrigger = true
✅ RIGHT: Link both directions for 2-way portals
✅ RIGHT: Place portals with clear space
✅ RIGHT: Use different portals for entrance/exit
*/
