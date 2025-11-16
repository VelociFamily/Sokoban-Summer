## UI Optimization: Static vs Dynamic HUD (Issue #45)

Implemented separation of gameplay HUD into two canvases via `HudStructureInitializer`:
- `StaticHUD` (sortingOrder 100): Unchanging decorative / background HUD elements.
- `DynamicHUD` (sortingOrder 101): Frequently updated counters (moves, timer, achievements text if any).

### Components Added
- `HudStructureInitializer`: Ensures canvases exist (auto‑creates if missing) and disables Raycast Target on non‑interactive graphics.
- `CanvasRebuildProfiler`: Registers dirty callbacks (layout / vertices / material) to approximate canvas rebuild activity and periodically logs counts.

### Usage
1. Add a root GameObject (e.g., `GameplayHUD`) and attach `HudStructureInitializer` in gameplay scenes.
2. Place static UI elements under the generated `StaticHUD` canvas, dynamic counters under `DynamicHUD`.
3. (Optional) Add `CanvasRebuildProfiler` to both canvases; enable `autoLog` to observe activity in play mode.

### Expected Result
Only `DynamicHUD` canvas incurs dirty callbacks during move/timer updates; `StaticHUD` remains stable, reducing rebuild cost.

### Raycast Target Rules
All `Graphic` / `TMP_Text` elements without a `Selectable` parent are marked `raycastTarget = false` to reduce event system raycast work.

### Future Extensions
- Add a marker component (e.g., `ForceRaycastTarget`) if specific non‑interactive elements must remain raycastable (tooltips, custom hit areas).
- Aggregate profiler results in a central UI performance dashboard.

### Verification Checklist
- Move counter increments: only DynamicHUD profiler logs increase.
- Timer ticks (0.1s): StaticHUD profiler remains at zero.
- No unintended loss of button/toggle interactivity.

---
Implemented on branch `issue/45-split-gameplay-hud-static-dynamic`.