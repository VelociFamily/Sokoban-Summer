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
- Access services via `SokobanSummer.Core.ServiceLocator.Get<T>()` instead of static Instance properties.
- Use `ServiceLocator.TryGet<T>(out var service)` for optional service access with null safety.
- Don't construct `new InputSystem_Actions()`; use `ServiceLocator.Get<InputService>().InputActions` and its forwarded events (`OnPlayerMove`, `OnUICancel`).
- Don't hardcode `buildIndex`; use `SceneInfo.GetActiveSceneType()` / `SceneInfo.ShouldShowBackground()`.
- Centralize audio through `ServiceLocator.Get<ModernAudioService>()`/`UnifiedAudioManager`; don't add stray `AudioSource`s.
- For Move/Timer UI, prefer exact names "Moves" and "Timer"; otherwise include terms: moves: move|moves|step; time: time|timer|clock; completion Canvas name contains "complete".
- Level select UI: use `UI/DynamicLevelSelector.cs` with a prefab that has `UI/DynamicLevelButton.cs`.

## Common APIs (copy/paste)
- Service access: `var inputService = SokobanSummer.Core.ServiceLocator.Get<InputService>();` then `inputService.EnablePlayerInput();`
- Audio: `SokobanSummer.Core.ServiceLocator.Get<ModernAudioService>().PlaySFX(clip);` `ServiceLocator.Get<ModernAudioService>().PlayMusic(clip, true);` `ServiceLocator.Get<ModernAudioService>().SetVolume(Audio.AudioChannelType.SFX, 0.5f);`
- Scene/levels: `var t = Core.SceneInfo.GetActiveSceneType(); var showBg = Core.SceneInfo.ShouldShowBackground();` `var levels = SokobanSummer.Core.ServiceLocator.Get<LevelManager>().GetLevels(Core.SceneType.GameplayLevel);`
- Moves/timer: `SokobanSummer.Core.ServiceLocator.Get<MoveCounter>().IncrementMove();` `ServiceLocator.Get<MoveCounter>().ResetCounter();`

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
- Init/services: `Core/GameInitializer.cs`, `Core/ServiceLocator.cs`, `Core/ModernAudioService.cs`, `Core/InputService.cs`, `Core/MoveCounter.cs`, `Core/MenuPersistence.cs`
- Audio: `Audio/UnifiedAudioManager.cs`, `Audio/AudioVolumeSettings.cs`, `Audio/VolumeSlider.cs`
- Scenes/levels: `Core/SceneInfo.cs`, `Core/SceneType.cs`, `Core/LevelManager.cs`, `Core/LevelData.cs`
- UI: `UI/DynamicLevelSelector.cs`, `UI/DynamicLevelButton.cs`, `UI/CompleteUI.cs`, `UI/MainMenuLevelSelector.cs`
- Tests/tools: `Core/InitializationValidator.cs`, `Testing/MoveCounterTimerTest.cs`

## Pitfalls
- Don't duplicate input/audio singletons or add extra `EventSystem`/`AudioListener` in additively loaded scenes; `MenuPersistence` manages duplication.
- Don't hardcode scene indices; do update `GameInitializer.LoadMainMenuAsync()` if the menu scene name changes.
- Don't use `.Instance` static properties; all services are accessed via `ServiceLocator.Get<T>()` or `ServiceLocator.TryGet<T>(out var service)` for null-safe optional access.

Questions or gaps? If any part of the architecture isn’t clear (e.g., extending `LevelManager` or adding audio channels), ask and reference the specific target files you plan to modify.

## Slash commands

### `/workon <issue-number>`
- Use when the user wants Copilot to start working on a numbered GitHub issue with minimal prompting (example: `/workon 32`).
- Parse the numeric argument and fetch the matching issue title/description via `gh issue view <number> --json title,body,url` (fallback: `gh issue view <number>` and parse plaintext). Confirm repository context if the command is run inside a fork or without `origin` remote.
- Derive a short, kebab-case slug from the issue title (drop stopwords, keep up to five words). Build the branch name `issue/<ISSUE_NUMBER>-<slug>`; if slug generation fails, use `issue/<ISSUE_NUMBER>`.
- Check for a clean working tree. If there are local changes, prompt the user to commit/stash or confirm continuing before switching branches.
- Create and checkout the branch from the default branch (`main` unless `git symbolic-ref refs/remotes/origin/HEAD` reports otherwise): `git fetch origin`, `git checkout -B issue/<ISSUE_NUMBER>-<slug> origin/<DEFAULT_BRANCH>`.
- Start a worklog message in chat summarizing the issue context (link, acceptance criteria, assumptions) and outline the first implementation steps you will take.
- If the repository uses GitHub CLI authentication, create a draft PR targeting the default branch (`gh pr create --draft`). Set the title to include both the issue number and title, and ensure the body contains the standard GitHub `Resolves` reference to that issue. Mention that work is in progress and list planned tasks as checkboxes.
- Report the new branch name, PR URL (if created), and immediate next actions back to the user. If any automation step fails (missing CLI, permissions, dirty tree), explain the blocker and request guidance.
