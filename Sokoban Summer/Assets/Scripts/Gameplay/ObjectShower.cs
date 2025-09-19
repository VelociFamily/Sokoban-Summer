using UnityEngine;

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
                Debug.Log($"ObjectShower: Player triggered showing of object '{objectToShow.name}'");
            }
            else
            {
                Debug.LogWarning("ObjectShower: objectToShow is not assigned!");
            }
        }
    }
    public void show()
    {
        if (objectToShow != null)
        {
            objectToShow.SetActive(true);
            Debug.Log($"ObjectShower: Manually showing object '{objectToShow.name}'");
        }
        else
        {
            Debug.LogWarning("ObjectShower: objectToShow is not assigned!");
        }
        
        if (menu != null)
        {
            menu.SetActive(false);
            Debug.Log($"ObjectShower: Hiding menu '{menu.name}'");
        }
        else
        {
            Debug.LogWarning("ObjectShower: menu is not assigned!");
        }
    }
}
