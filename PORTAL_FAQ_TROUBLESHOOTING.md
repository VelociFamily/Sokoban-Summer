# Portal Block System - FAQ & Troubleshooting

## ❓ Frequently Asked Questions

### Q: What happens if I don't set `Is Trigger = true`?
**A:** The portal will act as a physical wall and the player will collide and bounce off instead of teleporting. This is the #1 reason portals don't work.

### Q: Can I make one-way portals?
**A:** Yes! Only add PortalBlock to the entrance. The exit portal needs no PortalBlock component. Players can only teleport one direction.

### Q: What if I want random portal exits?
**A:** You could extend PortalBlock to have `PortalBlock[] randomExits` and randomly select one. This isn't implemented yet, but the system supports it.

### Q: Can portals teleport the player mid-collision?
**A:** No. Portal detection uses OnTriggerStay2D which only works if both colliders are in the trigger state. You can't teleport through walls.

### Q: Does momentum always preserve?
**A:** Yes, automatically. The system captures the player's direction vector before teleport and reapplies it after.

### Q: Can I change the teleport animation duration?
**A:** Yes. In inspector, set "Teleport Duration" on the PortalBlock component. Higher = longer animation.

### Q: What if the player is on a wall when they enter the portal?
**A:** They're repositioned to the exit portal position, then can move freely. The grid-snapping prevents wedging.

### Q: Can I destroy a portal while a player is teleporting?
**A:** The system has null-checks. Teleport will cancel gracefully with a debug error.

### Q: Do portals work with the momentum preservation of ice?
**A:** Yes! The system uses the player's current `moveDirection` which includes ice effects.

### Q: Can I have 3+ portals?
**A:** Yes! Create portal chains: A→B→C→D→A

### Q: Do I need to configure anything in PlayerController?
**A:** No. Portals work with default settings. Optional: Add teleportEffect particle and teleportSound for audio feedback.

---

## 🔧 Troubleshooting

### Problem: Portal Doesn't Trigger

**Symptom:** Player walks over portal but nothing happens.

**Checklist:**
1. Is BoxCollider2D.isTrigger = true?
   - Go to portal GameObject → Inspector → BoxCollider2D component
   - Look for checkbox "Is Trigger"
   - If unchecked → Check it and save

2. Does player have "Player" tag?
   - Select player in hierarchy
   - Top-left of inspector, "Tag" dropdown
   - Should say "Player"

3. Is PortalBlock component present?
   - Select portal GameObject
   - Look in Inspector for "PortalBlock" component
   - If missing: Add Component → Gameplay → PortalBlock

4. Is linked portal set?
   - Portal should show "Linked Portal" field
   - Should have reference to another portal
   - If empty: Drag the exit portal there

5. Is player moving with momentum?
   - Portal requires active movement (momentum > 0.1)
   - Drifting without input won't trigger
   - Try holding movement input toward portal

6. Is player aligned with portal?
   - Must be moving INTO portal, not away
   - Angle must be > 45° alignment
   - Try approaching more directly

**If all above are correct:**
- Check Console for errors
- Try moving closer to portal center
- Verify player is actually entering collider (use Physics2D Debug visualization)

---

### Problem: Player Teleports Immediately Back

**Symptom:** Player appears at exit, then instantly teleports back.

**Cause:** Exit portal is triggering immediately on reappearance.

**Solution:**
1. Check exit portal position - ensure it's not overlapping with entrance
2. Verify cooldown is working (0.1s built-in delay)
3. Move exit portal further away from entrance
4. If portal exits into a wall, player may be hitting wall and re-entering entrance

**Prevention:**
- Leave at least 1 unit clearance between portals
- Don't place exit portal directly adjacent to entrance
- Ensure exit position is clear and safe

---

### Problem: Teleport Effect Doesn't Play

**Symptom:** Teleportation works but no particle effect.

**Cause:** PlayerController doesn't have teleportEffect assigned.

**Solution:**
1. Select Player GameObject in hierarchy
2. In Inspector, find PlayerController component
3. Look for "Teleport Effect" field
4. Assign a particle system (or drag existing one)

**If effect field is missing:**
- Check that PlayerController script is loaded
- Check for compile errors in Console
- Try reimporting the script

---

### Problem: Teleport Sound Doesn't Play

**Symptom:** Teleportation works but no audio.

**Cause:** PlayerController doesn't have teleportSound assigned.

**Solution:**
1. Select Player GameObject
2. In Inspector → PlayerController component
3. Look for "Teleport Sound" field
4. Assign an audio clip

**If sound field is missing:**
- Check audio files exist in project
- Try assigning any AudioClip to test
- Check for compile errors in Console

---

### Problem: Player Glitches Through Portal

**Symptom:** Player walks over portal but passes through without teleporting.

**Cause:** Usually alignment validation is too strict.

**Solution:**
1. Approach portal more directly (not at angle)
2. Move in cardinal directions (up/down/left/right)
3. If still happening, there may be speed issues

**Advanced:**
- Alignment threshold is 0.5 (45°)
- Could modify PortalBlock.cs line: `if (alignmentDot < 0.5f)`
- Lower value = easier entry, higher = stricter entry

---

### Problem: Player Stuck at Portal

**Symptom:** Player is positioned at portal but can't move.

**Cause:** Usually a collision with walls or improper collider setup.

**Solution:**
1. Move portal away from walls
2. Check portal collider size isn't too large
3. Verify exit position is clear of obstacles
4. Reset scene and try again

