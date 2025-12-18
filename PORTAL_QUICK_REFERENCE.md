# Portal Block System - Quick Reference Card

## 📋 Files Created

| File | Purpose | Status |
|------|---------|--------|
| `PortalBlock.cs` | Main portal component | ✅ Ready |
| `Portal.png` | Portal sprite | ✅ Ready |
| `PORTAL_IMPLEMENTATION_GUIDE.md` | Full setup guide | ✅ Comprehensive |
| `PORTAL_CODE_EXAMPLES.md` | Code examples | ✅ 12+ examples |
| `PORTAL_SYSTEM_SUMMARY.md` | Overview | ✅ Complete |

## 🚀 5-Minute Setup

```
1. Open Level One scene
2. Create GameObject → Rename "Portal_Entrance"
3. Add Components:
   - SpriteRenderer (set sprite to Portal.png)
   - BoxCollider2D (IMPORTANT: Check "Is Trigger")
   - PortalBlock script
4. Repeat for "Portal_Exit" at different location
5. In Inspector: Drag Portal_Exit into Portal_Entrance's "Linked Portal" field
6. Repeat: Drag Portal_Entrance into Portal_Exit's "Linked Portal" field
7. Play and test!
```

## 🎮 How to Use

### Setup Portal Pair
```
Portal A (Entrance) ← linked to → Portal B (Exit)
Portal B (Exit)     ← linked to → Portal A (Entrance)
```

### One-Way Portal
```
Portal A (Entrance) ← linked to → Portal B (Exit)
Portal B (Exit)     ← NOT linked (no PortalBlock component)
```

### Portal Chain
```
Portal A → Portal B → Portal C → Portal A (loops back)
```

## 🔑 Key Features

✅ **Momentum Preservation** - Player exits moving same direction as entry  
✅ **Glitch Prevention** - Only triggers with active movement INTO portal  
✅ **Automatic Effects** - Uses existing teleport particle effects/sounds  
✅ **State Integration** - Works with player state machine  
✅ **Grid Snapping** - Prevents misalignment issues  
✅ **Cooldown** - 0.1s prevents immediate re-trigger  

## 📐 Requirements

Each portal must have:
- [ ] `BoxCollider2D` component
- [ ] `BoxCollider2D.isTrigger` = **TRUE** ⚠️
- [ ] `PortalBlock` component
- [ ] `linkedPortal` set in inspector
- [ ] "Player" tag on player GameObject
- [ ] Non-zero collider size

## ⚙️ Configuration

| Property | Default | Notes |
|----------|---------|-------|
| Linked Portal | None | Drag exit portal here |
| Teleport Duration | 0.3s | Animation duration |
| Alignment Threshold | 0.5 | Momentum alignment check |
| Re-trigger Cooldown | 0.1s | Built-in, not editable |

## 🧪 Testing Checklist

- [ ] Player teleports when moving INTO portal
- [ ] Player does NOT teleport when drifting over
- [ ] Momentum preserved (exit direction = entry direction)
- [ ] Teleport effect plays (particle system)
- [ ] Teleport sound plays (if assigned)
- [ ] Two-way teleportation works
- [ ] No infinite teleport loops
- [ ] Player grid-aligned at exit

## 🐛 Common Issues

| Issue | Solution |
|-------|----------|
| No teleportation | Check Is Trigger = true |
| Teleports too easily | Increase alignment threshold (0.7) |
| Teleports too easily (drifting) | Increase momentum threshold (0.5) |
| Player visible in both places | Check exit position, may be overlapping entrance |
| Infinite teleport loop | Verify 0.1s cooldown is working |
| Teleport effect doesn't play | Assign teleportEffect to PlayerController |
| Sound doesn't play | Assign teleportSound to PlayerController |

## 🔌 Inspector Workflow

