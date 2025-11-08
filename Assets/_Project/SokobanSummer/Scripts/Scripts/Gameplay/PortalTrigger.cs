using Core;
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
                    var audioService = SokobanSummer.Core.ServiceLocator.Get<ModernAudioService>();
                    audioService.PlaySFX(portalSound);
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

                // Notify LevelManager of completion to unlock progression
                var currentScene = SceneManager.GetActiveScene();
                var levelManager = SokobanSummer.Core.ServiceLocator.Get<LevelManager>();
                if (levelManager != null)
                {
                    levelManager.MarkLevelCompleted(currentScene.buildIndex);
                }

                Debug.Log($"[PortalTrigger]: Scene '{currentScene.name}' marked as completed");

                // (Optional) Automatically go to the main menu or next scene
                // SceneManager.LoadScene("MainMenu");
            }
        }
    }
}
