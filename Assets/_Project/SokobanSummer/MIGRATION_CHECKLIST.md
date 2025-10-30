# Migration checklist — Issue #32

This checklist documents the planned moves, verification steps, and rollback instructions for migrating assets into the new root folders:

- Targets:
  - Project-owned: `Assets/_Project/SokobanSummer`
  - Vendor/store drops: `Assets/ThirdParty`

1) Vendor assets (safe first step)
   - Move `Assets/Cainos/` -> `Assets/ThirdParty/Cainos/`
   - Move `Assets/JMO Assets/` -> `Assets/ThirdParty/JMO Assets/`
   - Move `Assets/visual effects/` -> `Assets/ThirdParty/visual effects/`

2) Project-owned assets (done in small batches)
   - Move `Assets/Scripts/` -> `Assets/_Project/SokobanSummer/Scripts/`
   - Move `Assets/Prefabs/` -> `Assets/_Project/SokobanSummer/Prefabs/`
   - Move `Assets/Scenes/` -> `Assets/_Project/SokobanSummer/Scenes/`
   - Move `Assets/Animations/` -> `Assets/_Project/SokobanSummer/Animations/`
   - Move `Assets/Presets/` -> `Assets/_Project/SokobanSummer/Presets/`
   - Move `Assets/Data/` -> `Assets/_Project/SokobanSummer/Data/`
   - Move `Assets/Settings/` -> `Assets/_Project/SokobanSummer/Settings/`
   - Move `Assets/Sounds/` -> `Assets/_Project/SokobanSummer/Sounds/`
   - Move `Assets/Sprites/` -> `Assets/_Project/SokobanSummer/Sprites/`
   - Move `Assets/Textures/` -> `Assets/_Project/SokobanSummer/Textures/`
   - Move `Assets/Editor/` -> `Assets/_Project/SokobanSummer/Editor/` (editor scripts owned by the project)

Verification steps after each commit:
- Open the project in Unity and let it reimport (or use a CI Unity runner).
- Run a play-mode smoke test: open `Main Menu` and one `Gameplay` scene; ensure no missing scripts or broken prefabs.
- Search the console for missing references or errors introduced by moved assets.

Rollback:
- Every move is committed in its own commit. If problems occur, revert the offending commit with `git revert <commit>` and push.

Notes and assumptions
- Assumption: `Cainos`, `JMO Assets`, and `visual effects` are vendor imports and should live under `Assets/ThirdParty`.
- Assumption: folders listed under project-owned are mostly authored by the team. If you prefer a different subset, tell me which folders to exclude or leave at root.
- Preserve `.meta` files when moving to keep GUIDs stable.

Next steps (recommended):
- Move vendor folders (done first) and run verification.
- Move project folders in small batches (Scenes & Scripts together, then Prefabs/Materials, then Art).
- Triage and fix any broken references after each batch.
