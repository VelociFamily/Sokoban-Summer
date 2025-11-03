# Issue #33 — Prefab/Asset Naming & Placement: Progress and How to Continue

This document tracks what’s been completed for issue #33 and provides a precise, copy-pasteable playbook to continue safely.

PR: https://github.com/VelociFamily/Sokoban-Summer/pull/66  
Branch: `issue/33-normalize-prefab-asset-naming-placement`

## What’s done so far

- Conventions
  - Added `GUIDELINES/PREFAB_NAMING.md` with kebab-case naming and folder layout under `Assets/Prefabs/...`.
- Validation tooling (Editor-only)
  - `Assets/Editor/Validation/PrefabValidation.cs` — scans key scenes/prefabs, flags missing scripts/refs and duplicate singletons.
  - `Assets/Editor/Validation/BatchPrefabValidationLauncher.cs` — enables `-runPrefabValidation` in batch mode.
  - `Assets/Editor/Validation/PrefabAutoFixer.cs` — dry-run/apply relinker (safe-only heuristics). Dry-run currently finds no safe suggestions.
- Stability tweaks for batch mode
  - `UI/DynamicLevelSelector.cs` now skips layout modifications when Unity runs with `-batchmode`, preventing OnValidate-time exceptions during automation.
- Pilot moves (GUIDs preserved by moving .meta alongside)
  - Audio/UI/background pilots in earlier commits.
  - Additional moves (examples):
    - `Assets/_Project/SokobanSummer/Prefabs/Background.prefab` -> `Assets/Prefabs/background/background.prefab`
    - `Assets/_Project/SokobanSummer/Prefabs/LeafForegroundEffect*.prefab` -> `Assets/Prefabs/effects/leaf-foreground-effect*.prefab`
    - `Assets/_Project/SokobanSummer/Prefabs/WindForegroundEffect.prefab` -> `Assets/Prefabs/effects/wind-foreground-effect.prefab`
    - `Assets/_Project/SokobanSummer/Prefabs/canvases/*` -> `Assets/Prefabs/ui/*` (level-complete, game-stats, pause-components)
    - `Assets/_Project/SokobanSummer/Prefabs/interactive/*` -> `Assets/Prefabs/interactive/*` (button, pf-player, teleport-pickup, tx-props-pot-b)
    - `Assets/_Project/SokobanSummer/Prefabs/walls/*` -> `Assets/Prefabs/walls/*` (obstacle, player-pen)
    - `Assets/_Project/SokobanSummer/Prefabs/Level Logger.prefab` -> `Assets/Prefabs/debug/level-logger.prefab`
- Validation status
  - Unity batch validator reports SUCCESS (no missing refs/duplicate singletons) after the latest fixes/moves.
  - Logs: `validation-unity.log` in repo root (created by batch runs).

## What to avoid (exclusions)

- Don’t move or duplicate engine/service singletons without care:
  - `Assets/_Project/SokobanSummer/Prefabs/UnifiedAudioManagerPrefab.prefab` — keep but do not duplicate. Moving is fine if .meta moves with it; prefer to defer until the end.
- Don’t move generated/timestamped achievement badge prefabs under `…/Prefabs/Achievement badges/` — these look like experimental assets and can be left as-is or archived later.

## Folder targets and naming

- Use these targets under `Assets/Prefabs/`:
  - `audio/`, `background/`, `effects/`, `ui/`, `interactive/`, `walls/`, `levels/`, `system/`, `debug/`, `achievement/` (as needed)
- Use kebab-case for filenames: `level-button.prefab`, `audio-manager.prefab`, etc.
- Always move the `.meta` file with the prefab to preserve GUIDs (prevents broken references).

## How to run validation (batch mode)

In PowerShell from repo root (paths assume Unity 6000.2.9f1; adjust if needed):

```pwsh
& 'C:\\Program Files\\Unity\\Hub\\Editor\\6000.2.9f1\\Editor\\Unity.exe' `
  -batchmode `
  -projectPath "$PWD" `
  -runPrefabValidation `
  -logFile 'validation-unity.log' `
  -quit
```

- Success criteria: log ends with a success line from PrefabValidation; no missing scripts/references reported.
- Auto-fixer dry-run (optional):

```pwsh
& 'C:\\Program Files\\Unity\\Hub\\Editor\\6000.2.9f1\\Editor\\Unity.exe' `
  -batchmode -projectPath "$PWD" `
  -executeMethod PrefabAutoFixer.RunAutoFixerDryRun `
  -logFile 'validation-autofix.log' -quit
```

## How to continue (repeatable small-batch loop)

1) Create target folders if missing (idempotent):
   - `Assets/Prefabs/{audio,background,effects,ui,interactive,walls,levels,system,debug,achievement}`
2) Move 3–10 low-risk prefabs at a time using `git mv`, always including the `.meta` twin.
   - Example mapping decisions:
     - Scene info/system: `GameSceneInfo.prefab`, `LevelSceneInfo.prefab`, `TutorialSceneInfo.prefab` -> `Assets/Prefabs/system/...`
     - Level template/control: `LevelTemplateManager.prefab` -> `Assets/Prefabs/levels/level-template-manager.prefab`
     - Effects: any `*Effect*.prefab` -> `Assets/Prefabs/effects/...`
     - UI canvases -> `Assets/Prefabs/ui/...`
3) Validate in batch mode (see command above). Ensure success before committing.
4) Commit and push:

```pwsh
git add -A
git commit -m "chore(prefabs): move <batch summary> (issue #33)"
git push origin issue/33-normalize-prefab-asset-naming-placement
```

5) Repeat until source folder `Assets/_Project/SokobanSummer/Prefabs/` is clean (minus exclusions).

## Ready-to-move candidates (next up)

- System/levels (safe; widely referenced but GUID-stable when .meta moves):
  - `GameSceneInfo.prefab`, `LevelSceneInfo.prefab`, `TutorialSceneInfo.prefab` -> `Assets/Prefabs/system/` (kebab-case names)
  - `LevelTemplateManager.prefab` -> `Assets/Prefabs/levels/level-template-manager.prefab`
- Effects (remaining): already moved leaf/wind; scan for any other `*Effect*.prefab` and group under `effects/`.

## Rollback strategy

- If validation fails, use `git restore --staged . && git restore .` to undo uncommitted moves, or `git revert <commit>` for a bad commit.
- Because we preserve `.meta` GUIDs with `git mv`, reference breakage is unlikely; issues typically stem from scripts or OnValidate behaviors.

## Notes

- Menu/background/audio singletons are managed by `Core/GameInitializer.cs`, `Core/MenuPersistence.cs`, and audio services. Avoid introducing duplicates across additively loaded scenes.
- Use `PrefabValidation` before and after moves to maintain confidence. Keep changes in small batches to isolate any regressions.
