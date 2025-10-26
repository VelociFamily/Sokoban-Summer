# AI contributor instructions for Sokoban Summer (Unity)

Short, actionable rules to get productive fast. Follow the existing service singletons and scene metadata; avoid re‑creating systems.

## Architecture (what runs and why)
- Entry: `Core/GameInitializer.cs` instantiates Background and MusicPlayer, initializes in parallel: Audio (`ModernAudioService` → `Audio/UnifiedAudioManager`), Input (`InputService`), Achievements, and `MoveCounter`; then loads "Main Menu" additively. If you rename the menu, update `LoadMainMenuAsync()`.
- Scene metadata: `Core/SceneInfo.cs` + `Core/SceneType.cs` replace build-index logic (also decides if background shows). Always add a `SceneInfo` to scenes.
- Dynamic levels: `Core/LevelManager.cs` scans Build Settings under `Assets/Scenes/Tutorials` and `Assets/Scenes/Levels`, derives ordering, and optionally enriches from `LevelData` ScriptableObjects (auto‑loaded via Resources name heuristics).
- Persistence and additive loading: `Core/MenuPersistence.cs` keeps menu/audio objects across scenes and removes duplicate `EventSystem`/`AudioListener` when scenes are loaded additively.
- UI counters: `Core/MoveCounter.cs` is a singleton that auto‑discovers TextMeshProUGUI for moves/timer using naming heuristics; stops timer when a Canvas with "complete" is active.

## Conventions and patterns
- Initialize via `InitializeAsync()` + `Task.Yield()`; keep `Awake/Start` light.
- Don’t construct `new InputSystem_Actions()`; use `InputService.Instance.InputActions` and its forwarded events (`OnPlayerMove`, `OnUICancel`).
- Don’t hardcode `buildIndex`; use `SceneInfo.GetActiveSceneType()` / `SceneInfo.ShouldShowBackground()`.
- Centralize audio through `ModernAudioService`/`UnifiedAudioManager`; don’t add stray `AudioSource`s.
- For Move/Timer UI, prefer exact names "Moves" and "Timer"; otherwise include terms: moves: move|moves|step; time: time|timer|clock; completion Canvas name contains "complete".
- Level select UI: use `UI/DynamicLevelSelector.cs` with a prefab that has `UI/DynamicLevelButton.cs`.

## Common APIs (copy/paste)
- Input: `var actions = Core.InputService.Instance.InputActions;` then `InputService.Instance.EnablePlayerInput();`
- Audio: `ModernAudioService.Instance.PlaySFX(clip);` `ModernAudioService.Instance.PlayMusic(clip, true);` `ModernAudioService.Instance.SetVolume(Audio.AudioChannelType.SFX, 0.5f);`
- Scene/levels: `var t = Core.SceneInfo.GetActiveSceneType(); var showBg = Core.SceneInfo.ShouldShowBackground();` `var levels = Core.LevelManager.Instance.GetLevels(Core.SceneType.GameplayLevel);`
- Moves/timer: `Core.MoveCounter.Instance.IncrementMove();` `Core.MoveCounter.Instance.ResetCounter();`

## Editor & workflows
- Play from the Game scene with `GameInitializer`. Inspector refs: `Background`, optional `UnifiedAudioManagerPrefab`, optional `MoveCounterPrefab`, `MusicPlayer`. Missing prefabs auto‑create sane defaults.
- Validation: `Core/InitializationValidator.cs` has context menu "Run Validation"; prints status for input/achievements/movecounter.
- Level UI: add `DynamicLevelSelector` to a container and assign a button prefab with `DynamicLevelButton`.
- Achievements UI: `AchievementManager` looks for a TextMeshProUGUI tagged `achievement`.
- Stats/progression: `LevelManager` uses `PlayerPrefs` for completion/best moves/time; call `SaveLevelStats`/`MarkLevelCompleted`.

## CI/build & repo
- CI: `.github/workflows/unity-ci.yml` (see `.github/workflows/README.md`) auto‑detects Unity from `ProjectSettings/ProjectVersion.txt` (e.g., 6000.2.8f1). Requires `UNITY_LICENSE` (or email/password/serial) secrets.
- Git LFS: extension‑based rules in `.gitattributes`; see root `README.md` for post‑move normalization.

## File map (start here)
- Init/services: `Core/GameInitializer.cs`, `Core/ModernAudioService.cs`, `Core/InputService.cs`, `Core/MoveCounter.cs`, `Core/MenuPersistence.cs`
- Audio: `Audio/UnifiedAudioManager.cs`, `Audio/AudioVolumeSettings.cs`, `Audio/VolumeSlider.cs`
- Scenes/levels: `Core/SceneInfo.cs`, `Core/SceneType.cs`, `Core/LevelManager.cs`, `Core/LevelData.cs`
- UI: `UI/DynamicLevelSelector.cs`, `UI/DynamicLevelButton.cs`, `UI/CompleteUI.cs`, `UI/MainMenuLevelSelector.cs`
- Tests/tools: `Core/InitializationValidator.cs`, `Testing/MoveCounterTimerTest.cs`

## Pitfalls
- Don’t duplicate input/audio singletons or add extra `EventSystem`/`AudioListener` in additively loaded scenes; `MenuPersistence` manages duplication.
- Don’t hardcode scene indices; do update `GameInitializer.LoadMainMenuAsync()` if the menu scene name changes.

Questions or gaps? If any part of the architecture isn’t clear (e.g., extending `LevelManager` or adding audio channels), ask and reference the specific target files you plan to modify.
