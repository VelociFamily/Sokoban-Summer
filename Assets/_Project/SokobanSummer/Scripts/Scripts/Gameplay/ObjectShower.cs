using UnityEngine;
using UI;

namespace Gameplay
{
    public class ObjectShower : MonoBehaviour
    {
        public GameObject objectToShow;
        public GameObject menu;
        private MenuNavigator menuNavigator;

        private void Awake()
        {
            // Discover MenuNavigator in the persistent UI scene
            menuNavigator = FindFirstObjectByType<MenuNavigator>();
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (objectToShow != null)
                {
                    objectToShow.SetActive(true);
                    Debug.Log($"[ObjectShower]: Player revealed object '{objectToShow.name}'");

                    // Drive UI via MenuNavigator instead of SetActive
                    menuNavigator?.ShowMainMenu();
                }
                else
                {
                    Debug.LogWarning("[ObjectShower]: Object to show not assigned - trigger has no effect");
                }
            }
        }
        public void show()
        {
            if (objectToShow != null)
            {
                objectToShow.SetActive(true);
                Debug.Log($"[ObjectShower]: Manually activated object '{objectToShow.name}'");

                // Ensure UI state via MenuNavigator; avoid SetActive toggling
                menuNavigator?.ShowMainMenu();
            }
            else
            {
                Debug.LogWarning("[ObjectShower]: Object to show not assigned - manual activation failed");
            }
        }
    }
}
