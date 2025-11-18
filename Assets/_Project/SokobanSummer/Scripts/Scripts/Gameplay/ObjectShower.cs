using UnityEngine;

namespace Gameplay
{
    public class ObjectShower : MonoBehaviour
    {
        public GameObject objectToShow;
        public GameObject menu;

        private void Awake()
        {
            // No-op: UI is managed centrally by PersistentUIManager
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (objectToShow != null)
                {
                    objectToShow.SetActive(true);
                    Debug.Log($"[ObjectShower]: Player revealed object '{objectToShow.name}'");

                    // Ensure persistent UI is visible (centralized UI flow)
                    Core.PersistentUIManager.Show(true);
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

                // Ensure persistent UI is visible (centralized UI flow)
                Core.PersistentUIManager.Show(true);
            }
            else
            {
                Debug.LogWarning("[ObjectShower]: Object to show not assigned - manual activation failed");
            }
        }
    }
}
