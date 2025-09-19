using UnityEngine;
using UnityEngine.SceneManagement;

public class Ifportaltouched : MonoBehaviour
{
    public GameObject player;
    public AudioSource audioSource;
    public AudioClip portalSound;
    public ParticleSystem portalEffect;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (audioSource != null && portalSound != null)
                audioSource.PlayOneShot(portalSound);

            if (portalEffect != null)
                portalEffect.Play();

            if (player != null)
                player.SetActive(false);

            var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneSelector.MarkSceneCompleted(currentSceneIndex);

            // (Optional) Automatically go to the main menu or next scene
            // SceneManager.LoadScene("MainMenu");
        }
    }
}