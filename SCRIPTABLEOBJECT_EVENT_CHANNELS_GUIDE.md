# ScriptableObject Event Channels Guide

## Overview
ScriptableObject event channels provide a decoupled, designer-friendly way to communicate between systems and scenes without direct references. This architecture reduces brittle initialization dependencies and polling patterns.

## Architecture

### Core Components

#### 1. **GameEvent (Void Events)**
Simple signals with no parameters.
- **Use for:** Level completed, game paused, button clicked
- **Asset menu:** `Sokoban Summer/Events/Game Event (Void)`
- **Listener:** `GameEventListener` component

#### 2. **IntGameEvent**
Events that pass an integer value.
- **Use for:** Move counts, scores, level numbers
- **Asset menu:** `Sokoban Summer/Events/Int Event`
- **Listener:** `IntGameEventListener` component

#### 3. **StringGameEvent**
Events that pass a string value.
- **Use for:** Level names, power-up types, messages
- **Asset menu:** `Sokoban Summer/Events/String Event`
- **Listener:** `StringGameEventListener` component

### How It Works

```
[Raiser System] → [ScriptableObject Event Asset] → [Listener Components in Scenes]
     ↓                        ↓                              ↓
  Calls Raise()       Stores listeners              Invokes UnityEvent
```

**Key Benefits:**
- No direct references between systems
- Scene objects can react to game events without polling
- Events persist across scene loads (ScriptableObject assets live in project)
- Unity Inspector integration for designers

## Integrated Event Channels

### 1. LevelLoaded (StringGameEvent)
**Raised by:** `LevelManager.LoadLevelAdditiveRoutine()`
**Data:** Level display name (string)
**Use cases:**
- UI updates when a level loads
- Analytics tracking
- Scene-specific initialization

**Integration:**
```csharp
// In LevelManager
[SerializeField] private StringGameEvent levelLoadedEvent;

// When level loads
levelLoadedEvent?.Raise(levelInfo.displayName);
```

