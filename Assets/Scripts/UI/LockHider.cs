using Core;
using UnityEngine;

namespace UI
{
    public class LockHider : MonoBehaviour
    {
        void Update()
        {
            if (AchievementManager.Instance != null && AchievementManager.Instance.CompleteTutorial)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
