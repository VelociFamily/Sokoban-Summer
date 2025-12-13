# Persistence Guide

This project uses a lightweight, file-based SaveService that serializes small data models to JSON under:

- Windows: %USERPROFILE%/AppData/LocalLow/<Company>/<Product>/Saves
- macOS: ~/Library/Application Support/<Company>/<Product>/Saves
- Linux: ~/.config/unity3d/<Company>/<Product>/Saves

Files:
- settings.json — audio volumes and future toggles
- achievements.json — unlocked achievements and selected hat
- progress.json — per-level completion, best moves/time

## Design
- Atomic writes: saves use write-to-temp + backup + replace to avoid corruption.
- Backup: a .bak is created/updated before replacing the main file; it is auto-read if the main file is missing/corrupt.
- Versioning: each file has a meta section with schemaVersion, appVersion, and lastWriteUtc for future migrations.

## API overview
- SaveService: low-level JSON IO (Save<T>, TryLoad<T>, LoadOrCreate<T>) using keys: "settings", "achievements", "progress".
- SaveFacade: high-level singleton with typed properties and one-time PlayerPrefs migration.

Usage:
- Initialize early (GameInitializer does this):
  - SaveFacade.Instance.InitializeAndMaybeMigrate();
- Read/write:
  - var vol = SaveFacade.Instance.Settings.musicVolume;
  - SaveFacade.Instance.Progress.levelStats[buildIndex.ToString()].bestMoves = 12;
  - SaveFacade.Instance.SaveAll();

## PlayerPrefs migration
On first run after this system lands, SaveFacade migrates selected PlayerPrefs keys:
- Volume_Master, Volume_Music, Volume_SFX
- Achievement_ConfuseAndSpeed, Achievement_CompleteTutorial, Achievement_CompleteLevelTwo, SelectedHat
- Level_<index>_Completed, Level_<index>_BestMoves, Level_<index>_BestTime (indices 0..999)
A flag SaveMigratedToJsonV1 guards this so it only runs once.

## Steam Cloud Save (future)
Two options:
1) Auto-Cloud: configure Steam to sync the Saves folder. No code changes needed.
2) ISteamRemoteStorage: implement a SteamRemoteDataStore that mirrors SaveService (Save/Load) and plug it behind SaveFacade.

Keep the number of files small (3) and include meta timestamps; this simplifies conflict resolution.

## Troubleshooting
- No files? Ensure Game scene uses GameInitializer and runs InitializeCoreSystemsAsync early.
- Corrupt file? The .bak should auto-recover. You can delete the json to reset.
- Custom settings: extend SettingsData/AchievementData/ProgressData and serialize as usual.
