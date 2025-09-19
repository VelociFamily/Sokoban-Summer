using JetBrains.Annotations;
using UnityEngine;

public class TeleportPowerUp : MonoBehaviour
{
    public GameObject teleportItem;
    public static int teleportTimes;
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            teleportTimes = 3;
            teleportItem.SetActive(false);
        }
    }
}
