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

(Initial placeholder commit to enable draft PR.)
