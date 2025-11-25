# Persistent UI Scene Migration Guide

## Overview

This guide explains the architectural shift from `MenuPersistence` (SetActive-based) to `PersistentUIManager` (CanvasGroup-based) for managing UI across scene transitions.

## Problem Statement

The previous `MenuPersistence.cs` approach had several issues:

1. **UI Flickering**: Using `SetActive(true/false)` on entire menu hierarchies caused visible flicker during scene transitions
2. **Performance**: Repeatedly enabling/disabling complex UI hierarchies was inefficient
3. **Fragility**: Duplicate EventSystem/AudioListener detection required constant cleanup
4. **Scalability**: The pattern didn't scale well as more menus and dynamic UI were added
5. **State Loss**: Toggling SetActive could reset UI state unexpectedly

## Solution Architecture

### New Components

#### 1. `PersistentUIManager.cs`
- Manages persistent UI elements using **CanvasGroup alpha** instead of SetActive
- Ensures single EventSystem and AudioListener throughout the session
- Loads once additively and stays resident
- Provides smooth fade transitions for UI visibility

#### 2. Persistent UI Scene
- Dedicated scene containing:
  - EventSystem (single instance)
  - AudioListener (single instance)
  - Menu UI hierarchies with CanvasGroup components
  - Other persistent UI elements

#### 3. Updated `GameInitializer.cs`
- New optional field: `PersistentUISceneName` (default: "PersistentUI")
- New toggle: `UsePersistentUIScene` to enable new system
- Loads persistent UI scene before main menu
- Backward compatible with legacy MenuPersistence

## Migration Steps

### Step 1: Create Persistent UI Scene

1. Create a new scene: `Assets/_Project/SokobanSummer/Scenes/PersistentUI.unity`
2. Add the following structure:

```
PersistentUI (Scene)
├── PersistentUIManager (GameObject)
│   ├── PersistentUIManager (Component)
│   ├── EventSystem (Component)
│   └── StandaloneInputModule (Component)
├── MainCamera (with AudioListener) [Optional - or use main menu camera]
└── MenuCanvas (GameObject)
    ├── Canvas (Component)
    ├── CanvasScaler (Component)
    ├── GraphicRaycaster (Component)
    ├── CanvasGroup (Component) ← IMPORTANT
    └── [Your menu UI hierarchy]
```

### Step 2: Configure PersistentUIManager

In the `PersistentUIManager` GameObject inspector:

- **Persistent UI Groups**: Assign CanvasGroup components for UI that should persist
- **Event System**: Assign the EventSystem in the scene
- **Audio Listener**: Assign if you have one in this scene (optional)
- **Show In Main Menu**: `true` (show UI in menu scenes)
- **Show In Gameplay**: `false` (hide UI during gameplay)
- **Fade Duration**: `0.3` (smooth transition time)

### Step 3: Add CanvasGroup to Menu UI

For each menu hierarchy that should fade in/out:

1. Select the root GameObject (e.g., "MainMenu", "SettingsMenu")
2. Add Component → UI → Canvas Group
3. Configure CanvasGroup:
   - **Alpha**: `1` (fully visible by default)
   - **Interactable**: `true`
   - **Block Raycasts**: `true`

### Step 4: Update GameInitializer

1. Open the `Game` scene
2. Select the `GameInitializer` GameObject
3. In the inspector:
   - Set **Use Persistent UI Scene**: `true`
   - Set **Persistent UI Scene Name**: `"PersistentUI"`

### Step 5: Update Build Settings

1. Open **File → Build Settings**
2. Add `PersistentUI.unity` to the build
3. Ensure it's listed **before** any gameplay scenes (order matters for additive loading)

### Step 6: Update PauseButton (Optional)

If you have custom pause menu logic:

1. Add `CanvasGroup pauseMenuCanvasGroup` field
2. Use `SetPauseMenuVisibility(bool visible)` helper method
3. Replace `pauseMenu.SetActive(true/false)` with CanvasGroup alpha changes

Example:
```csharp
// OLD
pauseMenu.SetActive(true);

// NEW
pauseMenuCanvasGroup.alpha = 1f;
pauseMenuCanvasGroup.interactable = true;
pauseMenuCanvasGroup.blocksRaycasts = true;
```

### Step 7: Remove Legacy MenuPersistence (Optional)

