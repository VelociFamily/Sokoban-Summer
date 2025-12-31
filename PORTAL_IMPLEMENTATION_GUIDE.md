# Portal Block Implementation Guide

## Overview
Portal blocks allow players to teleport from one location to another while preserving their momentum. When a player moves over a portal entrance, they are instantly teleported to a linked portal exit and continue moving in their original direction.

## Key Features
- **Momentum Preservation**: Player maintains their movement direction through the portal
- **Glitch Prevention**: Portal only triggers when player is actively moving INTO the portal (not drifting over it)
- **Instant Teleportation**: Player is teleported after a brief animation (0.3s default)
- **One-Way Logic**: Each portal pair is directional (entrance → exit)
- **State Management**: Uses TeleportingState for consistent player behavior

## Components

### 1. PortalBlock.cs
Located in: `Assets/_Project/SokobanSummer/Scripts/Scripts/Gameplay/PortalBlock.cs`

**Key Methods:**
- `OnTriggerStay2D()`: Detects when player is on the portal and initiates teleport
- `TeleportPlayer()`: Handles the teleportation sequence with animation
- Momentum validation prevents glitching over portals without proper entry

**Key Fields:**
- `linkedPortal`: Reference to the destination portal
- `teleportDuration`: Duration of teleport effect (default: 0.3s)

### 2. Portal Sprite
Located in: `Assets/_Project/SokobanSummer/Sprites/Portal.png`

A simple purple square sprite (10x10 pixels) that serves as the visual representation for portals. You can replace this with a more detailed design later.

## Setup Instructions

### Step 1: Create Portal Sprites in Scene

1. Open **Level One** scene in your project
2. Create an empty GameObject and name it `Portal_Entrance`
3. Add a **SpriteRenderer** component
4. Set the sprite to the Portal sprite
5. Add a **BoxCollider2D** component
6. **IMPORTANT**: Set the BoxCollider2D to `Is Trigger = true` (in the inspector, check the "Is Trigger" box)
7. Set the collider size to approximately `(1, 1)` to match the player size
8. Repeat steps 2-7 to create another portal named `Portal_Exit` at a different location

### Step 2: Configure Portal Connections

1. Select `Portal_Entrance` in the hierarchy
2. Add the **PortalBlock** component (Add Component > Gameplay > PortalBlock)
3. In the PortalBlock inspector:
   - Drag `Portal_Exit` into the "Linked Portal" field
   - Verify "Teleport Duration" is set to 0.3

4. Select `Portal_Exit` in the hierarchy
5. Add the **PortalBlock** component
6. In the PortalBlock inspector:
   - Drag `Portal_Entrance` into the "Linked Portal" field
   - Verify "Teleport Duration" is set to 0.3

**Note**: This creates a two-way portal system. If you want one-way portals, only add PortalBlock to the entrance.

### Step 3: Position Portals

1. Place `Portal_Entrance` at a reasonable distance from walls (e.g., position: (5, 5, 0))
2. Place `Portal_Exit` at another location on the level (e.g., position: (-5, -5, 0))
3. Ensure both portals are on the ground plane (z-axis should be 0)
4. Test by playing the scene and moving the player toward the entrance

### Step 4: Verify Setup

- [ ] Both portals have a BoxCollider2D with "Is Trigger" enabled
- [ ] Portal_Entrance has PortalBlock pointing to Portal_Exit
- [ ] Portal_Exit has PortalBlock pointing back to Portal_Entrance (or to another portal if one-way)
- [ ] Both portals are on the ground layer and properly aligned
- [ ] No walls are blocking portal entrance

## How It Works

### Entry Detection
- Player must be actively moving (momentum > 0.1)
- Portal checks if player is moving INTO the portal (alignment dot product > 0.5)
- This prevents glitching by drifting over the portal

