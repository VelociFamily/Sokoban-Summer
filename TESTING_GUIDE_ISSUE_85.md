# Testing Guide: ScriptableObject Event Channels (Issue #85)

This guide provides step-by-step instructions to complete the manual testing section of Issue #85 and verify all event channels are working correctly.

## Prerequisites

✅ **Already Completed:**
- All event assets created (LevelLoadedEvent, MovesChangedEvent, PowerUpConsumedEvent)
- Event channels assigned in LevelManager, MoveCounter, and PlayerController
- Helper script `EventChannelTestLogger.cs` created in `Assets/_Project/SokobanSummer/Scripts/Scripts/Testing/`

## Test 1: LevelLoaded Event

**Purpose:** Verify that `LevelLoadedEvent` fires when a level is loaded and broadcasts the level name.

### Setup in Unity Editor

1. **Open Main Menu Scene**
   - Navigate to `Assets/_Project/SokobanSummer/Scenes/Main Menu.unity`
   - Double-click to open

2. **Create Test GameObject**
   - Right-click in Hierarchy → `Create Empty`
   - Name it: `Test_LevelLoadedEvent`
   - **Important:** Set the tag to "[TEST]" using the Tag dropdown in the Inspector so it can be easily identified and removed later

3. **Add EventChannelTestLogger Component**
   - Select `Test_LevelLoadedEvent` GameObject
   - In Inspector, click `Add Component`
   - Search for `EventChannelTestLogger`
   - Add the component
   - Configure:
     - ✅ Enable Logging: checked
     - Log Prefix: `[LevelLoaded Test]`

4. **Add StringGameEventListener Component**
   - Click `Add Component` again
   - Search for `StringGameEventListener`
   - Add the component
   - Configure:
     - **Event**: Drag `Assets/_Project/SokobanSummer/Data/Events/Gameplay/LevelLoadedEvent.asset` into the Event field
     - **Response UnityEvent**:
       1. Click the `+` button to add a listener
       2. Drag the same GameObject (`Test_LevelLoadedEvent`) into the object field
       3. In the function dropdown: select `EventChannelTestLogger → LogStringEvent(string)`
       4. The parameter field should say "Dynamic string"

5. **Save the Scene**
   - `Ctrl+S` or `File → Save`

### Running Test 1

1. **Enter Play Mode**
   - Click the Play button or press `Ctrl+P`

2. **Load a Level**
   - From the Main Menu, click any level button (e.g., Tutorial 1 or Level 1)
   - Watch the Console window

3. **Expected Console Output**
   ```
   [LevelLoaded Test] String Event Received: Tutorial 1
   ```
   or
   ```
   [LevelLoaded Test] String Event Received: Level 1
   ```

4. **Test Multiple Levels**
   - Return to Main Menu (if applicable)
   - Load a different level
   - Verify console shows the new level name

### ✅ Test 1 Success Criteria
- [ ] Console shows level name when any level is loaded
- [ ] Level name matches the actual level loaded
- [ ] Event fires every time a different level is loaded
- [ ] No errors in console

---

## Test 2: MovesChanged Event

**Purpose:** Verify that `MovesChangedEvent` fires when the player moves and broadcasts the move count.

### Setup in Unity Editor

1. **Open a Tutorial Scene**
   - Navigate to `Assets/_Project/SokobanSummer/Scenes/Tutorials/Moving Tutorial.unity`
   - Double-click to open

2. **Create Test GameObject**
   - Right-click in Hierarchy → `Create Empty`
   - Name it: `Test_MovesChangedEvent`

3. **Add EventChannelTestLogger Component**
   - Select `Test_MovesChangedEvent` GameObject
   - In Inspector, click `Add Component`
   - Search for `EventChannelTestLogger`
   - Configure:
     - ✅ Enable Logging: checked
     - Log Prefix: `[MovesChanged Test]`

4. **Add IntGameEventListener Component**
   - Click `Add Component`
   - Search for `IntGameEventListener`
   - Add the component
   - Configure:
     - **Event**: Drag `Assets/_Project/SokobanSummer/Data/Events/UI/MovesChangedEvent.asset` into the Event field
     - **Response UnityEvent**:
       1. Click the `+` button
       2. Drag `Test_MovesChangedEvent` GameObject into the object field
       3. Function dropdown: `EventChannelTestLogger → LogIntEvent(int)`
       4. Parameter should say "Dynamic int"

