using UnityEngine;

public class Ifpressedshow : MonoBehaviour
{
    public GameObject objectToShow;
    public GameObject menu;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            objectToShow.SetActive(true);
        }
    }
    public void show()
    {
        objectToShow.SetActive(true);
        menu.SetActive(false);
    }
}
