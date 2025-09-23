using Core;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gameplay
{
    public class PortalTrigger : MonoBehaviour
    {
        public GameObject player;
        // Removed: public AudioSource audioSource; - now using centralized AudioService
        public AudioClip portalSound;
        public ParticleSystem portalEffect;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                Debug.Log("[PortalTrigger]: Level completed - Player reached portal");
            
                // Use centralized AudioService instead of local AudioSource
                if (portalSound != null)
                {
                    ModernAudioService.Instance.PlaySFX(portalSound);
                }
                else
                {
                    Debug.LogWarning("[PortalTrigger]: Portal sound not assigned - no completion sound");
                }

                if (portalEffect != null)
                {
                    portalEffect.Play();
                }
                else
                {
                    Debug.LogWarning("[PortalTrigger]: Portal effect not assigned - no visual feedback");
                }

                if (player != null)
                {
                    player.SetActive(false);
                }
                else
                {
                    Debug.LogWarning("[PortalTrigger]: Player reference not assigned - cannot disable player");
                }

                // Use scene name for better maintainability instead of buildIndex
                var currentScene = SceneManager.GetActiveScene();
                SceneSelector.MarkNextLevelUnlocked(currentScene.buildIndex+1); // SceneSelector still uses buildIndex internally
                Debug.Log($"[PortalTrigger]: Scene '{currentScene.name}' marked as completed and unlocked for progression");

                // (Optional) Automatically go to the main menu or next scene
                // SceneManager.LoadScene("MainMenu");
            }
        }
    }
}
