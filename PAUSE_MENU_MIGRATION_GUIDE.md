# Pause Menu Migration Guide (Persistent UI Architecture)

## Overview
The current pause menu lives inside individual gameplay / tutorial scenes (e.g. a dedicated *Pause Button Canvas* in a level). This causes duplication, extra EventSystems / AudioListeners risk, and breaks the clean separation provided by the **PersistentUI** scene architecture. This guide explains how to migrate the pause menu into the PersistentUI scene and integrate it with `MenuNavigator`, ensuring smooth transitions, no flicker, and centralized input handling.

## Goals
- Single, persistent Pause Panel shared across all gameplay scenes
- Flicker‑free show/hide using `CanvasGroup` alpha (no SetActive for panels)
- Consistent input (ESC / Start / Pause) via `InputService` → `PauseButton` / controller logic
- Isolation from level scene loading/unloading (pause UI is never destroyed mid‑game)
- Avoid duplicate `EventSystem` and `AudioListener`

## Problems With Current (In-Scene) Setup
| Issue | Impact |
|-------|--------|
| Pause Canvas inside each level | Duplication, harder maintenance |
| Uses SetActive on menu GameObjects | Visual popping, potential state loss |
| Independent initialization | Risk of missing `InputService` enable/disable sequencing |
| Scene unload destroys pause UI | Lose state if reusing between levels |
| Potential duplicate EventSystem/AudioListener when returning to menus | Input conflicts, audio warnings |

## Target Hierarchy (PersistentUI Scene)
```
PersistentUI (Scene)
└── MenuCanvas (root Canvas, Screen Space Overlay)
    ├── CanvasGroup (root fade during transitions)
    ├── MenuNavigator (manages panels)
    ├── MainMenuPanel (CanvasGroup)
    ├── SettingsPanel (CanvasGroup)
    ├── LevelSelectionPanel (CanvasGroup)
    ├── AchievementsPanel (CanvasGroup)
    ├── CreditsPanel (CanvasGroup)
    └── PausePanel (CanvasGroup)  ← NEW
        ├── TitleText ("Paused")
        ├── StatsContainer (Moves / Timer optional bindings)
        ├── ResumeButton
        ├── MenuButton (return to Main Menu)
        └── Optional Extras (Hints / Controller Icons / Background dim)
```

## Panel Composition Guidelines
| Element | Recommendation |
|---------|---------------|
| Root Panel Object | `PausePanel` (exact string matches name used by `PauseButton.pausePanelName`) |
| Visibility | Controlled by `MenuNavigator.ShowPanel("Pause")` & `HideAllPanels()` |
| CanvasGroup | Required (alpha 0 initially, interactable false, blocksRaycasts false) |
| Stats (Moves/Timer) | Bind via existing UI binding scripts, or leave blank if persistent bindings already show elsewhere |
| Background Dim | Add an Image child covering full panel; adjust alpha when showing PausePanel |
| First Selected | Assign to `ResumeButton` for gamepad navigation (set in MenuNavigator list) |

## Input Flow
```
InputService (central) → invokes actions (UI Escape/Start) → PauseButton component
PauseButton:
  - On ESC/Start: toggle pause
  - Shows PausePanel via MenuNavigator (no SetActive)
  - Time.timeScale = 0 while paused
  - DepthOfField / post-processing optional activation
```
> Ensure `PauseButton` only exists once (e.g., in PersistentUI scene OR a lightweight trigger in gameplay that finds the persistent PauseButton). Prefer **single persistent instance**.

## Migration Steps
1. **Backup**: Commit or stash current changes before large hierarchy edits.
2. **Add PausePanel to PersistentUI**:
   - Under `MenuCanvas`, create `PausePanel` GameObject
   - Add `CanvasGroup` (alpha 0, interactable & blocksRaycasts false)
   - Populate children (Title, ResumeButton, MenuButton, etc.)
3. **Register Panel with MenuNavigator**:
   - In `MenuNavigator.menuPanels` list, add a new entry:
     - `panelName`: `Pause`
     - `canvasGroup`: (PausePanel CanvasGroup)
     - `firstSelectedButton`: (ResumeButton)
4. **Move PauseButton Component**:
   - Remove `PauseButton` GameObject from gameplay scene(s)
   - Create a `PauseController` empty child under `MenuCanvas` or reuse existing object; attach `PauseButton` there
   - Assign `pausePanelName` = `Pause` (matches panelName above)
   - Optional: Remove `pauseMenu` & `pauseMenuCanvasGroup` references if no longer needed (they are now internal; can remain null) – current implementation gracefully falls back.
5. **Clean Up Level Scene**:
   - Delete old *Pause Button Canvas* GameObject
   - Ensure no other pause menu scripts remain
6. **Verify DepthOfField / Post-Processing**:
   - If using blur, keep `Volume` in gameplay scene OR create a *global effects* object in PersistentUI (with `DontDestroyOnLoad`) and adjust references.
7. **Time Scale Safety**:
   - Confirm no other scripts modify `Time.timeScale` while paused. Centralize pause logic in `PauseButton`.