### Teleportation Sequence
1. Player enters portal trigger
2. TeleportingState is activated (effects and sounds play)
3. After 0.3 seconds, player is repositioned at exit portal
4. Player's movement direction is preserved
5. Player returns to Idle state with momentum intact
6. Next frame, player continues moving in original direction

### Momentum Preservation
The player's current movement vector is captured before teleport and reapplied after teleportation. This means:
- If moving right, player exits moving right
- If moving up, player exits moving up
- The teleport is "transparent" to the movement system

## Debugging

### Portal Not Triggering?
- Check that BoxCollider2D has "Is Trigger" enabled
- Verify the collider size matches the player size
- Check that the player is moving INTO the portal (not drifting)
- Open Console and check for errors

### Player Visible in Both Places?
- This shouldn't happen, but if it does, check that both portals are not in the same location
- Verify the linked portal reference is set correctly

### Player Keeps Teleporting Back and Forth?
- The system has a 0.1-second cooldown after teleport to prevent re-trigger
- If this still happens, check that Portal_Exit's exit point is not overlapping with a wall

## Advanced Customization

### Change Teleport Duration
- Select the portal GameObject
- In the PortalBlock component, adjust "Teleport Duration"
- Higher values = longer effect animation
- Lower values = faster teleportation

### Create Portal Chains
You can create chains of portals:
- Portal A → Portal B → Portal C
- Set Portal A's linked portal to B
- Set Portal B's linked portal to C
- Set Portal C's linked portal to A (or another)

### One-Way Portals
- Only add PortalBlock to the entrance portal
- Exit portal has no PortalBlock component
- Player can only teleport in one direction

### Visual Effects
- The Portal sprite can be replaced with any sprite of your choice
- Portal effects are controlled by PlayerController's `teleportEffect` and `teleportSound`
- These are played automatically when TeleportingState is entered

## Testing Checklist

1. **Player can enter portal**
   - Move player toward portal entrance
   - Verify player disappears and reappears at exit

2. **Momentum is preserved**
   - Enter portal moving right
   - Exit should continue moving right
   - Enter portal moving up
   - Exit should continue moving up

3. **No glitching**
   - Try to drift over portal without moving
   - Player should not teleport
   - Try to slide past portal
   - Player should not teleport unexpectedly

4. **Animation plays**
   - When teleporting, see teleport effect/particle
   - Hear teleport sound if available

5. **Two-way works**
   - Teleport A → B
   - Then teleport B → A
   - Both directions should work consistently

## Performance Considerations

- PortalBlock uses OnTriggerStay2D which is efficient for occasional teleportation
- The 0.1-second cooldown prevents frame-by-frame re-triggering
- Multiple portals can coexist without performance impact

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Player doesn't teleport | Check Is Trigger is true, verify collider size, check momentum |
| Player teleports too early | Reduce collider size or check alignment threshold |
| Player glitches through walls | Add portals with space around them, check exit position |
| Teleport effect doesn't play | Check PlayerController has teleportEffect assigned |
| Teleport sound doesn't play | Check PlayerController has teleportSound assigned |

## File References

- **Script**: `Assets/_Project/SokobanSummer/Scripts/Scripts/Gameplay/PortalBlock.cs`
- **Sprite**: `Assets/_Project/SokobanSummer/Sprites/Portal.png`
- **Related**: 
  - `Assets/_Project/SokobanSummer/Scripts/Scripts/Gameplay/PlayerController.cs`
  - `Assets/_Project/SokobanSummer/Scripts/Scripts/Gameplay/TeleportingState.cs`
  - `Assets/_Project/SokobanSummer/Scripts/Scripts/Gameplay/PlayerStateMachine.cs`

## Future Enhancements

- [ ] Portal animation (spinning/pulsing effect)
- [ ] Portal color coding for multi-level puzzles
- [ ] Conditional portals (only accessible with certain items)
- [ ] Portal timers (active only for certain durations)
- [ ] Portal groups (select random exit from group)
- [ ] Portal UI indicator showing connected portals
