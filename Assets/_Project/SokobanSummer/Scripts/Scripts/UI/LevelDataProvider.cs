using System.Collections.Generic;
using UnityEngine;
using Core;

namespace UI
{
    /// <summary>
    /// Handles level data retrieval, filtering, and ordering for level selection UI.
    /// Single responsibility: Data management only.
    /// </summary>
    public class LevelDataProvider
    {
        private readonly List<LevelManager.LevelInfo> orderedLevelSequence = new List<LevelManager.LevelInfo>();
        private readonly Dictionary<LevelManager.LevelInfo, DynamicLevelButton> levelInfoToButton = new Dictionary<LevelManager.LevelInfo, DynamicLevelButton>();

        public IReadOnlyList<LevelManager.LevelInfo> OrderedLevels => orderedLevelSequence;
        public IReadOnlyDictionary<LevelManager.LevelInfo, DynamicLevelButton> LevelToButtonMap => levelInfoToButton;

        /// <summary>
        /// Gather levels based on filter settings
        /// </summary>
        public void GatherLevels(bool showTutorials, bool showGameplayLevels)
        {
            orderedLevelSequence.Clear();

            if (!ServiceLocator.TryGet<LevelManager>(out var levelManager))
            {
                levelManager = Object.FindFirstObjectByType<LevelManager>();
            }

            if (levelManager == null)
            {
                Debug.LogWarning("[LevelDataProvider] LevelManager not available in ServiceLocator or Scene");
                return;
            }

            if (showTutorials)
            {
                var tutorials = levelManager.GetLevels(SceneType.TutorialLevel);
                if (tutorials != null && tutorials.Count > 0)
                {
                    orderedLevelSequence.AddRange(tutorials);
                }
            }

            if (showGameplayLevels)
            {
                var levels = levelManager.GetLevels(SceneType.GameplayLevel);
                if (levels != null && levels.Count > 0)
                {
                    orderedLevelSequence.AddRange(levels);
                }
            }
        }

        /// <summary>
        /// Find the index of the last unlocked level (or first level as fallback)
        /// </summary>
        public int GetDefaultSelectionIndex()
        {
            if (!ServiceLocator.TryGet<LevelManager>(out var levelManager))
            {
                levelManager = Object.FindFirstObjectByType<LevelManager>();
            }

            if (orderedLevelSequence.Count == 0 || levelManager == null)
                return -1;

            int lastUnlockedIndex = -1;
            for (int i = 0; i < orderedLevelSequence.Count; i++)
            {
                var info = orderedLevelSequence[i];
                if (info != null && levelManager.CanLoadLevel(info))
                {
                    lastUnlockedIndex = i;
                }
            }

            if (lastUnlockedIndex >= 0)
                return lastUnlockedIndex;

            return orderedLevelSequence.Count > 0 ? 0 : -1;
        }

        /// <summary>
        /// Register a button for a specific level info
        /// </summary>
        public void RegisterButton(LevelManager.LevelInfo levelInfo, DynamicLevelButton button)
        {
            if (levelInfo == null || button == null)
                return;

            levelInfoToButton[levelInfo] = button;
        }

        /// <summary>
        /// Unregister a button
        /// </summary>
        public void UnregisterButton(LevelManager.LevelInfo levelInfo)
        {
            if (levelInfo == null)
                return;

            levelInfoToButton.Remove(levelInfo);
        }

        /// <summary>
        /// Clear all registered buttons
        /// </summary>
        public void ClearButtonRegistrations()
        {
            levelInfoToButton.Clear();
        }

        /// <summary>
        /// Get level info at specific index
        /// </summary>
        public LevelManager.LevelInfo GetLevelAt(int index)
        {
            if (index < 0 || index >= orderedLevelSequence.Count)
                return null;

            return orderedLevelSequence[index];
        }

        /// <summary>
        /// Get total level count
        /// </summary>
        public int GetLevelCount()
        {
            return orderedLevelSequence.Count;
        }
    }
}
