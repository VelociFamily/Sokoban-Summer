using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    [Serializable]
    public class SaveMeta
    {
        public int schemaVersion = 1;
        public string appVersion = Application.version;
        public string lastWriteUtc;

        public static SaveMeta Create()
        {
            return new SaveMeta { lastWriteUtc = DateTime.UtcNow.ToString("o") };
        }
    }

    [Serializable]
    public class SettingsData
    {
        public SaveMeta meta = SaveMeta.Create();
        public float masterVolume = 1f;
        public float musicVolume = 1f;
        public float sfxVolume = 1f;
        // Extend with additional toggles
    }

    [Serializable]
    public class AchievementData
    {
        public SaveMeta meta = SaveMeta.Create();
        public bool confuseAndSpeed;
        public bool completeTutorial;
        public bool completeLevelTwo;
        public string selectedHatName = "";
    }

    [Serializable]
    public class LevelStat
    {
        public bool completed;
        public int bestMoves;
        public float bestTime;
    }

    [Serializable]
    public class ProgressData
    {
        public SaveMeta meta = SaveMeta.Create();
        // Keyed by buildIndex string for stability across JSON (avoids int-key JSON edge cases)
        public Dictionary<string, LevelStat> levelStats = new Dictionary<string, LevelStat>();
    }
}
