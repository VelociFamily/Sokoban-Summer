# Prefab & Asset Naming and Placement Guidelines

This document defines a lightweight, low-risk convention for prefab and asset naming and placement for the Sokoban Summer project. Follow these rules to reduce confusion, prevent duplicate runtime singletons, and make future automated moves safer.

## Contract (inputs / outputs / success)
- Inputs: Unity assets under `Assets/` (prefabs, scenes, scripts). Prefab `.meta` files will be preserved when moving files.
- Output: consistent kebab-case filenames, system-scoped prefab folders, and a small changelog for each move.
- Success criteria: guideline file added; subsequent moves keep prefab `.meta` files paired and scenes open without duplicates.

## Naming rules
- Use kebab-case for filenames: all-lowercase words separated by hyphens. Example: `background-canvas.prefab`, `audio-manager.prefab`.
- No spaces, no leading/trailing underscores or dots. Use descriptive names with the system prefix when helpful: `<system>-<role>`, e.g., `ui-mainmenu-panel.prefab`.
- Keep Unity component names and class names in PascalCase in scripts only; filenames are kebab-case for assets.

## Folder placement
- Place prefabs adjacent to their system code where feasible.
  - Example: `Assets/Prefabs/Audio/`, `Assets/Prefabs/Background/`, `Assets/Prefabs/UI/`.
- For shared/global singletons, use `Assets/Prefabs/Global/` and document instantiation responsibility (scene or runtime).

## Safe move procedure (manual or scripted)
1. Preserve `.meta` files — always move the prefab file and its `.meta` together to preserve GUIDs.
2. Prefer `git mv` for tracked files. For safety in Unity, perform moves while Unity is closed or use Unity's AssetDatabase (ideally in-editor) to relocate assets.
3. After moving, open the relevant scenes in Unity to verify references resolve and there are no missing references.
4. If a prefab is both present in-scene and instantiated by a runtime singleton, coordinate with the owning service to avoid duplicates (remove scene instance OR disable runtime instantiation during validation).
5. Document each moved prefab in the PR description with original path -> new path and a short validation note (which scenes were opened and which checks passed).

## Checklist for each prefab move (include in PR)
- [ ] Moved files include both `.prefab` and corresponding `.meta`.
- [ ] `git mv` was used for tracked files (or Unity AssetDatabase if moved in-editor).
- [ ] Scenes referencing the prefab were opened and verified (list scene names).
- [ ] No duplicate runtime singletons were introduced (list checks performed).
- [ ] PR body lists moved prefabs and validation steps.

## Examples
- Bad: `Assets/Prefabs/Background Canvas.prefab`
- Good: `Assets/Prefabs/Background/background-canvas.prefab`

## Next steps (recommended incremental plan)
1. Add this guideline file (low risk) — done in this PR.
2. Run a small scan for obvious offenders (files with spaces, uppercase words, or prefabs in root `Assets/Prefabs/`) and move 1-3 prefabs as a pilot.
3. After pilot PR is verified by opening affected scenes in Unity, continue with batched moves in subsequent PRs.

## Notes
- When moving many assets, split into small PRs by system to make review and manual validation tractable.
- If you need an in-editor helper script to automate safe moves and update scenes, propose it in a follow-up issue/PR.

---
Generated as the first step for issue #33: Normalize prefab and asset naming and placement.
