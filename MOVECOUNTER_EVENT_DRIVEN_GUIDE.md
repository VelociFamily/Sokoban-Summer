# MoveCounter Event-Driven Refactoring Guide

## Overview
MoveCounter has been refactored to use an event-driven architecture, eliminating tight UI coupling and reducing GC allocations from per-frame string formatting.

## What Changed

### Before (Old System)
- MoveCounter used `FindObjectsByType<TextMeshProUGUI>()` to auto-discover UI components
- Updated UI text every frame in `Update()`
- Tight coupling between game logic and UI presentation
- GC allocations from string formatting every frame

### After (New System)
- MoveCounter exposes `OnMovesChanged` and `OnTimerChanged` events
- UI components subscribe to events and update only when values change
- Clean separation of concerns
- Minimal GC allocations (events fire only on changes, timer updates ~10x/second)

## How to Use

### Setting Up UI Bindings

#### For Move Counter Display:
1. Add a `TextMeshProUGUI` component to your scene (e.g., named "Moves")
2. Add the `MoveCounterUIBinding` component to the same GameObject
3. The component will auto-discover the TextMeshProUGUI and subscribe to events
4. Optionally customize the format string (default: `"Moves: {0}"`)

#### For Timer Display:
1. Add a `TextMeshProUGUI` component to your scene (e.g., named "Timer")
2. Add the `TimerUIBinding` component to the same GameObject
3. The component will auto-discover the TextMeshProUGUI and subscribe to events
4. Optionally customize the format string (default: `"Time: {0:00}:{1:00}.{2:00}"`)

### Example Scene Setup

```
Canvas
├── Moves (TextMeshProUGUI + MoveCounterUIBinding)
└── Timer (TextMeshProUGUI + TimerUIBinding)
```

### Custom Format Strings

#### MoveCounterUIBinding
- `{0}` = move count
- Examples:
  - `"Moves: {0}"` (default)
  - `"Steps: {0}"`
  - `"{0} moves"`

#### TimerUIBinding
- `{0}` = minutes, `{1}` = seconds, `{2}` = hundredths
- Examples:
  - `"Time: {0:00}:{1:00}.{2:00}"` (default)
  - `"{0}m {1}s"`
  - `"⏱ {0:00}:{1:00}"`

## API Changes

### MoveCounter Public API

#### Events
```csharp
public event EventHandler<MoveCountChangedEventArgs> OnMovesChanged;
public event EventHandler<TimerChangedEventArgs> OnTimerChanged;
```

#### Event Args
```csharp
public class MoveCountChangedEventArgs : EventArgs
{
    public int MoveCount { get; }
}

public class TimerChangedEventArgs : EventArgs
{
    public float ElapsedTime { get; }
}
```

#### Methods (unchanged)
```csharp
public void IncrementMove();
public void ResetCounter();
public float GetElapsedTime();
```

#### Deprecated Fields
```csharp
[Obsolete] public TextMeshProUGUI moveText;  // Use OnMovesChanged event
[Obsolete] public TextMeshProUGUI timerText; // Use OnTimerChanged event
```

## Migration Guide

### For Existing Scenes

If you have existing scenes with UI elements that were auto-discovered by MoveCounter:

1. **Add UI Binding Components:**
   - Find the TextMeshProUGUI GameObjects that display moves/timer
   - Add `MoveCounterUIBinding` to the moves display
   - Add `TimerUIBinding` to the timer display

2. **Remove Old References:**
   - The old `moveText` and `timerText` fields are now obsolete
   - They will still work for backward compatibility but should be migrated

3. **Test Your Scene:**
   - Enter play mode and verify UI updates correctly
   - Check that moves increment when the player moves
   - Verify timer displays and updates properly

### For Code That Directly Updates UI

If you have code that directly modifies `moveText.text` or `timerText.text`:

**Before:**
```csharp
var counter = ServiceLocator.Get<MoveCounter>();
counter.moveText.text = "Custom text";
```

**After:**
```csharp
// Subscribe to events in OnEnable
var counter = ServiceLocator.Get<MoveCounter>();
counter.OnMovesChanged += (sender, e) => {
    // Update your custom UI here
    myText.text = $"Custom: {e.MoveCount}";
};

// Unsubscribe in OnDisable
counter.OnMovesChanged -= OnMovesChanged;
```

## Performance Benefits

### GC Allocation Reduction
- **Before:** String concatenation every frame for timer (~60 allocations/second)
- **After:** String formatting only on changes (~10 allocations/second for timer)
- **Moves:** Only allocates on actual move changes (typically 0-5/second during gameplay)

### CPU Usage
- Eliminated per-frame `FindObjectsByType` calls
- Eliminated per-frame string formatting
- Timer updates reduced from 60 FPS to 10 Hz (still smooth for display)

## Testing

### Automated Tests
- `MoveCounterTimerTest`: Validates event firing and core functionality
- `MoveCounterSceneFilterTest`: Validates event-driven architecture

### Manual Testing
1. Enter a gameplay level
2. Verify moves counter updates when player moves
3. Verify timer displays and increments correctly
4. Check profiler for reduced GC allocations
5. Test level reset to ensure events fire correctly

## Troubleshooting

### UI Not Updating
- Ensure `MoveCounterUIBinding` or `TimerUIBinding` is attached to the UI GameObject
- Check that MoveCounter is initialized via `GameInitializer`
- Verify ServiceLocator contains MoveCounter instance

### Events Not Firing
- Check that MoveCounter's `IncrementMove()` is being called (e.g., by PlayerController)
- Verify timer is running (should start automatically in gameplay scenes)
- Use the test scripts to validate event firing

### Compile Errors
- The old `verboseLogging` field and UI discovery methods have been removed
- Update any code referencing these to use the new event system
- Use the UI binding components instead of direct TextMeshProUGUI references

## Related Files

### Core System
- `Core/MoveCounter.cs` - Event-driven counter logic
- `Core/ServiceLocator.cs` - Service access pattern

### UI Bindings
- `UI/MoveCounterUIBinding.cs` - Binds to OnMovesChanged
- `UI/TimerUIBinding.cs` - Binds to OnTimerChanged

### Tests
- `Testing/MoveCounterTimerTest.cs` - Event functionality tests
- `Testing/MoveCounterSceneFilterTest.cs` - Architecture validation

### Documentation
- `.github/copilot-instructions.md` - Architecture overview
- `MOVECOUNTER_EVENT_DRIVEN_GUIDE.md` - This document