### 2. MovesChanged (IntGameEvent)
**Raised by:** `MoveCounter.IncrementMove()` and `MoveCounter.ResetCounter()`
**Data:** Current move count (int)
**Use cases:**
- UI displays (complements C# events from Issue #39)
- Achievement triggers
- Level statistics tracking

**Integration:**
```csharp
// In MoveCounter
[SerializeField] private IntGameEvent movesChangedEvent;

// When moves increment
movesChangedEvent?.Raise(moveCount);
```

**Note:** This complements the C# `OnMovesChanged` event. Use C# events for code-to-code communication, ScriptableObject events for scene-based UI.

### 3. PowerUpConsumed (StringGameEvent)
**Raised by:** `PowerUpManager.HandlePowerUpStateChanged()`
**Data:** Power-up name (string)
**Use cases:**
- UI feedback when power-ups are used
- Sound effects
- Achievement tracking

**Integration:**
```csharp
// In PlayerController
[SerializeField] private StringGameEvent powerUpConsumedEvent;

// Pass to PowerUpManager
_powerUpManager = new PowerUpManager(this, powerUpConsumedEvent);

// PowerUpManager raises event
_powerUpConsumedEvent?.Raise(e.PowerUpName);
```

## Usage Guide

### Creating Event Assets

1. **Create the ScriptableObject:**
   - Right-click in Project window
   - `Create > Sokoban Summer > Events > [Event Type]`
   - Name it descriptively (e.g., `LevelLoadedEvent`, `MovesChangedEvent`)

2. **Store in a sensible location:**
   - Recommended: `Assets/_Project/Data/Events/`
   - Keep events organized by category

### Raising Events (Code)

```csharp
using Core.Events;

public class MySystem : MonoBehaviour
{
    [SerializeField] private GameEvent myEvent;
    [SerializeField] private IntGameEvent scoreChanged;
    [SerializeField] private StringGameEvent messageEvent;

    public void TriggerEvents()
    {
        // Void event
        myEvent?.Raise();

        // Int event
        scoreChanged?.Raise(100);

        // String event
        messageEvent?.Raise("Hello World");
    }
}
```

### Listening to Events (Scene Objects)

#### Option A: Using Listener Components (No Code)
1. Add listener component to GameObject:
   - `GameEventListener` for void events
   - `IntGameEventListener` for int events
   - `StringGameEventListener` for string events

2. Assign the event asset in Inspector

3. Configure UnityEvent response:
   - Can call methods on any component
   - Can pass event data to methods
   - Can chain multiple responses

**Example:**
```
GameObject: MoveCounterUI
  ├─ TextMeshProUGUI
  └─ IntGameEventListener
       Event: MovesChangedEvent (asset)
       Response:
         └─ MoveCounterUIBinding.OnMovesChanged(int)
```

#### Option B: Manual Subscription (Code)
```csharp
using Core.Events;

public class MyListener : MonoBehaviour, IGenericGameEventListener<int>
{
    [SerializeField] private IntGameEvent eventToListenTo;

    private void OnEnable()
    {
        eventToListenTo?.RegisterListener(this);
    }

    private void OnDisable()
    {
        eventToListenTo?.UnregisterListener(this);
    }

    public void OnEventRaised(int data)
    {
        Debug.Log($"Received event with data: {data}");
    }
}
```

## Best Practices

### When to Use ScriptableObject Events

**✅ Good use cases:**
- Cross-scene communication
- UI responding to game state changes
- Designer-configurable responses
- Decoupling systems that don't need tight coupling

**❌ Avoid for:**
- Performance-critical code (small overhead vs direct calls)
- Tight loops or frequent calls (e.g., every frame)
- When caller needs return values or bidirectional communication

### Event Naming Conventions

- **Noun-based:** `LevelLoaded`, `ScoreChanged`, `PowerUpCollected`
- **Past tense:** Indicates something has happened
- **Specific:** `MovesChanged` not `DataChanged`

### Organization

```
Assets/_Project/
├─ Data/
│  └─ Events/
│     ├─ Gameplay/
│     │  ├─ LevelLoadedEvent.asset
│     │  └─ PowerUpConsumedEvent.asset
│     └─ UI/
│        └─ MovesChangedEvent.asset
├─ Scripts/
│  └─ Core/
│     └─ Events/
│        ├─ GameEvent.cs
│        ├─ GameEventListener.cs
│        ├─ IntGameEvent.cs
│        └─ StringGameEvent.cs
```

## Debugging

### Check if Event is Raised
Add a `GameEventListener` with a Debug.Log response:
```csharp
// In UnityEvent response
Debug.Log("Event was raised!");
```

### Check Listener Registration
```csharp
// In event asset
public int GetListenerCount() => listeners.Count;
```

### Common Issues

**Problem:** Event not firing
- ✓ Check that event asset is assigned in Inspector
- ✓ Verify Raise() is actually being called
- ✓ Ensure listener is enabled (OnEnable called)

**Problem:** Listener not responding
- ✓ Check UnityEvent is configured in Inspector
- ✓ Verify target method exists and is public
- ✓ Check parameter types match (int listener for int event)

**Problem:** Events persist between play sessions
- ScriptableObjects retain runtime state in editor
- Use `ClearListeners()` in editor scripts if needed
- Or rely on OnEnable/OnDisable for proper cleanup

## Performance Considerations

### Overhead
- Event raise: O(n) where n = listener count
- Typical overhead: ~0.01ms for 10 listeners
- Negligible for gameplay events (moves, level loads)

### Memory
- Event assets: ~200 bytes each
- Listeners: ~50 bytes per registered listener
- No GC allocations during raise (uses List, not delegates)

### Optimization Tips
- Prefer C# events for high-frequency updates (e.g., per-frame)
- Use ScriptableObject events for discrete game events
- Batch multiple state changes if possible

## Migration from Polling/Direct References

### Before (Polling)
```csharp
// Bad: Polling in Update
void Update()
{
    var moveCounter = ServiceLocator.Get<MoveCounter>();
    if (moveCounter.moveCount != lastMoveCount)
    {
        UpdateUI(moveCounter.moveCount);
        lastMoveCount = moveCounter.moveCount;
    }
}
```

### After (Event-Driven)
```csharp
// Good: Event-driven response
// No code needed! Use IntGameEventListener in Inspector
// Or:
void OnEnable()
{
    movesChangedEvent.RegisterListener(this);
}

public void OnEventRaised(int moveCount)
{
    UpdateUI(moveCount);
}
```

### Before (Direct Reference)
```csharp
// Bad: Direct reference, brittle initialization
public class LevelUI : MonoBehaviour
{
    public LevelManager levelManager; // Must be assigned!
    
    void Start()
    {
        levelManager.OnLevelLoaded += HandleLevelLoaded;
    }
}
```

### After (ScriptableObject Event)
```csharp
// Good: No direct reference needed
// Use StringGameEventListener in Inspector with event asset
// Response automatically configured by designer
```

## Examples

### Example 1: Level Loaded Notification

**Setup:**
1. Create `LevelLoadedEvent.asset` (StringGameEvent)
2. Assign to LevelManager's `levelLoadedEvent` field
3. Add `StringGameEventListener` to UI canvas
4. Configure response to show level name

**Result:** When any level loads, UI automatically updates without direct coupling.

### Example 2: Move Counter Statistics

**Setup:**
1. Create `MovesChangedEvent.asset` (IntGameEvent)
2. Assign to MoveCounter's `movesChangedEvent` field
3. Add `IntGameEventListener` to statistics panel
4. Configure response to update chart/graph

**Result:** Statistics update automatically when moves change, no polling needed.

### Example 3: Power-Up Feedback

**Setup:**
1. Create `PowerUpConsumedEvent.asset` (StringGameEvent)
2. Assign to PlayerController's `powerUpConsumedEvent` field
3. Add `StringGameEventListener` to SFX manager
4. Configure response to play sound based on power-up name

**Result:** Audio feedback plays when power-ups are used, fully designer-configurable.

## Related Systems

### Complementary Patterns
- **C# Events:** Use for code-to-code communication (e.g., `OnMovesChanged` in MoveCounter)
- **ServiceLocator:** Use for singleton service access (e.g., `ServiceLocator.Get<MoveCounter>()`)
- **UnityEvents:** Use for Inspector-configured callbacks within a single component

### When to Use Each
| Pattern | Use Case | Example |
|---------|----------|---------|
| **ScriptableObject Events** | Cross-scene, UI-driven, designer-configurable | Level loaded notification |
| **C# Events** | Code-to-code, performance-critical | Per-frame timer updates |
| **ServiceLocator** | Singleton access | Getting MoveCounter instance |
| **UnityEvents** | Single-component callbacks | Button click handlers |

## Testing

### Unit Tests
```csharp
[Test]
public void EventRaisesCorrectly()
{
    var gameEvent = ScriptableObject.CreateInstance<IntGameEvent>();
    int receivedValue = -1;
    
    var listener = new MockListener();
    listener.OnEventRaised = (val) => receivedValue = val;
    
    gameEvent.RegisterListener(listener);
    gameEvent.Raise(42);
    
    Assert.AreEqual(42, receivedValue);
}
```

### Integration Tests
- Create test scenes with listeners
- Raise events via context menus
- Verify UnityEvent responses trigger correctly

## Troubleshooting

### Event Assets Turn Red in Inspector
- Event asset was deleted or moved
- Recreate asset and reassign

### Listeners Don't Unregister
- Ensure OnDisable is called
- Check for exceptions in OnDisable
- Verify object lifecycle (DontDestroyOnLoad, scene unloading)

### Events Fire Multiple Times
- Listener registering multiple times
- Check OnEnable/OnDisable pairs
- Use `if (!listeners.Contains(listener))` guard

## Related Files

### Core Event System
- `Core/Events/GameEvent.cs` - Void event base
- `Core/Events/GameEventListener.cs` - Void event listener
- `Core/Events/GenericGameEvent.cs` - Typed event base
- `Core/Events/IntGameEvent.cs` - Integer events
- `Core/Events/StringGameEvent.cs` - String events

### Integrated Systems
- `Core/LevelManager.cs` - Raises LevelLoaded
- `Core/MoveCounter.cs` - Raises MovesChanged
- `Gameplay/PowerUpManager.cs` - Raises PowerUpConsumed
- `Gameplay/PlayerController.cs` - Passes event to PowerUpManager

### Documentation
- `.github/copilot-instructions.md` - Architecture overview
- `SCRIPTABLEOBJECT_EVENT_CHANNELS_GUIDE.md` - This document
- `MOVECOUNTER_EVENT_DRIVEN_GUIDE.md` - MoveCounter C# events (Issue #39)