### Step 1: Select Portal_Entrance
```
Name: Portal_Entrance
Transform:
  Position: (5, 5, 0)
Components:
  ✓ SpriteRenderer
  ✓ BoxCollider2D
  ✓ PortalBlock
PortalBlock:
  ├─ Linked Portal: [Drag Portal_Exit here]
  └─ Teleport Duration: 0.3
```

### Step 2: Select Portal_Exit
```
Name: Portal_Exit
Transform:
  Position: (-5, -5, 0)
Components:
  ✓ SpriteRenderer
  ✓ BoxCollider2D
  ✓ PortalBlock
PortalBlock:
  ├─ Linked Portal: [Drag Portal_Entrance here]
  └─ Teleport Duration: 0.3
```

## 📊 Technical Details

### Entry Detection Flow
```
OnTriggerStay2D
├─ Player tag check
├─ Processing state check
├─ Linked portal check
├─ PlayerController check
├─ Momentum magnitude check (> 0.1)
└─ Alignment check (dot product > 0.5)
    └─ TeleportPlayer() coroutine
```

### Teleport Sequence
```
[1] TeleportingState.Enter()
    ├─ Disable direction change
    ├─ Stop movement
    └─ Play effects/sounds

[2] Wait 0.3 seconds (teleport animation)

[3] Reposition player
    ├─ Calculate exit position
    ├─ Grid-snap position
    └─ Move player

[4] Restore momentum
    └─ SetMoveDirection(preserved momentum)

[5] Return to Idle
    ├─ Change state to Idle
    └─ Enable direction change

[6] Cooldown
    └─ Wait 0.1 seconds before re-trigger
```

## 🎨 Customization

### Change Portal Duration
Inspector → PortalBlock → Teleport Duration → 0.5

### Add Custom Effects
1. Assign particle to PlayerController.teleportEffect
2. Assign sound to PlayerController.teleportSound
3. Automatically plays when entering portal

### Create Multiple Portal Pairs
Repeat setup steps for each pair independently.

## 📚 Documentation Files

- **PORTAL_IMPLEMENTATION_GUIDE.md** - Complete setup guide
- **PORTAL_CODE_EXAMPLES.md** - 12+ code examples
- **PORTAL_SYSTEM_SUMMARY.md** - Architecture overview
- **PORTAL_QUICK_REFERENCE.md** - This file

## ✨ Advanced Tips

1. **Select portal in Editor** → Gizmo shows portal connection
2. **Use Portal_A and Portal_B naming** → Clarifies direction
3. **Test with simple level first** → Verify mechanics
4. **Consider camera following** → Smooth on-teleport
5. **Add audio feedback** → Enhances game feel

## 🔄 Momentum Examples

| Entry Direction | Exit Direction | Notes |
|-----------------|----------------|-------|
| Right (1, 0) | Right (1, 0) | Player continues right |
| Up (0, 1) | Up (0, 1) | Player continues up |
| Left (-1, 0) | Left (-1, 0) | Player continues left |
| Down (0, -1) | Down (0, -1) | Player continues down |
| Zero (0, 0) | No teleport | Portal doesn't trigger |

## 🚨 Critical Checklist

Before playing level:
- [ ] `BoxCollider2D.isTrigger` = **TRUE** on BOTH portals
- [ ] Portal A linked to Portal B
- [ ] Portal B linked back to Portal A (or different portal)
- [ ] Both portals have PortalBlock component
- [ ] Player has "Player" tag
- [ ] Sprite assigned (Portal.png or custom)
- [ ] Collider size reasonable (0.8-1.2)

## 🎯 Success Criteria

Portal system working correctly when:
1. ✅ Player enters portal with momentum
2. ✅ Teleport animation plays (0.3s)
3. ✅ Player appears at exit portal
4. ✅ Player continues moving same direction
5. ✅ Player can change direction immediately after
6. ✅ No glitching when drifting over portal
7. ✅ Two-way works if configured
8. ✅ No error logs in Console

---

**Need Help?** → See PORTAL_IMPLEMENTATION_GUIDE.md for detailed troubleshooting
