using UnityEngine;

public class ConfusePowerDown : MonoBehaviour
{
    public static int confuseTurns;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            confuseTurns = 5;
            gameObject.SetActive(false);
            Debug.Log("[ConfusePowerDown]: Confusion effect activated - 5 turns of reversed controls");
            
            // Start the confusion animation immediately
            var playerController = other.GetComponent<PlayerController>();
            if (playerController != null && playerController.confuseEffect != null && !playerController.confuseEffect.isPlaying)
            {
                playerController.confuseEffect.Play();
                Debug.Log("[ConfusePowerDown]: Confusion animation started immediately");
            }
        }
    }
}
