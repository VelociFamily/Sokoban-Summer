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
        }
    }
}
