using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalTrigger : MonoBehaviour
{
    public GameObject player;
    public AudioSource audioSource;
    public AudioClip portalSound;
    public ParticleSystem portalEffect;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("PortalTrigger: Player entered portal - Level complete!");
            
            if (audioSource != null && portalSound != null)
            {
                audioSource.PlayOneShot(portalSound);
                Debug.Log("PortalTrigger: Playing portal sound effect");
            }
            else
            {
                Debug.LogWarning("PortalTrigger: Audio source or portal sound not assigned");
            }

            if (portalEffect != null)
            {
                portalEffect.Play();
                Debug.Log("PortalTrigger: Playing portal particle effect");
            }
            else
            {
                Debug.LogWarning("PortalTrigger: Portal effect not assigned");
            }

            if (player != null)
            {
                player.SetActive(false);
                Debug.Log("PortalTrigger: Player disabled");
            }
            else
            {
                Debug.LogWarning("PortalTrigger: Player reference not assigned");
            }

            var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneSelector.MarkSceneCompleted(currentSceneIndex);
            Debug.Log($"PortalTrigger: Scene {currentSceneIndex} marked as completed");

            // (Optional) Automatically go to the main menu or next scene
            // SceneManager.LoadScene("MainMenu");
        }
    }
}