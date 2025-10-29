# Contributing

This repository uses Git LFS for large binary assets and has rewritten history to move certain files into LFS. If you contribute, follow these setup steps.

Setup (one-time per machine)
```
git config --global core.autocrlf true
git lfs install
```

After a history rewrite (or if you encounter strange diffs)
- The repository's history was rewritten to migrate large assets into Git LFS. The safest way to get the updated history is to re-clone:
```
git clone https://github.com/VelociFamily/Sokoban-Summer.git
```
- Alternatively, to reset an existing local copy (advanced):
```
git fetch origin
git reset --hard origin/main
git lfs pull
```

Committing large assets
- Ensure the file types below are tracked by LFS before adding large binary files (they should be configured in `.gitattributes`):
  - `*.png`, `*.jpg`, `*.jpeg`, `*.tga`, `*.psd`, `*.fbx`, `*.wav`, `*.mp3`, `*.ogg`, `*.mp4`, `*.mov`

If you need help migrating additional files into LFS or want CI configured differently, open an issue or contact the repository maintainers.

## Assembly definition (asmdef) guidelines

- We use Unity Assembly Definition files to enforce boundaries between Core, Gameplay, UI, Audio and other domains. When adding new scripts, place them under the correct domain folder and update or create an asmdef if needed.
- Keep the Core assembly free of references to higher-level feature assemblies (Audio, UI, Gameplay). Shared contracts (interfaces) that Core uses should live in `Assets/Scripts/CoreShared` and be exposed via `SokobanSummer.CoreShared` asmdef.
- The generated Input System class (`InputSystem_Actions.cs`) is compiled into `SokobanSummer.Input` (see `Assets/Settings/Input system things/Input.asmdef`). Assemblies that construct or reference `InputSystem_Actions` should reference `SokobanSummer.Input` rather than `Assembly-CSharp`.

## ModernAudioService convenience helpers

- `CoreShared.IAudioManager` intentionally uses `int` for channel identifiers to avoid a compile-time dependency on the Audio assembly. To make calling code ergonomic, the Audio assembly provides small extension helpers that accept the `Audio.AudioChannelType` enum and forward casts to the int-based API.
- Location: `Assets/Scripts/Audio/ModernAudioServiceExtensions.cs` (assembly `SokobanSummer.Audio`).