Once everything works with PersistentUIManager:

1. Locate any `MenuPersistence` GameObjects in scenes
2. Remove or disable them
3. Update any scripts that reference `MenuPersistence.RestoreMenu()`
4. Clean up tags like "MenuUI" if no longer needed

## API Reference

### PersistentUIManager Public Methods

```csharp
// Show all persistent UI (with optional fade)
PersistentUIManager.Show(animated: true);

// Hide all persistent UI (with optional fade)
PersistentUIManager.Hide(animated: true);

// Check if manager exists
bool exists = PersistentUIManager.Exists;
```

### Visibility Control

The manager automatically shows/hides UI based on scene type:

- **Main Menu Scenes**: UI visible (controlled by `showInMainMenu`)
- **Gameplay Scenes**: UI hidden (controlled by `showInGameplay`)
- **Other Scenes**: UI remains in last state

## Troubleshooting

### Issue: UI doesn't fade, just appears/disappears

**Solution**: Ensure CanvasGroup is attached to the UI root GameObject and assigned in PersistentUIManager's `persistentUIGroups` list.

### Issue: Multiple EventSystems warning

**Solution**: 
1. Check that only ONE scene contains an EventSystem
2. Verify PersistentUIManager's `eventSystem` field is assigned
3. Remove EventSystems from other additively loaded scenes

### Issue: No audio after scene transition

**Solution**: 
1. Ensure only one AudioListener is active
2. Assign the AudioListener in PersistentUIManager
3. Check that `RemoveDuplicateAudioListeners()` is working

### Issue: Input not working

**Solution**:
1. Verify EventSystem is active and assigned
2. Check that CanvasGroup `interactable` and `blocksRaycasts` are `true` when visible
3. Ensure no other EventSystem is conflicting

### Issue: UI flickers on first scene load

**Solution**: Load PersistentUI scene **before** loading the main menu in GameInitializer's initialization sequence.

## Performance Considerations

### Benefits

1. **No GameObject Hierarchy Traversal**: CanvasGroup only changes alpha, doesn't traverse children
2. **No Awake/OnEnable Calls**: UI stays active, preventing repeated initialization
3. **Smooth Transitions**: Hardware-accelerated alpha blending
4. **Predictable State**: UI state preserved across scenes

### Potential Issues

1. **Memory**: All persistent UI stays in memory (usually negligible)
2. **Update Calls**: UI components still receive Update if needed (use `enabled = false` on scripts if not needed)

## Backward Compatibility

The new system is **opt-in** via `GameInitializer.UsePersistentUIScene`:

- **false** (default): Uses legacy MenuPersistence behavior
- **true**: Uses new PersistentUIManager and scene-based approach

This allows gradual migration and testing without breaking existing functionality.

## Testing Checklist

- [ ] PersistentUI scene loads on game start
- [ ] EventSystem is present and only one exists
- [ ] AudioListener is present and only one is active
- [ ] Menu UI fades in smoothly when entering main menu
- [ ] Menu UI fades out smoothly when entering gameplay
- [ ] No duplicate EventSystem warnings in console
- [ ] No duplicate AudioListener warnings in console
- [ ] Input works correctly in menus
- [ ] Pause menu shows/hides smoothly during gameplay
- [ ] Scene transitions are flicker-free
- [ ] Audio plays correctly across scene transitions

## Example Implementation

See the following files for complete implementation:

- `Assets/_Project/SokobanSummer/Scripts/Scripts/Core/PersistentUIManager.cs`
- `Assets/_Project/SokobanSummer/Scripts/Scripts/Core/GameInitializer.cs` (updated)
- `Assets/_Project/SokobanSummer/Scripts/Scripts/UI/PauseButton.cs` (updated)

## Future Enhancements

Possible improvements to this system:

1. **Animation Support**: Add support for Animator-based transitions instead of just alpha fade
2. **Multiple UI Layers**: Support different fade durations per UI layer
3. **Scene-Specific Overrides**: Allow certain scenes to override visibility rules
4. **UI State Persistence**: Save/restore UI state (selected buttons, scroll positions, etc.)
5. **Pooling**: UI element pooling for dynamic menus

## Related Issues

- Issue #48: Original refactoring request
- See also: `SCENE_MANAGEMENT_INSTRUCTIONS.md`, `PERSISTENCE_GUIDE.md`
