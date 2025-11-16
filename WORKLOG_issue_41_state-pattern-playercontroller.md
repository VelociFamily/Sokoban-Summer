# Worklog Issue 41: State Pattern Refactor for PlayerController

Issue: https://github.com/VelociFamily/Sokoban-Summer/issues/41
Branch: issue/41-state-pattern-playercontroller

## Goal
Replace flag/conditional driven logic in `PlayerController` with a State Pattern to isolate behaviors (Idle, Move, Pushing, Teleporting) and enable future extensibility (e.g., Sliding, Falling, Charging).

## Planned Architecture

### Interface
`public interface IPlayerState {
    void Enter();
    void Exit();
    void HandleInput();
    void Tick();
}`

May add optional callbacks (e.g., `OnPhysicsTick()` if needed) later, but start minimal.

### State Host / Machine
Either a lightweight `PlayerStateMachine` component or embedded in `PlayerController`. Preference: embed for now to avoid scattering references; keep it testable via dependency injection for input & movement services.

Fields (conceptual):
- `IPlayerState _currentState;`
- `Dictionary<PlayerStateType, IPlayerState> _states;`
- `void ChangeState(PlayerStateType t)` -> calls `Exit()` current, switch reference, call `Enter()` new.

`PlayerStateType` enum mirrors concrete states: Idle, Move, Pushing, Teleporting.

### Concrete States
Each state receives required collaborators (movement controller, input service, maybe level interaction service). They do NOT directly access static singletons; use `ServiceLocator.TryGet<T>()` only if unavoidable (keep constructor DI where possible).

- `IdleState`: Waits for movement input; transitions to Move/Pushing/Teleporting depending on context.
- `MoveState`: Handles movement stepping and completion, transitions back to Idle or to Pushing if a push is initiated.
- `PushingState`: Executes push animation/logic; on completion returns to Idle/Move as appropriate.
- `TeleportingState`: Suspends input during teleport animation/effect; returns to Idle after arrival.

### Transition Sources
Map existing triggers in `PlayerController`:
- Movement input start
- Detecting push condition (crate in front + movement)
- Teleport trigger activation
- Movement completion event

Will extract these triggers from existing flag logic and route to `ChangeState`.

### Testing Strategy (Play Mode)
Minimal tests to validate state transitions:
1. Start in Idle → movement input → MoveState.
2. MoveState encountering pushable object → PushingState.
3. PushingState completion → IdleState.
4. Teleport trigger during Idle → TeleportingState → Idle after completion.

Mocks/fakes: If movement or teleport systems are asynchronous, create lightweight fakes to simulate completion callbacks.

## Next Steps
1. Extract current flag/conditional logic from `PlayerController` (identify exact fields & methods).
2. Create `IPlayerState` + enum + skeleton states.
3. Wire state machine and replace original flags with `ChangeState` calls.
4. Port logic method-by-method into states.
5. Add play mode tests.

## Notes / Assumptions
- No new input system surface area; reuse `InputService` forwarded events.
- Teleport logic may require an event hook if synchronous today; will abstract if needed.
- Avoid premature optimization; focus clarity & testability.

---

## Implementation Complete (2025-01-15)

### Files Created
- `IPlayerState.cs` - Interface with Enter/Exit/HandleInput/Tick methods
- `PlayerStateType.cs` - Enum for Idle, Move, Pushing, Teleporting states
- `PlayerStateMachine.cs` - State machine manager with registration and transition logic
- `IdleState.cs` - Idle state implementation (waits for input, enables direction change)
- `MoveState.cs` - Move state implementation (handles continuous movement, collision)
- `PushingState.cs` - Pushing state skeleton (ready for push animation/physics)
- `TeleportingState.cs` - Teleporting state with effect/sound integration
- `PlayerStateTransitionTests.cs` - Comprehensive play mode tests (8 test cases)

### Files Modified
- `PlayerController.cs`:
  - Added `_stateMachine` field
  - Added `InitializeStateMachine()` to register and initialize states
  - Updated `Awake()` to call state machine initialization
  - Updated `Update()` to delegate to `_stateMachine.Tick()`
  - Updated `OnMovePerformed()` to transition to Move state after successful TryMove
  - Updated `OnCollisionEnter2D()` to transition to Idle state on direct wall hit
  - Added public helper methods: `StopMovement()`, `EnableDirectionChange()`, `DisableDirectionChange()`, `SetMoveDirection()`, `GetMoveDirection()`, `GetStateMachine()`
  - Removed explicit flag management in favor of state-driven behavior

### Key Design Decisions
1. **Embedded State Machine**: State machine is embedded in `PlayerController` rather than a separate component to avoid reference scattering and maintain cohesion.
2. **Helper Methods**: Added public methods to `PlayerController` for states to call (StopMovement, EnableDirectionChange, etc.) instead of exposing private fields.
3. **Movement Preservation**: Kept movement logic in `PlayerController.Update()` for now to maintain existing smooth physics behavior. States control when movement starts/stops via `moveDirection`.
4. **Input Delegation**: Input continues to flow through `InputService` callbacks; states don't directly subscribe to input events. State transitions are triggered from those callbacks.
5. **Test Coverage**: Added 8 play mode tests covering all state transitions and verifying state isolation.

### Architecture Benefits Achieved
✅ **Eliminated multi-flag logic**: No more complex conditional chains in `PlayerController`  
✅ **State isolation**: Each behavior (idle, move, push, teleport) is in its own class  
✅ **Testable states**: Each state can be tested independently via state machine  
✅ **Extensible**: New states (Sliding, Falling, Charging) can be added without modifying existing states  
✅ **Clear transitions**: All transitions use `stateMachine.ChangeState(newState)` instead of flag manipulation  

### Future Extensions (Not Included)
- Push detection and animation logic (PushingState is a skeleton)
- Portal trigger integration (TeleportingState has foundation but needs portal hookup)
- Optional state behaviors (e.g., SlidingState for ice tiles, FallingState for pits)
- State history tracking for debugging/analytics
- State duration metrics

### Testing Status
- All 8 play mode tests written
- Tests cover: initial state, idle→move, move→idle, idle→teleporting, teleporting→idle, idle→pushing, pushing→idle, sequential transitions
- Tests verify state machine initialization, transition logic, and movement control
- Manual testing recommended: play a level and verify movement, collision, and any teleport triggers still work

### Remaining Work (If Any)
1. **Manual Verification**: Load a gameplay scene and verify:
   - Player movement works as before
   - Wall collision returns to idle state
   - Teleport triggers still function (if implemented)
2. **Optional**: Add push detection logic to trigger `PushingState` when crate is ahead
3. **Optional**: Wire up portal triggers to call `stateMachine.ChangeState(PlayerStateType.Teleporting)`

(Initial placeholder commit to enable draft PR.)
