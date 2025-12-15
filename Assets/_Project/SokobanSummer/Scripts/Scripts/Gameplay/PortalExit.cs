using Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gameplay
{
    /// <summary>
    /// Attach to the portal/exit object. When the player enters, shows the Level Complete panel.
    /// One-shot: prevents re-triggering while the panel is shown.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PortalExit : MonoBehaviour
    {
        [Tooltip("Tag used to identify the player GameObject")] 
        [SerializeField] private string playerTag = "Player";

        [Tooltip("Optional: automatically pause gameplay time when level completes")] 
        [SerializeField] private bool pauseOnComplete = true;

        private bool triggered;

        private void Reset()
        {
            // Ensure trigger collider for 2D portals by default
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggered) return;
            if (other == null) return;

            // Basic player detection via tag
            if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag)) return;

            // Only trigger in gameplay scenes
            var activeScene = SceneManager.GetActiveScene();
            if (!SceneInfo.IsGameplayScene(activeScene)) return;

            triggered = true;

            // Make sure persistent UI is visible (avoids hidden CanvasGroups during gameplay)
            if (PersistentUIManager.Exists)
            {
                PersistentUIManager.Show(animated: false);
            }

            // Show Level Complete via MenuNavigator without direct assembly reference (reflection)
            MonoBehaviour menuNavigator = null;
            // Try ServiceLocator first (returns object typed as MonoBehaviour when casting later)
            if (ServiceLocator.TryGet(out object resolved))
            {
                var mb = resolved as MonoBehaviour;
                if (mb != null && mb.GetType().Name == "MenuNavigator")
                {
                    menuNavigator = mb;
                }
            }
            if (menuNavigator == null)
            {
                // Fallback: scan loaded MonoBehaviours and pick the first with type name 'MenuNavigator'
                var behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
                foreach (var b in behaviours)
                {
                    if (b != null && b.GetType().Name == "MenuNavigator")
                    {
                        menuNavigator = b;
                        break;
                    }
                }
            }

            if (menuNavigator != null)
            {
                if (pauseOnComplete) Time.timeScale = 0f;
                var t = menuNavigator.GetType();
                var method = t.GetMethod("ShowLevelComplete", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (method != null)
                {
                    method.Invoke(menuNavigator, null);
                }
                else
                {
                    Debug.LogWarning("[PortalExit]: Found MenuNavigator but method 'ShowLevelComplete' was not found.");
                    if (pauseOnComplete) Time.timeScale = 1f;
                    triggered = false;
                }
            }
            else
            {
                Debug.LogWarning("[PortalExit]: MenuNavigator instance not found. Ensure it exists in the Persistent UI scene.");
                // As a fallback, unpause to avoid lock-up
                if (pauseOnComplete) Time.timeScale = 1f;
                triggered = false; // allow retry if navigator appears later
            }
        }
    }
}
