using Core;
using UnityEngine;

namespace UI
{
    public class LockHider : MonoBehaviour
    {
        void Update()
        {
            var achievementManager = SokobanSummer.Core.ServiceLocator.Get<AchievementManager>();
            if (achievementManager != null && achievementManager.CompleteTutorial)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
