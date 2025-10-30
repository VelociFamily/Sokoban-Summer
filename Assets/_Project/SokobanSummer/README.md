# Assets/_Project/SokobanSummer

This folder is the home for project-owned assets for Sokoban Summer.

Purpose
- Keep custom scripts, prefabs, scenes, materials, and UI that are authored/modified by the team.

Structure guidance
- Scripts/ - C# scripts and related editor code owned by the project
- Prefabs/ - project prefabs used by levels and UI
- Scenes/ - project scenes (Game, Menu, Levels)
- Materials/ - project materials and shaders
- UI/ - UI prefabs and canvas assets

When adding or moving assets here, preserve meta files to keep GUIDs intact. If any references break, use Unity's prefab and scene re-linking tools and run the play-mode smoke test.

See issue: https://github.com/VelociFamily/Sokoban-Summer/issues/32
