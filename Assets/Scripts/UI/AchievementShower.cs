using UnityEngine;

public class AchievementShower : MonoBehaviour
{
    [Header("Assign the badge GameObjects for each achievement")]
    public GameObject confuseAndSpeedBadge;
    public GameObject completeTutorialBadge;
    public GameObject completeLevelTwoBadge;

    private void Update()
    {
        if (AchievementManager.Instance == null)
        {
            Debug.LogWarning("[AchievementShower]: AchievementManager instance not found - badges will not update");
            return;
        }

        // Update each badge's active state based on AchievementManager
        if (confuseAndSpeedBadge != null)
        {
            var shouldShow = AchievementManager.Instance.ConfuseAndSpeed;
            if (confuseAndSpeedBadge.activeSelf != shouldShow)
                confuseAndSpeedBadge.SetActive(shouldShow);
        }

        if (completeTutorialBadge != null)
        {
            var shouldShow = AchievementManager.Instance.CompleteTutorial;
            if (completeTutorialBadge.activeSelf != shouldShow)
                completeTutorialBadge.SetActive(shouldShow);
        }

        if (completeLevelTwoBadge != null)
        {
            var shouldShow = AchievementManager.Instance.CompleteLevelTwo;
            if (completeLevelTwoBadge.activeSelf != shouldShow)
                completeLevelTwoBadge.SetActive(shouldShow);
        }
    }
}