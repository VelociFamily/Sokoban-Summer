# Sokoban Summer — repository guidelines

This repository contains a Unity project edited with Visual Studio / .NET C# scripts. To keep the repository healthy and fast, follow these recommended Git best practices.

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