5. **Save the Scene**
   - `Ctrl+S`

### Running Test 2

1. **Enter Play Mode**
   - Open `Moving Tutorial` scene if not already open
   - Click Play button

2. **Move the Player**
   - Use WASD or Arrow Keys to move the player
   - Watch the Console window

3. **Expected Console Output**
   ```
   [MovesChanged Test] Int Event Received: 1
   [MovesChanged Test] Int Event Received: 2
   [MovesChanged Test] Int Event Received: 3
   ```
   (Number increments with each move)

4. **Test Move Counter Reset**
   - Complete the level or restart
   - Move again
   - Verify counter resets to 1

### ✅ Test 2 Success Criteria
- [ ] Console shows move count increment with each player movement
- [ ] Count starts at 1 on first move
- [ ] Count resets to 1 when level restarts
- [ ] Event fires immediately on each move (no delay)
- [ ] No errors in console

---

## Test 3: PowerUpConsumed Event

**Purpose:** Verify that `PowerUpConsumedEvent` fires when a power-up is consumed and broadcasts the power-up name.

### Finding a Scene with Power-Ups

First, identify which scenes have power-ups (teleport pickups, speed boosts, etc.):
- Check `Assets/_Project/SokobanSummer/Scenes/Tutorials/`
- Check `Assets/_Project/SokobanSummer/Scenes/Levels/`
- Look for scenes with teleport or power-up mechanics

**Recommended:** Use `Button Tutorial` or any level with teleport pickups.

### Setup in Unity Editor

1. **Open a Scene with Power-Ups**
   - Navigate to a scene that contains power-ups (e.g., `Button Tutorial.unity`)
   - Double-click to open

2. **Verify Power-Ups Exist**
   - In Hierarchy, look for GameObjects with "teleport" or "pickup" in the name
   - If no power-ups exist, you may need to add a test power-up or use a different scene

3. **Create Test GameObject**
   - Right-click in Hierarchy → `Create Empty`
   - Name it: `Test_PowerUpConsumedEvent`

4. **Add EventChannelTestLogger Component**
   - Select `Test_PowerUpConsumedEvent` GameObject
   - Click `Add Component`
   - Search for `EventChannelTestLogger`
   - Configure:
     - ✅ Enable Logging: checked
     - Log Prefix: `[PowerUpConsumed Test]`

5. **Add StringGameEventListener Component**
   - Click `Add Component`
   - Search for `StringGameEventListener`
   - Add the component
   - Configure:
     - **Event**: Drag `Assets/_Project/SokobanSummer/Data/Events/Gameplay/PowerUpConsumedEvent.asset`
     - **Response UnityEvent**:
       1. Click `+` button
       2. Drag `Test_PowerUpConsumedEvent` into object field
       3. Function: `EventChannelTestLogger → LogStringEvent(string)`
       4. Parameter: "Dynamic string"

6. **Save the Scene**
   - `Ctrl+S`

### Running Test 3

1. **Enter Play Mode**
   - Open the scene with power-ups
   - Click Play

2. **Collect and Use Power-Up**
   - Move player to collect a power-up (e.g., teleport pickup)
   - Use the power-up (check game controls - might be Space or specific key)
   - Watch Console

3. **Expected Console Output**
   ```
   [PowerUpConsumed Test] String Event Received: Teleport
   ```
   or
   ```
   [PowerUpConsumed Test] String Event Received: [PowerUpName]
   ```

4. **Test Multiple Power-Ups**
   - If scene has multiple power-up types, test each one
   - Verify different names appear in console

### ✅ Test 3 Success Criteria
- [ ] Console shows power-up name when consumed
- [ ] Power-up name matches the type used
- [ ] Event fires immediately when power-up is consumed
- [ ] No errors in console

### Troubleshooting Test 3

**If power-up event doesn't fire:**
1. Verify `pf-player.prefab` has `powerUpConsumedEvent` assigned
2. Check that the power-up collection triggers the consumption logic
3. Ensure `PowerUpManager` is properly initialized in `PlayerController`
4. Add debug logs in `PowerUpManager.HandlePowerUpStateChanged()` to trace execution

