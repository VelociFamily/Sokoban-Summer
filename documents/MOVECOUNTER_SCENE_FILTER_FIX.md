# MoveCounter Scene Filter Fix

## Problem Summary

The `MoveCounter` class was using an auto-discovery system to find and assign UI components (move counter text and timer text) in any scene. This caused unintended behavior where it would search for and potentially assign UI elements from:
- Main menu
- Settings screens
- Pause menus
- Other non-gameplay scenes

This was problematic because:
1. It could incorrectly assign menu UI elements to the move counter
2. It wasted processing cycles searching for UI in scenes that don't need tracking
3. It could cause confusion when UI elements in menus had similar names to gameplay elements

## Solution

Modified the `MoveCounter` class to **only search for and assign UI components in gameplay levels** (scenes marked as `TutorialLevel` or `GameplayLevel` in their `SceneInfo` component).

## Changes Made

### 1. Modified `TryFindUIComponents()` Method

**File**: `Assets/Scripts/Core/MoveCounter.cs`

Added an early return check at the beginning of the method:

```csharp
private void TryFindUIComponents()
{
    // Only search for UI components in gameplay levels (not menus, settings, etc.)
    if (!SceneInfo.IsGameplayScene())
    {
        if (verboseLogging)
        {
            Debug.Log($"[MoveCounter]: Skipping UI component discovery - current scene is not a gameplay level (Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name})");
        }
        return;
    }
    
    // ... rest of the UI discovery logic
}
```

This ensures that:
- UI discovery only happens in Tutorial and GameplayLevel scenes
- Non-gameplay scenes are skipped entirely
- Verbose logging provides clear feedback about why discovery was skipped

### 2. Updated `OnSceneLoaded()` Method

**File**: `Assets/Scripts/Core/MoveCounter.cs`

Moved the `DelayedUISearch()` coroutine call inside the gameplay scene check:

```csharp
private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    // Clear UI references when a new scene loads since they're no longer valid
    moveText = null;
    timerText = null;
    levelCompleteCanvas = null;
    
    // Reset counters for new level (only if it's a gameplay scene)
    if (SceneInfo.IsGameplayScene(scene))
    {
        ResetCounter();
        
        // Try to find new UI components in the loaded scene
        // Use a coroutine to delay this until after the scene is fully loaded
        StartCoroutine(DelayedUISearch());
    }
    else
    {
        if (verboseLogging)
        {
            Debug.Log($"[MoveCounter]: Scene '{scene.name}' is not a gameplay scene - skipping UI component discovery");
        }
    }
}
```

This ensures that:
- UI discovery is only triggered when loading a gameplay scene
- Non-gameplay scenes don't trigger any UI search operations
- Counter resets only happen in gameplay scenes

### 3. Added Test Script

**File**: `Assets/Scripts/Testing/MoveCounterSceneFilterTest.cs`

Created a comprehensive test script that validates:
- Scene type detection works correctly
- UI components are only assigned in gameplay scenes
- The discovery system respects scene type boundaries
- Provides context menu options for manual testing and debugging

## Expected Behavior

### In Gameplay Levels (Tutorial and GameplayLevel scenes):
- ✅ MoveCounter searches for move counter and timer UI components
- ✅ Auto-discovery system finds and assigns appropriate TextMeshProUGUI elements
- ✅ Move counter increments when player moves
- ✅ Timer runs and displays elapsed time
- ✅ Counters reset when entering a new level

### In Non-Gameplay Scenes (MainMenu, Settings, Pause, etc.):
- ✅ MoveCounter **skips** UI component discovery entirely
- ✅ No search for TextMeshProUGUI elements
- ✅ UI references remain cleared (null)
- ✅ No counter resets
- ✅ No processing cycles wasted on UI discovery

## Benefits

1. **Correctness**: Move counter and timer only track gameplay in actual levels
2. **Performance**: Avoids unnecessary UI searches in menu scenes
3. **Clarity**: Clear separation between gameplay tracking and menu UI
4. **Maintainability**: Uses existing `SceneInfo.IsGameplayScene()` infrastructure
5. **Backward Compatible**: Doesn't break existing functionality in levels

## Testing

### Automated Testing
Use the `MoveCounterSceneFilterTest` component:
1. Add component to any GameObject in a scene
2. Enable "Run Test On Start" for automatic testing
3. Check Console for test results
4. Use context menu options for manual tests:
   - "Run Scene Filter Test"
   - "Force Rediscover UI Components"
   - "Log Current Scene Information"

### Manual Testing
1. Play from Game.unity scene (initializes MoveCounter)
2. Navigate to Main Menu - verify no UI discovery logs
3. Enter a Tutorial or Level - verify UI discovery happens
4. Enter Settings (if present) - verify no UI discovery
5. Complete a level - verify timer stops
6. Enter next level - verify counters reset and UI discovery happens again

### Expected Console Messages

**In Main Menu or other non-gameplay scenes:**
```
[MoveCounter]: Skipping UI component discovery - current scene is not a gameplay level (Scene: Main Menu)
[MoveCounter]: Scene 'Main Menu' is not a gameplay scene - skipping UI component discovery
```

**In Tutorial or Gameplay Level:**
```
[MoveCounter]: Found exact-named moveText by name 'Moves' on 'UI_Canvas'
[MoveCounter]: Auto-assigned timerText to 'Timer' on 'UI_Canvas'
[MoveCounter]: Game counters reset for new attempt
```

## Troubleshooting

### UI Components Not Being Found in Levels
- **Check**: Verify the scene has a `SceneInfo` component
- **Check**: Verify `SceneInfo.sceneType` is set to `TutorialLevel` or `GameplayLevel`
- **Fix**: Add `SceneInfo` component to scene and set correct scene type

### UI Components Being Assigned in Menus
- **Check**: Verify the scene's `SceneInfo.sceneType` is **not** `TutorialLevel` or `GameplayLevel`
- **Check**: Check Console for discovery logs - should see "skipping" messages
- **Fix**: Set correct scene type on the `SceneInfo` component

### Want to Test Discovery in Non-Gameplay Scene
- This is now **by design** prevented
- If you need move tracking in a non-gameplay scene, manually assign the UI components in the Inspector rather than relying on auto-discovery

## Notes

- This fix leverages the existing `SceneInfo` infrastructure already present in the codebase
- All scenes should have a `SceneInfo` component with the appropriate `sceneType` set
- The fix is minimal and focused - only affects UI discovery logic
- No changes to move counting, timer logic, or other MoveCounter functionality
- Backward compatible with existing levels that have manually assigned UI components

## Security Summary

No security vulnerabilities were introduced by these changes:
- Changes are minimal and focused on conditional logic
- No new external dependencies added
- No user input handling modified
- No file system or network operations involved
- Only affects internal Unity scene object discovery
- CodeQL scan timed out (expected for large Unity projects), but manual review confirms no security issues
