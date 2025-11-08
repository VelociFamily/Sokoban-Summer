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

        private void Update()
        {
            var achievementManager = SokobanSummer.Core.ServiceLocator.Get<AchievementManager>();
            if (achievementManager == null)
            {
                Debug.LogWarning("[AchievementShower]: AchievementManager instance not found - badges will not update");
                return;
            }

            // Update each badge's active state based on AchievementManager
            if (confuseAndSpeedBadge != null)
            {
                var shouldShow = achievementManager.ConfuseAndSpeed;
                if (confuseAndSpeedBadge.activeSelf != shouldShow)
                    confuseAndSpeedBadge.SetActive(shouldShow);
            }

            if (completeTutorialBadge != null)
            {
                var shouldShow = achievementManager.CompleteTutorial;
                if (completeTutorialBadge.activeSelf != shouldShow)
                    completeTutorialBadge.SetActive(shouldShow);
            }

            if (completeLevelTwoBadge != null)
            {
                var shouldShow = achievementManager.CompleteLevelTwo;
                if (completeLevelTwoBadge.activeSelf != shouldShow)
                    completeLevelTwoBadge.SetActive(shouldShow);
            }
        }
    }
}