**If still stuck:**
- Check player can move in normal world (not portal issue)
- Verify player has BoxCollider2D
- Check player isn't on wrong layer

---

### Problem: Portal References Show "None (PortalBlock)"

**Symptom:** Linked Portal field shows reference but displays "None".

**Cause:** Reference got broken or wrong GameObject type.

**Solution:**
1. Delete the broken reference (clear field)
2. Drag the actual portal GameObject into field
3. Wait for scene to save
4. If still broken: Reimport scene

---

### Problem: Compile Error "PortalBlock not found"

**Symptom:** Script doesn't load, says PortalBlock doesn't exist.

**Cause:** Script file not in correct location or hasn't compiled.

**Solution:**
1. Check file exists at: `Assets/_Project/SokobanSummer/Scripts/Scripts/Gameplay/PortalBlock.cs`
2. If missing: Create it from the provided code
3. Wait 10+ seconds for compilation
4. Check Console for any errors
5. Try Force Recompile (Tools → Reload SceneHierarchy... or reimport)

---

### Problem: Works in Editor but Not in Build

**Symptom:** Portals work in editor but fail in standalone build.

**Cause:** Usually a missing reference or script issue.

**Solution:**
1. Check that PortalBlock script is in a compiled assembly
2. Verify portal scene is in Build Settings
3. Test in editor first to confirm working
4. Try Clean Build

---

### Problem: Multiple Portals Interfering

**Symptom:** Portals are teleporting to wrong destinations.

**Cause:** Linked Portal references are mixed up.

**Solution:**
1. Verify each portal's "Linked Portal" field
2. For each pair: A→B and B→A
3. For chains: A→B→C→D→A
4. Use OnDrawGizmosSelected to visualize connections (select portal)
5. Create a spreadsheet if many portals:
   ```
   Portal_A → Portal_B
   Portal_B → Portal_C
   Portal_C → Portal_A
   ```

---

### Problem: Player Momentum Doesn't Preserve

**Symptom:** Player teleports but doesn't continue moving.

**Cause:** Usually momentum vector is zero when entering.

**Solution:**
1. Verify player is moving INTO portal (hold input)
2. Check GetMoveDirection() is returning correct values
3. If player was stuck before portal, momentum might be zero
4. Test with clean movement from far away

**Advanced Debug:**
- Add Debug.Log to PortalBlock.OnTriggerStay2D
- Print playerMomentum value
- Should not be (0,0) if teleporting

---

### Problem: Scene Won't Load

**Symptom:** Scene crashes or won't load with portals present.

**Cause:** Usually missing component or reference error.

**Solution:**
1. Remove all portals from scene
2. Re-add one portal at a time
3. After each: Load scene and check
4. First broken portal indicates issue
5. Check components and references

---

## 🧪 Validation Tests

### Test 1: Basic Entry
```
1. Create Portal_A at (0, 0)
2. Create Portal_B at (5, 0)
3. Move player toward Portal_A with momentum
Expected: Player teleports to Portal_B
```

### Test 2: Momentum Preservation
```
1. Move player right (momentum = 1, 0)
2. Enter portal moving right
3. Exit portal
Expected: Player continues moving right
```

### Test 3: Glitch Prevention
```
1. Move player near portal without moving into it
2. Let player drift over portal
Expected: No teleport occurs
```

### Test 4: Two-Way Teleport
```
1. Teleport A→B
2. Teleport B→A
Expected: Both directions work correctly
```

### Test 5: Effects Play
```
1. Enter portal
2. Watch for particle effect and hear sound
Expected: Both play automatically
```

---

## 📋 Pre-Implementation Checklist

Before creating portals:
- [ ] Have Portal.png sprite available
- [ ] Know destination positions for portals
- [ ] Level design allows portal placement
- [ ] Player can reach portal locations
- [ ] No critical walls blocking portals
- [ ] Understand momentum mechanics

Before testing:
- [ ] Run validation in editor
- [ ] Check Console for errors
- [ ] Verify all components visible in inspector
- [ ] Test player can move normally first

---

## 🎓 Learning Resources

### Understanding Momentum
- Momentum = Vector2 of movement direction
- Set via `SetMoveDirection()`
- Get via `GetMoveDirection()`
- Preserved through portal by design

### Understanding State Machine
- TeleportingState handles teleport effects
- Called automatically by portal
- No manual setup needed
- Plays effects and sounds

### Understanding Colliders
- Must be Collider2D type (not 3D)
- Must have isTrigger = true
- Size should match player size (~1x1)
- Parent transform position is collider position

---

## 🚨 Emergency Reset

If portals are causing issues:

1. **Remove all portals:**
   - Select portal GameObjects
   - Delete them
   - Save scene

2. **Verify player works:**
   - Move player around level
   - Ensure movement works normally
   - Confirm no errors in Console

3. **Start fresh:**
   - Create one portal pair
   - Test basic functionality
   - Add more portals once working

---

## 📞 Still Having Issues?

1. Check Console for error messages (bottom of screen)
2. Read error message carefully - often indicates exact problem
3. Review PORTAL_IMPLEMENTATION_GUIDE.md detailed section
4. Check PORTAL_CODE_EXAMPLES.md for examples
5. Verify all checklist items above
6. Try simplest possible setup (one portal pair)

---

## ✅ Sign of Success

You know the portal system is working when:
- ✅ Player enters portal
- ✅ Teleport animation/sound plays (0.3s)
- ✅ Player appears at exit portal
- ✅ Player continues moving same direction
- ✅ Can immediately change direction
- ✅ No errors in Console
- ✅ Repeatable and consistent
