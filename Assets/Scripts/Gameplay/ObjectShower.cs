using UnityEngine;

namespace Gameplay
{
    public class ObjectShower : MonoBehaviour
    {
        public GameObject objectToShow;
        public GameObject menu;
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (objectToShow != null)
                {
                    objectToShow.SetActive(true);
                    Debug.Log($"[ObjectShower]: Player revealed object '{objectToShow.name}'");
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
            }
            else
            {
                Debug.LogWarning("[ObjectShower]: Object to show not assigned - manual activation failed");
            }
        
            if (menu != null)
            {
                menu.SetActive(false);
            }
            else
            {
                Debug.LogWarning("[ObjectShower]: Menu reference not assigned - cannot hide menu");
            }
        }
    }
}
