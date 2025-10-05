using System;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// High-level entry point for accessing saves and coordinating one-time migrations.
    /// </summary>
    public class SaveFacade
    {
        private static SaveFacade _instance;
        public static SaveFacade Instance => _instance ??= new SaveFacade();

        public static event System.Action<string> OnReset; // "Settings", "Achievements", "Progress", "All"

        private const string SettingsKey = "settings";
        private const string AchievementsKey = "achievements";
        private const string ProgressKey = "progress";
        private const string MigrationFlag = "SaveMigratedToJsonV1";

    private bool _initialized;
    private SettingsData _settings;
    private AchievementData _achievements;
    private ProgressData _progress;

    public SettingsData Settings { get { EnsureInitialized(); return _settings; } }
    public AchievementData Achievements { get { EnsureInitialized(); return _achievements; } }
    public ProgressData Progress { get { EnsureInitialized(); return _progress; } }

        private SaveFacade() { }

        public void InitializeAndMaybeMigrate()
        {
            // Load or create
            _settings = SaveService.Instance.LoadOrCreate(SettingsKey, () => new SettingsData());
            _achievements = SaveService.Instance.LoadOrCreate(AchievementsKey, () => new AchievementData());
            _progress = SaveService.Instance.LoadOrCreate(ProgressKey, () => new ProgressData());

            // One-time migration from PlayerPrefs
            if (PlayerPrefs.GetInt(MigrationFlag, 0) == 0)
            {
                try
                {
                    MigrateFromPlayerPrefs();
                    PlayerPrefs.SetInt(MigrationFlag, 1);
                    PlayerPrefs.Save();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveFacade] Migration failed: {e}");
                }
            }
            _initialized = true;
        }

        private void EnsureInitialized()
        {
            if (_initialized) return;
            InitializeAndMaybeMigrate();
        }

        public void SaveAll()
        {
            EnsureInitialized();
            SaveService.Instance.Save(SettingsKey, _settings);
            SaveService.Instance.Save(AchievementsKey, _achievements);
            SaveService.Instance.Save(ProgressKey, _progress);
        }

        public void SaveSettings()
        {
            EnsureInitialized();
            SaveService.Instance.Save(SettingsKey, _settings);
        }

        public void SaveAchievements()
        {
            EnsureInitialized();
            SaveService.Instance.Save(AchievementsKey, _achievements);
        }

        public void SaveProgress()
        {
            EnsureInitialized();
            SaveService.Instance.Save(ProgressKey, _progress);
        }

        public void ResetSettings()
        {
            EnsureInitialized();
            SaveService.Instance.Delete(SettingsKey);
            _settings = new SettingsData();
            SaveSettings();
            OnReset?.Invoke("Settings");
        }

        public void ResetAchievements()
        {
            EnsureInitialized();
            SaveService.Instance.Delete(AchievementsKey);
            _achievements = new AchievementData();
            SaveAchievements();
            OnReset?.Invoke("Achievements");
        }

        public void ResetProgress()
        {
            EnsureInitialized();
            SaveService.Instance.Delete(ProgressKey);
            _progress = new ProgressData();
            SaveProgress();
            OnReset?.Invoke("Progress");
        }

        public void ResetAll()
        {
            ResetSettings();
            ResetAchievements();
            ResetProgress();
            PlayerPrefs.DeleteKey(MigrationFlag);
            PlayerPrefs.Save();
            OnReset?.Invoke("All");
        }

        private void MigrateFromPlayerPrefs()
        {
            Debug.Log("[SaveFacade] Migrating PlayerPrefs to JSON saves...");

            // Audio settings — use AudioVolumeSettings default keys if they exist
            _settings.masterVolume = PlayerPrefs.GetFloat("Volume_Master", _settings.masterVolume);
            _settings.musicVolume = PlayerPrefs.GetFloat("Volume_Music", _settings.musicVolume);
            _settings.sfxVolume = PlayerPrefs.GetFloat("Volume_SFX", _settings.sfxVolume);

            // Achievements
            _achievements.confuseAndSpeed = PlayerPrefs.GetInt("Achievement_ConfuseAndSpeed", _achievements.confuseAndSpeed ? 1 : 0) == 1;
            _achievements.completeTutorial = PlayerPrefs.GetInt("Achievement_CompleteTutorial", _achievements.completeTutorial ? 1 : 0) == 1;
            _achievements.completeLevelTwo = PlayerPrefs.GetInt("Achievement_CompleteLevelTwo", _achievements.completeLevelTwo ? 1 : 0) == 1;
            _achievements.selectedHatName = PlayerPrefs.GetString("SelectedHat", _achievements.selectedHatName ?? "");

            // Level progress: scan a reasonable range (or replace when we wire LevelManager fully)
            for (int i = 0; i < 1000; i++)
            {
                var completed = PlayerPrefs.GetInt($"Level_{i}_Completed", -1);
                var bestMoves = PlayerPrefs.GetInt($"Level_{i}_BestMoves", -1);
                var bestTime = PlayerPrefs.GetFloat($"Level_{i}_BestTime", -1f);

                if (completed == -1 && bestMoves == -1 && Math.Abs(bestTime + 1f) < 0.0001f)
                    continue; // nothing saved for this index

                var key = i.ToString();
                if (!_progress.levelStats.TryGetValue(key, out var stat))
                {
                    stat = new LevelStat();
                    _progress.levelStats[key] = stat;
                }

                stat.completed = completed == 1 || stat.completed;
                if (bestMoves >= 0)
                    stat.bestMoves = stat.bestMoves == 0 || (bestMoves > 0 && bestMoves < stat.bestMoves) ? bestMoves : stat.bestMoves;
                if (bestTime >= 0f)
                    stat.bestTime = stat.bestTime == 0f || (bestTime > 0 && bestTime < stat.bestTime) ? bestTime : stat.bestTime;
            }

            SaveAll();
            Debug.Log("[SaveFacade] Migration completed.");
        }
    }
}
