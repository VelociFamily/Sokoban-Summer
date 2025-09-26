using Core;
using UnityEngine;

namespace UI
{
    public class LockHider : MonoBehaviour
    {
        private bool hasCheckedTutorial = false;

        void Start()
        {
            // Check tutorial status once on start
            CheckTutorialStatus();
        }

        void OnEnable()
        {
            // Check again when enabled in case achievement status changed
            if (!hasCheckedTutorial)
            {
                CheckTutorialStatus();
            }
        }

        private void CheckTutorialStatus()
        {
            if (AchievementManager.Instance != null && AchievementManager.Instance.CompleteTutorial)
            {
                hasCheckedTutorial = true;
                gameObject.SetActive(false);
            }
            else if (AchievementManager.Instance != null)
            {
                // Subscribe to achievement changes if a proper event system exists
                // For now, we'll use a coroutine to check periodically instead of every frame
                if (!hasCheckedTutorial)
                {
                    StartCoroutine(CheckTutorialPeriodically());
                }
            }
        }

        private System.Collections.IEnumerator CheckTutorialPeriodically()
        {
            while (!hasCheckedTutorial && gameObject.activeInHierarchy)
            {
                yield return new WaitForSeconds(0.5f); // Check every 0.5 seconds instead of every frame
                
                if (AchievementManager.Instance != null && AchievementManager.Instance.CompleteTutorial)
                {
                    hasCheckedTutorial = true;
                    gameObject.SetActive(false);
                    break;
                }
            }
        }
    }
}
