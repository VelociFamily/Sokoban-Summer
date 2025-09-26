# MoveCounter and Timer Fix - Unity Editor Setup Guide

## Problem Summary
The moves counter and timer were not working in most levels and tutorials because:
1. **MoveCounter singleton wasn't persisting across scenes** - It was destroyed during scene transitions
2. **GameInitializer wasn't creating MoveCounter** - Some scenes didn't have a MoveCounter instance
3. **UI components weren't properly assigned** - Move and timer text fields weren't linked to the MoveCounter

## Code Changes Made
✅ **Fixed MoveCounter singleton persistence** - Added `DontDestroyOnLoad()` and proper scene transition handling  
✅ **Added MoveCounter to GameInitializer** - Ensures MoveCounter always exists  
✅ **Added automatic UI component discovery** - MoveCounter can now find UI elements by name  
✅ **Fixed LevelLogger singleton** - Now properly initializes and persists  
✅ **Added scene-based counter reset** - Counters reset when entering new gameplay levels  

## Unity Editor Setup Instructions

### 1. Update GameInitializer Prefab (REQUIRED)

**Location**: Find the `GameInitializer` prefab (likely in Game.unity scene or as a prefab asset)

**Steps**:
1. Select the `GameInitializer` GameObject
2. In the Inspector, find the `GameInitializer` component
3. **Add MoveCounter Prefab Reference**:
   - Look for the new `MoveCounterPrefab` field under "Other Systems"
   - Create a MoveCounter prefab (see step 2) or leave empty for auto-creation
4. Save the scene/prefab

### 2. Create MoveCounter Prefab (RECOMMENDED)

**Create the prefab**:
1. Create an empty GameObject in any scene
2. Name it "MoveCounter"
3. Add the `MoveCounter` component
4. **Configure UI References** (optional - auto-discovery will handle this if not set):
   - `Move Text`: Drag a TextMeshPro component for displaying moves
   - `Timer Text`: Drag a TextMeshPro component for displaying time  
   - `Level Complete Canvas`: Drag the canvas that appears when level is completed
5. Drag the GameObject to Project window to create a prefab
6. Delete the GameObject from the scene
7. Assign this prefab to GameInitializer's `MoveCounterPrefab` field

### 3. UI Component Naming Convention (IMPORTANT)

The MoveCounter uses an **improved auto-discovery algorithm** with prioritized matching and exclusion rules:

**Move Text Component**:
- **Best matches**: "move", "moves", "step", "steps" in name/parent name
- **Excludes**: Components containing "time", "timer", "second" 
- **Priority**: Exact name match > GameObject name > Parent name > Text content
- Examples: "MoveText", "Move Counter", "UI_Move_Display", "Steps_Counter"

**Timer Text Component**:
- **Best matches**: "time", "timer", "clock", "duration" in name/parent name  
- **Excludes**: Components containing "move", "step", "count"
- **Priority**: Exact name match > GameObject name > Parent name > Text content
- Examples: "TimerText", "Time Display", "UI_Time_Counter", "Clock_Display"

**Level Complete Canvas**:
- Canvas GameObject name should contain "complete" (case-insensitive)
- Examples: "CompleteCanvas", "Level Complete UI", "Victory_Complete"

**Debug Features**:
- Enable "Verbose Logging" on MoveCounter component for detailed assignment logs
- Use "Re-discover UI Components" context menu to force re-assignment
- Use MoveCounterTimerTest "Debug UI Component Discovery" to see all available components

### 4. Scene-Specific Setup

**For each Level/Tutorial scene**:

**Option A: Use Auto-Discovery (Recommended)**
- Just ensure UI components follow the naming convention above
- MoveCounter will automatically find and link them

**Option B: Manual Assignment**  
- If auto-discovery doesn't work, add a MoveCounter GameObject to the scene
- Manually assign the UI components in the Inspector
- The persistent MoveCounter will take precedence, so this is mainly for testing

### 5. Testing the Fix

**Test in Unity Editor**:
1. Play the Game scene (this initializes MoveCounter)
2. Navigate to any tutorial or level
3. Check Console for messages:
   - ✅ `[MoveCounter]: Instance initialized and persisted across scenes`
   - ✅ `[MoveCounter]: Auto-assigned moveText to 'TextName'`
   - ✅ `[MoveCounter]: Auto-assigned timerText to 'TextName'`
4. **Move the player** - should see move counter increment
5. **Watch timer** - should see time counting up
6. **Complete level** - timer should stop when completion canvas appears

**Use the Test Script** (Recommended):
1. Add the `MoveCounterTimerTest` component to any GameObject in a scene
2. Set "Run Test On Start" to true for automatic testing
3. Play the scene and check Console for test results
4. Or right-click the component and use "Run MoveCounter Test" from context menu
5. Individual test methods available: "Test Move Increment" and "Test Counter Reset"

**Test Scene Transitions**:
1. Complete a level and go to next level
2. Move counter should reset to 0
3. Timer should reset to 00:00.00
4. Both should start working immediately in new level

## Troubleshooting

### Move Counter Not Incrementing
- **Check**: PlayerController exists in scene and can move
- **Check**: Console for `[MoveCounter]: Move text UI component not assigned` warnings
- **Fix**: Ensure UI component naming follows convention or manually assign

### Timer Not Running  
- **Check**: Console for `[MoveCounter]: Timer text UI component not assigned` warnings
- **Check**: Timer text component exists and is named correctly
- **Fix**: Create a TextMeshPro component for timer display

### Timer Not Stopping at Level Complete
- **Check**: Level complete canvas is properly named and detected
- **Check**: Canvas becomes active when level is completed
- **Fix**: Ensure completion canvas name contains "complete"

### UI Not Found Automatically / Wrong Assignment
- **Check**: Component names contain the keywords ("move", "time", "complete")
- **Check**: Components are TextMeshProUGUI (not legacy Text)
- **Check**: Use MoveCounterTimerTest with "Debug UI Component Discovery" to see all available components
- **Fix**: 
  - Rename components or parents to follow convention
  - Use MoveCounter's "Re-discover UI Components" context menu option
  - Manually assign components in MoveCounter Inspector if auto-discovery fails
  - Enable "Verbose Logging" on MoveCounter for detailed assignment logs

### Counters Don't Reset Between Levels
- **Check**: Scenes have proper SceneInfo components with correct SceneType
- **Check**: Console for scene transition messages
- **Fix**: Add SceneInfo component to scene and set SceneType to TutorialLevel or GameplayLevel

## Testing Checklist

- [ ] Game.unity scene loads and initializes MoveCounter
- [ ] Moving Tutorial: Counter and timer work
- [ ] Button Tutorial: Counter and timer work  
- [ ] Speed Tutorial: Counter and timer work
- [ ] Confuse Tutorial: Counter and timer work
- [ ] Level One: Counter and timer work
- [ ] Level Two: Counter and timer work (should still work as before)
- [ ] All levels: Counters reset when entering new level
- [ ] All levels: Timer stops when level is completed
- [ ] Scene transitions work smoothly without errors

## Notes

- **Backward Compatibility**: Existing manually assigned UI components will still work
- **Performance**: Auto-discovery only runs once per scene load
- **Extensibility**: Easy to add new UI component types following same pattern
- **Error Handling**: Graceful fallbacks if UI components aren't found

If you encounter issues not covered here, check the Unity Console for detailed error messages with the `[MoveCounter]` prefix.