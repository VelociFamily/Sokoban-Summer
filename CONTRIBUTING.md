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
