# Sokoban Summer

Sokoban Summer is a cozy, Unity-made puzzle game inspired by classic Sokoban mechanics: guide a character through handcrafted levels, push movable blocks to reach targets, and solve increasingly clever spatial puzzles. The project includes player controls (with optional teleport and confusion powerups), move and time tracking for speedrun-style challenges, multiple tutorial scenes, and clean UI feedback for moves and completion. Designed for desktop platforms, the game combines simple, tactile controls with thoughtful level design — perfect for players who enjoy deliberate, brain-teasing puzzles.

# Repository Guidelines

Quick checklist
- Install Git LFS: `git lfs install`
- Track large binary asset types: `git lfs track "*.psd" "*.png" "*.fbx" "*.wav"` (see below)
- Ensure `core.autocrlf` is set appropriately on Windows: `git config --global core.autocrlf true`
- Commit `.gitattributes`, `.gitignore`, and `.editorconfig` before adding large assets

Why these files exist
- `.gitignore`: excludes Unity build artifacts, `Library/`, `Temp/`, Visual Studio caches, and other generated files.
- `.gitattributes`: normalizes line endings for text files and marks common binary asset types (recommendation for Git LFS).
- `.editorconfig`: enforces consistent C# formatting and encoding across editors.

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
- I scaffolded a full Unity CI workflow at `.github/workflows/unity-ci.yml` using `game-ci/unity-builder`.
- You must set repository secrets:
	- `UNITY_EMAIL`, `UNITY_PASSWORD`, and `UNITY_SERIAL` (or use a different licensing approach).
- Edit `unityVersion` in the workflow to match the project's Unity Editor version before relying on it.

Final scan (files > 1 MB in `Assets/`)
- `Assets/Sounds/Music songs/gaming-game-minecraft-background-music-372242.mp3` — 6.4 MB
- `Assets/Sounds/Music songs/Piece by Piece.mp3` — 5.2 MB
- `Assets/Textures/Sci Fi/colony (1)-1.png.png` — 4.77 MB
- `Assets/Settings/Lit2DSceneTemplate.scenetemplate` — 3.76 MB
- `Assets/Others/TextMesh Pro/.../LiberationSans SDF.asset` — 2.15 MB
- `Assets/Others/TextMesh Pro/.../Unity SDF.asset` — 2.01 MB
- a handful of `.png`, `.prefab`, and `.unity` files between 1–1.6 MB

CI update: I updated `.github/workflows/unity-ci.yml` to use the detected editor version from `ProjectSettings/ProjectVersion.txt` and a matrix of `StandaloneWindows64` and `StandaloneLinux64`. The workflow expects a `UNITY_LICENSE` secret containing your license file content (or use GameCI's preferred license setup). If you'd like, I can change the workflow to use email/password/serial activation or a different publishing target.


