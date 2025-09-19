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
        }
    }
}
