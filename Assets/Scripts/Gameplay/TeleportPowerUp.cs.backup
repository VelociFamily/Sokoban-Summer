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
            Debug.Log("[TeleportPowerUp]: Teleport power-up collected - 3 uses granted");
            
            // Start the teleport animation immediately
            var playerController = collision.GetComponent<PlayerController>();
            if (playerController != null && playerController.teleportEffect != null && !playerController.teleportEffect.isPlaying)
            {
                playerController.teleportEffect.Play();
                Debug.Log("[TeleportPowerUp]: Teleport animation started immediately");
            }
        }
    }
}
