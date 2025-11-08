# Testing Checklist: Issue #36 - Coordinated Service Lifecycle

## Overview
Verify that GameInitializer correctly orchestrates service startup and teardown, preventing duplicate singletons across play-mode sessions and scene transitions.

---

## Test 1: Direct Gameplay Scene Entry
**Objective:** Ensure services initialize once when entering a gameplay scene directly.

### Steps:
1. Open Unity Editor
2. Open `Game` scene (or any scene with `GameInitializer`)
3. Enter Play Mode
4. Check Console for initialization logs:
   - `[GameInitializer]: Starting async game initialization...`
   - `[InputService]: Initializing input systems...`
   - `[ModernAudioService]: Initializing modern audio system...`
   - `[AchievementManager]: Initialized asynchronously with default unlocks`
   - `[MoveCounter]: Instance initialized and persisted across scenes`
5. Stop Play Mode
6. Check Console for teardown logs:
   - `[GameInitializer]: Beginning coordinated service teardown...`
   - `[AchievementManager]: Primary instance destroyed - clearing static reference` (or similar)
   - `[MoveCounter]: Primary instance destroyed - clearing static reference`
   - `[GameInitializer]: Service teardown completed.`
7. Re-enter Play Mode
8. Verify NO duplicate warnings appear (e.g., no `"Duplicate instance detected"`)

**Expected Result:**
- Clean initialization on first play
- Proper teardown logs on stop
- No duplicates on second play session

---

## Test 2: Scene Transition (A → B → A)
**Objective:** Ensure no duplicate singletons when transitioning between scenes.

### Steps:
1. Enter Play Mode in `Game` scene
2. Load a gameplay level scene (e.g., Tutorial_Level_1) additively or via `SceneManager.LoadScene`
3. Check Console - should see:
   - `[MoveCounter]: Scene '...' is not a gameplay scene - skipping UI component discovery` OR
   - `[MoveCounter]: Game counters reset for new attempt` (if gameplay scene)
4. Return to `Main Menu` or load another scene
5. Load back to the original gameplay scene
6. Check Console for duplicate warnings
7. Verify only ONE instance of each singleton exists in Hierarchy:
   - Search for `MoveCounter` GameObject
   - Search for `AchievementManager` GameObject

**Expected Result:**
- No duplicate instance warnings
- Only single GameObject for each singleton in Hierarchy
- Clean UI component re-discovery logs

---

## Test 3: SaveFacade State Persistence
**Objective:** Confirm SaveFacade persists data after shutdown/restart.

### Steps:
1. Enter Play Mode
2. Modify a setting via code or UI (e.g., volume slider)
3. Verify setting saved: Check `SaveFacade.Instance.Settings.masterVolume`
4. Stop Play Mode (triggers `SaveFacade.Shutdown()` which calls `SaveAll()`)
5. Re-enter Play Mode
6. Check `SaveFacade.Instance.Settings.masterVolume` - should retain previous value

**Expected Result:**
- Settings persist across play sessions
- No data loss after shutdown

---

## Test 4: Service Initialization Order
**Objective:** Verify services initialize in the documented order.

### Steps:
1. Open Console
2. Clear Console logs
3. Enter Play Mode in `Game` scene
4. Review initialization sequence in Console timestamps:
   - Background initialization
   - Music player initialization
   - **Core systems (parallel):**
     - SaveFacade (synchronous, first)
     - InputService
     - ModernAudioService
     - AchievementManager
     - MoveCounter
   - LevelLogger
   - Foreground effects
   - Main Menu scene load
   - Splash screen

**Expected Result:**
- Services initialize in documented order
- No errors or null reference exceptions
- Parallel tasks complete successfully

---

## Test 5: Multiple GameInitializer Guard
**Objective:** Ensure `_servicesStarted` guard prevents duplicate initialization if multiple GameInitializers exist.

### Steps:
1. Temporarily duplicate `GameInitializer` GameObject in `Game` scene (for testing only)
2. Enter Play Mode
3. Check Console for:
   - `[GameInitializer]: Services already started - skipping duplicate startup.`
4. Remove duplicate `GameInitializer` after test

**Expected Result:**
- Only one full initialization occurs
- Guard prevents duplicate service creation

---

## Test 6: Play Mode Reentry (Editor-Specific)
**Objective:** Validate clean teardown and restart in Unity Editor play-mode cycling.

### Steps:
1. Enter Play Mode → Stop → Enter again (3-5 cycles)
2. Each cycle should show:
   - Clean startup logs
   - Clean teardown logs
   - No accumulating errors
3. Check Hierarchy after each stop:
   - No orphaned DontDestroyOnLoad objects

**Expected Result:**
- Consistent behavior across all cycles
- No memory leaks or lingering GameObjects

---

## Automated Test Coverage
See `Assets/_Project/SokobanSummer/Scripts/Scripts/Testing/ServiceLifecycleTests.cs` for automated EditMode tests verifying:
- Service startup initializes singletons
- Shutdown clears Instance references
- Guard prevents duplicate initialization

---

## Regression Watch
Monitor for:
- `NullReferenceException` when accessing services after shutdown
- Duplicate instance warnings in Console
- Lost references after scene transitions
- SaveFacade data loss

---

## Sign-off
- [ ] Test 1: Direct scene entry - PASS
- [ ] Test 2: Scene transitions - PASS
- [ ] Test 3: SaveFacade persistence - PASS
- [ ] Test 4: Init order verification - PASS
- [ ] Test 5: Guard behavior - PASS
- [ ] Test 6: Play-mode cycling - PASS
- [ ] Automated tests - PASS

**Tester:** _____________  
**Date:** _____________  
**Notes:** _____________