---

## Cleanup After Testing

Once all tests pass, remove test GameObjects from scenes:

1. **For each test scene:**
   - Open the scene
   - Select the `Test_*` GameObject
   - Right-click → Delete (or press Delete key)
   - Save scene (`Ctrl+S`)

2. **Scenes to clean up:**
   - Main Menu.unity (Test_LevelLoadedEvent)
   - Moving Tutorial.unity (Test_MovesChangedEvent)
   - [Power-up scene] (Test_PowerUpConsumedEvent)

3. **Optional: Keep EventChannelTestLogger Script**
   - The `EventChannelTestLogger.cs` script can remain in the project
   - It's useful for future event channel debugging
   - Located in `Assets/_Project/SokobanSummer/Scripts/Scripts/Testing/`

---

## Final Verification Checklist

Mark these items complete once all tests pass:

### Event Assets
- [x] LevelLoadedEvent.asset exists at correct path
- [x] MovesChangedEvent.asset exists at correct path
- [x] PowerUpConsumedEvent.asset exists at correct path

### System Configuration
- [x] LevelManager has LevelLoadedEvent assigned
- [x] MoveCounter has MovesChangedEvent assigned
- [x] PlayerController has PowerUpConsumedEvent assigned

### Runtime Testing
- [ ] Test 1 (LevelLoaded) - Event fires and shows level name
- [ ] Test 2 (MovesChanged) - Event fires and shows move count
- [ ] Test 3 (PowerUpConsumed) - Event fires and shows power-up name
- [ ] All console logs show expected output
- [ ] No errors in console during any test

### Cleanup
- [ ] Test GameObjects removed from all scenes
- [ ] Scenes saved after cleanup

---

## Reporting Test Results

After completing all tests, document results in Issue #85:

```markdown
## Testing Completed

### Test 1: LevelLoaded Event ✅
- Event fires when loading levels
- Level name correctly passed: [example name]
- Screenshot: [optional]

### Test 2: MovesChanged Event ✅
- Event fires on each player move
- Move count increments correctly: 1, 2, 3...
- Resets properly on level restart

### Test 3: PowerUpConsumed Event ✅
- Event fires when power-ups consumed
- Power-up name correctly passed: [example name]
- Tested in scene: [scene name]

All acceptance criteria met. Issue ready to close.
```

---

## Additional Notes

### Integration with Existing Systems

**MoveCounter:**
- The `MovesChangedEvent` complements the existing C# event `OnMovesChanged`
- UI bindings can use either event system
- C# events: Better for code-to-code communication
- ScriptableObject events: Better for scene-based UI responses

**MoveCounterUIBinding:**
- Currently uses C# events via `MoveCounter.OnMovesChanged`
- Could be refactored to use `IntGameEventListener` instead
- Both approaches are valid; choose based on preference

### Extending Event Channels

If you need additional event channels in the future:

1. **Create new event asset:**
   - Right-click → `Create > Sokoban Summer > Events > [Type]`
   - Name descriptively (e.g., `ScoreChangedEvent`)

2. **Assign in raiser system:**
   - Add `[SerializeField] private IntGameEvent scoreChangedEvent;`
   - Call `scoreChangedEvent?.Raise(newScore);` when appropriate

3. **Add listeners in scenes:**
   - Use `IntGameEventListener` component
   - Wire to UI or debug logger

4. **Test following this guide**

---

## Reference Documentation

- **Main Guide:** `SCRIPTABLEOBJECT_EVENT_CHANNELS_GUIDE.md`
- **Issue:** [#85 - Unity Setup: Create and configure ScriptableObject event channel assets](https://github.com/VelociFamily/Sokoban-Summer/issues/85)
- **Related Issue:** #84 - ScriptableObject event channels implementation (code)
- **Related Issue:** #39 - MoveCounter event-driven refactoring (C# events)

---

## Questions or Issues?

If you encounter problems during testing:

1. Check console for error messages
2. Verify event assets are assigned in Inspector (not missing/red)
3. Ensure listener components are on enabled GameObjects
4. Verify UnityEvent responses are configured correctly
5. Add additional debug logs in raiser systems if needed

Good luck with testing! 🎮
