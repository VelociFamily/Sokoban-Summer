using Core;
using UnityEngine;

namespace UI
{
    public class AchievementShower : MonoBehaviour
    {
        [Header("Assign the badge GameObjects for each achievement")]
        public GameObject confuseAndSpeedBadge;
        public GameObject completeTutorialBadge;
        public GameObject completeLevelTwoBadge;

        private AchievementManager _achievementManager;
        private bool _warnedMissing;
        private void OnEnable()
        {
            TrySubscribe();
        }

        private void OnDisable()
        {
            if (_achievementManager != null)
            {
                _achievementManager.AchievementsChanged -= RefreshBadges;
            }
        }

        private void TrySubscribe()
        {
            if (_achievementManager == null)
            {
                if (!ServiceLocator.TryGet<AchievementManager>(out _achievementManager))
                {
                    if (!_warnedMissing)
                    {
                        Debug.LogWarning("[AchievementShower]: AchievementManager not registered yet - will retry on next enable");
                        _warnedMissing = true;
                    }
                    return;
                }
            }
            _achievementManager.AchievementsChanged -= RefreshBadges; // avoid duplicate
            _achievementManager.AchievementsChanged += RefreshBadges;
            RefreshBadges(); // initial sync
        }

        private void RefreshBadges()
        {
            if (_achievementManager == null) return;

            if (confuseAndSpeedBadge != null)
            {
                var shouldShow = _achievementManager.ConfuseAndSpeed;
                if (confuseAndSpeedBadge.activeSelf != shouldShow)
                    confuseAndSpeedBadge.SetActive(shouldShow);
            }

            if (completeTutorialBadge != null)
            {
                var shouldShow = _achievementManager.CompleteTutorial;
                if (completeTutorialBadge.activeSelf != shouldShow)
                    completeTutorialBadge.SetActive(shouldShow);
            }

            if (completeLevelTwoBadge != null)
            {
                var shouldShow = _achievementManager.CompleteLevelTwo;
                if (completeLevelTwoBadge.activeSelf != shouldShow)
                    completeLevelTwoBadge.SetActive(shouldShow);
            }
        }
    }
}
