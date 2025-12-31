# Sokoban Summer

Sokoban Summer is a cozy, Unity-made puzzle game inspired by classic Sokoban mechanics: guide a character through handcrafted levels, push movable blocks to reach targets, and solve increasingly clever spatial puzzles. The project includes player controls (with optional teleport and confusion powerups), move and time tracking for speedrun-style challenges, multiple tutorial scenes, and clean UI feedback for moves and completion. Designed for desktop platforms, the game combines simple, tactile controls with thoughtful level design — perfect for players who enjoy deliberate, brain-teasing puzzles.

## Quick Links for Repository Admins
- **[How to delete protected branches](./documents/ADMIN_GUIDE.md#deleting-protected-branches)** - Admin bypass configuration
- **[Branch protection settings](./documents/ADMIN_GUIDE.md#branch-protection-and-admin-bypass)** - Step-by-step setup guide
- **[Troubleshooting admin permissions](./documents/ADMIN_GUIDE.md#troubleshooting)** - Common issues and solutions

# Repository Guidelines

Quick checklist

Why these files exist

## Documentation

Project documentation is organized into focused guides:

### Game Development Guides
- **[LEVEL_BUTTON_VISUAL_POLISH_GUIDE.md](./documents/LEVEL_BUTTON_VISUAL_POLISH_GUIDE.md)** - Complete guide for designing and implementing polished level selection UI, including grid/vertical layout decisions, font/background cohesion, and sprite import settings
- **[LEVEL_BUTTON_SETUP_GUIDE.md](./documents/LEVEL_BUTTON_SETUP_GUIDE.md)** - Technical setup instructions for level button prefabs with DynamicLevelButton component
- **[DYNAMIC_LEVEL_SYSTEM_GUIDE.md](./documents/DYNAMIC_LEVEL_SYSTEM_GUIDE.md)** - Architecture and implementation guide for the dynamic level management system
- **[PERSISTENCE_GUIDE.md](./documents/PERSISTENCE_GUIDE.md)** - How the game saves and loads player progress across sessions

### Repository Administration
- **[ADMIN_GUIDE.md](./documents/ADMIN_GUIDE.md)** - Repository administration guide covering branch protection, admin permissions, and branch deletion procedures
- **[CONTRIBUTING.md](./documents/CONTRIBUTING.md)** - Contribution guidelines for developers

Recommended Git LFS usage
1. Install Git LFS: `git lfs install`
2. Track asset types you want to store in LFS, for example:
```
git lfs track "*.png"
git lfs track "*.psd"
git lfs track "*.fbx"
git lfs track "*.wav"
git add .gitattributes
git commit -m "Track binary assets with Git LFS"
```
 PERSISTENCE_GUIDE.md
Migrating existing large files into LFS (use carefully)
- If you already have large files in history, migrate them with:
```
git lfs migrate import --include="*.psd,*.png,*.fbx,*.wav" --include-ref=refs/heads/main
```
- Read `git lfs migrate` docs before running; it rewrites history.

Windows line endings
- This repo enforces `crlf` for C# files in `.editorconfig`. On Windows, set `core.autocrlf` to `true` so developers get CRLF working copies.

Optional recommendations
- Add a `LICENSE` file if you want to open-source the project.
- Add `CODEOWNERS` to assign responsibilities for folders.
- Add CI to run Unity tests or build players (Unity Cloud Build, GitHub Actions, etc.).

Support
If you want, I can also:
- Run `git lfs track` commands and commit the results
- Create a small GitHub Actions workflow that runs a Unity check/build
- Help migrate large files into LFS (I will need confirmation before rewriting history)

Next steps (optional)

- Track binaries with Git LFS (example):
```
git lfs track "*.png" "*.psd" "*.fbx" "*.wav"
git add .gitattributes
git add .gitattributes
git commit -m "Track common binary assets with Git LFS"
```

- Migrate large files into LFS (rewrites history):
```
git lfs migrate import --include="*.psd,*.png,*.fbx,*.wav" --include-ref=refs/heads/main
```

- Add `CODEOWNERS` to the repository root to assign reviewers for paths.
- Add `LICENSE` if you plan to open-source the project.
- Consider adding CI (GitHub Actions) to run Unity lint/build checks. I can scaffold a minimal workflow.

If you want me to perform any of the above (e.g., run `git lfs track` and commit or scaffold CI), tell me which items to run and I'll perform them.

Actions performed by automation on 2025-09-18
- Added `.gitignore`, `.gitattributes`, and `.editorconfig` to repository root.
- Ran `git lfs install` and `git lfs track` for common Unity binary types and committed `.gitattributes`.
- Scaffolded a basic GitHub Actions workflow at `.github/workflows/unity-check.yml` (scaffold; no Unity build configured).
- Added `.github/CODEOWNERS` and a `LICENSE` (MIT) file.

Migration performed
- Ran: `git lfs migrate import --include="Assets/Sounds/**/*.mp3" --include-ref=refs/heads/main` to move `Assets/Sounds/*.mp3` into LFS (history rewritten on `main`).
- After migration, LFS tracking patterns include `Assets/Sounds/**/*.mp3` and common binary patterns. Contributors will need to re-clone or follow Git LFS migration instructions.

Push and verification
- Pushed rewritten `main` to `origin` and pushed LFS objects.
- To verify locally:
```
git lfs ls-files -l
git rev-list --objects --all | Select-String -Pattern "\.mp3"
```

CI / Unity build notes
- **Enhanced Unity CI workflow** at `.github/workflows/unity-ci.yml` with self-hosted runner support:
  - **Automatically tries self-hosted Unity runners first** for faster builds
  - **Falls back to Windows runners** with approval when self-hosted unavailable
  - **Manual controls** to force Windows runner or retry self-hosted
  - See `.github/workflows/README.md` for detailed setup and usage
- You must set repository secrets:
	- `UNITY_EMAIL`, `UNITY_PASSWORD`, and `UNITY_LICENSE` (or use `UNITY_SERIAL`).
- Unity version is auto-detected from `ProjectSettings/ProjectVersion.txt` (currently 6000.2.8f1)

Final scan (files > 1 MB in `Assets/`)
- `Assets/Sounds/Music songs/gaming-game-minecraft-background-music-372242.mp3` — 6.4 MB
- `Assets/Sounds/Music songs/Piece by Piece.mp3` — 5.2 MB
- `Assets/Textures/Sci Fi/colony (1)-1.png.png` — 4.77 MB
- `Assets/Settings/Lit2DSceneTemplate.scenetemplate` — 3.76 MB
- `Assets/Others/TextMesh Pro/.../LiberationSans SDF.asset` — 2.15 MB
- `Assets/Others/TextMesh Pro/.../Unity SDF.asset` — 2.01 MB
- a handful of `.png`, `.prefab`, and `.unity` files between 1–1.6 MB

CI update: I updated `.github/workflows/unity-ci.yml` to use the detected editor version from `ProjectSettings/ProjectVersion.txt` and a matrix of `StandaloneWindows64` and `StandaloneLinux64`. The workflow expects a `UNITY_LICENSE` secret containing your license file content (or use GameCI's preferred license setup). If you'd like, I can change the workflow to use email/password/serial activation or a different publishing target.

Unity CI git fix (2025-09-19)
- Fixed "git rev-parse --is-shallow-repository" error by adding `fetch-depth: 0` to checkout action
- This provides full git history required for semantic versioning in game-ci/unity-builder
- Alternative solution: use `versioning: Custom` with manual version number to avoid git history requirements

## Post-move Git LFS fix (2025-09-19)

What happened
- The Unity project folder was moved out of a nested "Sokoban Summer/" directory to the repository root.
- `.gitattributes` contained path-anchored LFS patterns like `Sokoban[[:space:]]Summer/**/*.png`, which stopped matching after the move.
- As a result, a large number of binary assets appeared as regular file changes or as deleted/added under the old path in `git status`.

What we changed
- Simplified `.gitattributes` to use extension-based rules (e.g., `*.png`, `*.fbx`, `*.wav`, `*.mp3`, etc.) so tracking is path-agnostic.
- Kept an optional explicit rule for `Assets/Sounds/**/*.mp3` for redundancy.
- Verified Unity `.gitignore` is present at repo root to ignore `Library/`, `Temp/`, `Logs/`, `obj/`, etc.

How to re-stage correctly
1) Ensure LFS is installed/enabled
	 - git lfs install
2) Reapply attributes to the index so moved files match the new rules
	 - git add --renormalize .
3) Review LFS status
	 - git lfs status
4) Stage and commit
	 - git add .gitattributes
	 - git commit -m "fix: normalize LFS attributes after moving project to repo root"

Optional: if any large binaries were accidentally committed as normal (not LFS)
- Convert them into LFS without rewriting history for other files:
	- git lfs migrate import --include="*.png,*.jpg,*.jpeg,*.tga,*.psd,*.fbx,*.wav,*.mp3,*.ogg,*.mp4,*.mov" --include-ref=refs/heads/main

Verification
- List files tracked by LFS:
	- git lfs ls-files -l
- Check current LFS tracking patterns:
	- git lfs track
- Confirm working tree is clean:
	- git status

Prevention tips
- Prefer extension-based LFS rules to avoid path coupling.
- Keep `.gitattributes` and `.gitignore` at the repository root.
- When moving folders, run `git add --renormalize .` to re-evaluate attributes.


