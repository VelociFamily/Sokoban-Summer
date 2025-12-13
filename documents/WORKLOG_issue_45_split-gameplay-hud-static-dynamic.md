# Worklog: Issue 45 – Split Gameplay HUD into Static and Dynamic Canvases

Issue: https://github.com/VelociFamily/Sokoban-Summer/issues/45
Branch: `issue/45-split-gameplay-hud-static-dynamic`

## Summary
Goal is to reduce unnecessary UI canvas rebuilds by separating immutable HUD elements (static) from rapidly changing ones (dynamic: moves counter, timer, achievement popups if they update, etc.). This should confine dirtying to the DynamicHUD canvas only.

## Acceptance Criteria (from issue)
- Profiler shows minimal rebuilds on value updates; static canvas unaffected.
- Non-interactive visuals have Raycast Target disabled.

## Initial Analysis
Current MoveCounter updates timer text every frame; any shared parent canvas gets flagged dirty. Separating canvases reduces rebuild scope. Need to audit existing HUD scene or menu scene for how UI objects are structured. Some text objects discovered heuristically by `MoveCounter` (names: "Moves", "Timer"). Must ensure discovery still works when moved to DynamicHUD canvas.

## Planned Tasks
1. Inspect current gameplay scene hierarchy for HUD root(s).
2. Introduce two root canvases under a `HUDRoot` empty: `StaticHUD` and `DynamicHUD`.
3. Move dynamic elements (Moves counter, Timer, any animated counters) to `DynamicHUD`.
4. Ensure `MoveCounter` can still find TextMeshProUGUI objects (may require optional parent search tweak if it was assuming a single canvas).
5. Disable Raycast Target on non-interactive graphics/text (set `raycastTarget = false`).
6. Add optional runtime validator script `HudCanvasProfiler` to log canvas rebuild counts in development builds.
7. Profile in Unity: confirm only DynamicHUD dirties during move/timer updates.
8. Document change in `IMPLEMENTATION_SUMMARY.md` or new `UI_OPTIMIZATION_NOTES.md`.

## Risks / Considerations
- Automatic text discovery logic could be canvas-scope limited; may require adjusting search to look across multiple canvases.
- Scene changes can’t be versioned easily here; may add a helper `EnsureHudStructure` script that creates canvases if absent (editor-time fallback).

## Next Steps
- Implement runtime helper script and adjust `MoveCounter` search if needed.
- Provide profiling helper.
- Update documentation.

## Profiling Helper Idea (outline)
```
// Pseudocode
// Attach to DynamicHUD and StaticHUD
public class CanvasDirtyLogger : MonoBehaviour {
    Canvas _canvas;
    int _lastFrame; int _dirtyCount;
    void Awake(){ _canvas = GetComponent<Canvas>(); }
    void LateUpdate(){
        if (Time.frameCount != _lastFrame) { _dirtyCount = 0; _lastFrame = Time.frameCount; }
        // Hook into GraphicRegistry or use reflection if necessary (placeholder)
    }
}
```

---
Work in progress – will update as implementation proceeds.