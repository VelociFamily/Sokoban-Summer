# Worklog: Issue #37 – Consolidate Singleton Creation through a Service Locator

**Issue:** [#37](https://github.com/VelociFamily/Sokoban-Summer/issues/37)  
**Branch:** `issue/37-consolidate-singleton-service-locator`  
**Status:** In Progress

---

## Context & Problem

Mixed singleton patterns across the codebase—`GameInitializer`/`ModernAudioService` vs ad-hoc static instances in `MoveCounter`, `AchievementManager`, and others—make lifecycle management inconsistent and prone to bugs. This violates best practices for maintainability and testability.

---

## Goal

- Provide a **single service registration/access pattern** for all singletons (Service Locator or Dependency Injection container).
- Instantiate all services from a **central bootstrapping point** (`GameInitializer`).
- Remove ad-hoc static instances in individual classes.

---

## Acceptance Criteria

- [ ] All services are resolved via a single documented access pattern.
- [ ] No class creates its own static instance outside the service layer.
- [ ] Service registration and retrieval are documented in code or in a README.

---

## Assumptions & Constraints

- **Unity constraints:** Services must support additive scene loading and `DontDestroyOnLoad` behavior.
- **Existing architecture:** `GameInitializer.cs` already orchestrates initialization; we'll extend it with a Service Locator.
- **Backward compatibility:** Migrate existing singletons (`ModernAudioService`, `InputService`, `MoveCounter`, `AchievementManager`, `LevelManager`) without breaking existing scene references or inspector assignments.
- **Testing:** Ensure `InitializationValidator.cs` and existing tests continue to pass; add validation for service registration.

---

## Implementation Plan

- [ ] **1. Create `ServiceLocator.cs`**  
  - Implement a simple, thread-safe service locator in `Assets/Scripts/Core/`.
  - Provide `Register<T>(T instance)`, `Get<T>()`, `TryGet<T>(out T service)`, and `Clear()` methods.
  - Document usage and lifecycle in XML comments.

- [ ] **2. Update `GameInitializer.cs`**  
  - Register all services (`ModernAudioService`, `InputService`, `MoveCounter`, `AchievementManager`, `LevelManager`) in `ServiceLocator` after instantiation.
  - Ensure registration happens before scene load and after `DontDestroyOnLoad` is set.

- [ ] **3. Migrate `ModernAudioService.cs`**  
  - Remove `Instance` static property and singleton pattern.
  - Remove `Awake()` singleton logic; rely on `GameInitializer` to instantiate and register.
  - Update all callers to use `ServiceLocator.Get<ModernAudioService>()`.

- [ ] **4. Migrate `InputService.cs`**  
  - Remove `Instance` static property and singleton pattern.
  - Update callers to use `ServiceLocator.Get<InputService>()`.

- [ ] **5. Migrate `MoveCounter.cs`**  
  - Remove `Instance` static property and ad-hoc singleton logic.
  - Instantiate from `GameInitializer` (already done) and register in `ServiceLocator`.
  - Update callers to use `ServiceLocator.Get<MoveCounter>()`.

- [ ] **6. Migrate `AchievementManager.cs`**  
  - Remove static `Instance` and `FindObjectOfType` logic.
  - Instantiate from `GameInitializer` and register in `ServiceLocator`.
  - Update callers to use `ServiceLocator.Get<AchievementManager>()`.

- [ ] **7. Migrate `LevelManager.cs`**  
  - Remove `Instance` static property and singleton pattern.
  - Update callers to use `ServiceLocator.Get<LevelManager>()`.

- [ ] **8. Update all service consumers**  
  - Search workspace for direct `Instance` references to migrated services.
  - Replace with `ServiceLocator.Get<T>()` calls.
  - Add null checks or fallback logic where appropriate.

- [ ] **9. Add validation to `InitializationValidator.cs`**  
  - Add checks to verify all required services are registered in `ServiceLocator` at runtime.
  - Report missing or duplicate service registrations.

- [ ] **10. Update documentation**  
  - Add service locator usage to `README.md` or `ARCHITECTURE.md`.
  - Update `.github/copilot-instructions.md` with new service access pattern.

- [ ] **11. Test in editor and runtime**  
  - Play from `Game` scene; verify all services initialize and are accessible.
  - Run `InitializationValidator` context menu; confirm no errors.
  - Check `MoveCounterTimerTest.cs` and any other tests.

---

## Notes

- **Thread safety:** `ServiceLocator` must be safe for main-thread Unity calls; no multi-threading is expected.
- **Scene reloads:** Services registered in `ServiceLocator` should persist across additive scene loads but be cleared on domain reload (handled by Unity's static field reset).
- **Extension hooks:** If future DI container (e.g., Zenject, VContainer) is adopted, `ServiceLocator` can be replaced with minimal refactoring.

---

## Next Steps

1. Implement `ServiceLocator.cs` with registration/retrieval API.
2. Update `GameInitializer.cs` to register all services.
3. Migrate each service one-by-one, starting with `ModernAudioService`.
4. Update all callers and validate.