8. **Return to Menu Handling**:
   - On MenuButton click inside PausePanel, call `PauseButton.LoadMenu()` or a refactored method that unloads gameplay scenes then shows MainMenu via `MenuNavigator.ShowMainMenu()` (after scene ops complete).
9. **Scene Loading Path**:
   - Ensure `GameInitializer.UsePersistentUIScene = true` so the PersistentUI loads first, making PausePanel available in all gameplay scenes.
10. **Remove Redundant Duplicates**:
    - Confirm only one `EventSystem` & one `AudioListener` (PersistentUI handles duplication prevention).

## Recommended Minor Refactor (Optional)
Consider renaming `PauseButton` to `PauseMenuController` and trimming unused fields:
```csharp
// Suggested adjustments inside PauseButton.cs
// Remove: public GameObject pauseMenu; public CanvasGroup pauseMenuCanvasGroup;
// Keep: pausePanelName, menuNavigator reference, time scale / DepthOfField logic.
// Replace SetPauseMenuVisibility body with only MenuNavigator calls.
```
This makes the component explicitly panel-driven and removes confusion about inactive GameObject references.

## Returning to Main Menu
Instead of loading by build index directly, prefer:
```csharp
menuNavigator.ShowMainMenu();
// If gameplay scenes must unload:
foreach (var s in GetGameplayScenesLoaded()) SceneManager.UnloadSceneAsync(s);
```
Use your `SceneInfo` helpers (`SceneInfo.IsGameplayScene(scene)`) to detect gameplay scenes.

## Testing Checklist
- [ ] Game starts; PersistentUI (MenuCanvas + PausePanel hidden) present
- [ ] Press ESC/Start in gameplay → PausePanel fades in, time scale = 0
- [ ] Resume button restores time scale = 1 and hides PausePanel (alpha transitions, no flicker)
- [ ] Menu button unloads gameplay scenes and shows MainMenuPanel
- [ ] Only one EventSystem exists during pause
- [ ] DepthOfField (if used) enables on pause and disables on resume
- [ ] Moves / Timer either remain visible (if intended) or are hidden as designed
- [ ] No SetActive calls for panels (search for `SetActive(` on panel objects returns none related to pause panel visibility)
- [ ] Playing multiple levels sequentially does not duplicate PausePanel or PauseButton

## Common Pitfalls
| Pitfall | Fix |
|---------|-----|
| PausePanel name mismatch (`Pause` vs `pause`) | Ensure exact string match in `pausePanelName` & MenuNavigator panelName |
| Trying to disable the panel GameObject | Use CanvasGroup alpha instead; keep GameObject active |
| Multiple PauseButtons in additive scenes | Keep a single instance in PersistentUI |
| Forgetting to add CanvasGroup to PausePanel | Panel won’t fade and may not block raycasts properly |
| ESC does nothing | Verify `InputService` UI action map enabled and `PauseButton` subscribed |
| Menu loads with duplicate EventSystem / AudioListener | Use provided duplication handlers or rely on PersistentUI-only EventSystem |

## FAQ
**Q: Should I keep `pauseMenu` and `pauseMenuCanvasGroup` fields?**  
A: They can be removed after migration; the MenuNavigator path supersedes them.

**Q: How do I support controller navigation?**  
A: Set `firstSelectedButton` for `PausePanel` in `MenuNavigator.menuPanels`. On show, `EventSystem.current.SetSelectedGameObject()` is invoked.

**Q: Can I add animation (scale/blur) rather than simple fade?**  
A: Yes—wrap fade coroutine or add a secondary animator on PausePanel. Keep alpha control for consistent interaction toggling.

**Q: Where do I place achievement or stats overlays during pause?**  
A: Either inside PausePanel (optional) or rely on existing HUD if it should remain visible. If hiding HUD, ensure MoveCounter/timer UI is not part of PausePanel unless desired.

## Migration Verification Script (Optional)
You can add a quick editor utility to assert a single PausePanel exists:
```csharp
#if UNITY_EDITOR
using UnityEditor; using UnityEngine;
public static class PauseMenuValidator
{
    [MenuItem("Tools/Validation/Validate Pause Menu Setup")] public static void Validate()
    {
        var navigators = Object.FindObjectsByType<UI.MenuNavigator>(FindObjectsSortMode.None);
        if (navigators.Length == 0) { Debug.LogError("No MenuNavigator found."); return; }
        var navigator = navigators[0];
        var pausePanel = navigator.GetPanel("Pause");
        if (pausePanel == null) Debug.LogError("Pause panel not registered in MenuNavigator.");
        else if (pausePanel.canvasGroup == null) Debug.LogError("Pause panel missing CanvasGroup.");
        else Debug.Log("Pause menu setup OK.");
    }
}
#endif
```

## Final Summary
Move the pause menu into the `PersistentUI` scene as a `PausePanel` managed by `MenuNavigator`. Use CanvasGroup alpha for visibility, centralize input through `PauseButton` (or renamed `PauseMenuController`), eliminate SetActive toggling, prevent duplication, and validate with a small checklist. This unifies UI logic and reduces maintenance overhead across scenes.

---
If you would like, I can perform the optional refactor (rename component and prune fields). Just ask.
