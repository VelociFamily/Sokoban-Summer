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

        [Header("Update Settings")]
        [Tooltip("How often to check for achievement updates (in seconds). Set to 0 to check every frame.")]
        public float updateInterval = 1.0f;

        private bool confuseAndSpeedLastState;
        private bool completeTutorialLastState;
        private bool completeLevelTwoLastState;
        private float lastUpdateTime;

        private void Start()
        {
            // Initial update
            UpdateBadges(true);
            lastUpdateTime = Time.time;
        }

        private void Update()
        {
            // Only update at specified intervals to reduce performance impact
            if (updateInterval <= 0 || Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateBadges();
                lastUpdateTime = Time.time;
            }
        }

        private void UpdateBadges(bool forceUpdate = false)
        {
            if (AchievementManager.Instance == null)
            {
                if (forceUpdate) // Only warn on forced updates to avoid spam
                    Debug.LogWarning("[AchievementShower]: AchievementManager instance not found - badges will not update");
                return;
            }

            // Update confuseAndSpeedBadge only if state changed
            if (confuseAndSpeedBadge != null)
            {
                var shouldShow = AchievementManager.Instance.ConfuseAndSpeed;
                if (forceUpdate || shouldShow != confuseAndSpeedLastState)
                {
                    confuseAndSpeedBadge.SetActive(shouldShow);
                    confuseAndSpeedLastState = shouldShow;
                }
            }

            // Update completeTutorialBadge only if state changed
            if (completeTutorialBadge != null)
            {
                var shouldShow = AchievementManager.Instance.CompleteTutorial;
                if (forceUpdate || shouldShow != completeTutorialLastState)
                {
                    completeTutorialBadge.SetActive(shouldShow);
                    completeTutorialLastState = shouldShow;
                }
            }

            // Update completeLevelTwoBadge only if state changed
            if (completeLevelTwoBadge != null)
            {
                var shouldShow = AchievementManager.Instance.CompleteLevelTwo;
                if (forceUpdate || shouldShow != completeLevelTwoLastState)
                {
                    completeLevelTwoBadge.SetActive(shouldShow);
                    completeLevelTwoLastState = shouldShow;
                }
            }
        }

        /// <summary>
        /// Force an immediate update of all badge states
        /// </summary>
        public void ForceUpdateBadges()
        {
            UpdateBadges(true);
        }

        private void OnEnable()
        {
            // Update when component is enabled
            UpdateBadges(true);
        }
    }
}