using System.Threading.Tasks;
using UnityEngine;
using CoreShared;

namespace Core
{
    /// <summary>
    /// Modernized AudioService that works with the new UnifiedAudioManager
    /// Provides a clean interface for the game initialization system
    /// </summary>
    public class ModernAudioService
    {
        private static ModernAudioService _instance;
        public static ModernAudioService Instance => _instance ??= new ModernAudioService();

    private IAudioManager _audioManager; // Use shared interface to avoid asmdef cycles

        private ModernAudioService() { }

        /// <summary>
        /// Initialize the modern audio system
        /// </summary>
        public async Task InitializeAsync()
        {
            Debug.Log("[ModernAudioService]: Initializing modern audio system...");

            // Find existing implementer of IAudioManager in the scene
            var found = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var mb in found)
            {
                if (mb is IAudioManager ia)
                {
                    _audioManager = ia;
                    break;
                }
            }

            if (_audioManager == null)
            {
                Debug.LogWarning("[ModernAudioService]: No IAudioManager implementation found in scene. Ensure UnifiedAudioManager exists or provide a prefab via GameInitializer.");
            }

            // Wait a frame to ensure initialization completes
            await Task.Yield();

            Debug.Log("[ModernAudioService]: Modern audio system initialized successfully");
        }

        /// <summary>
        /// Initialize with prefab reference (for compatibility with GameInitializer)
        /// </summary>
        public async Task InitializeWithPrefabAsync(UnityEngine.Object audioManagerPrefab)
        {
            Debug.Log("[ModernAudioService]: Initializing with prefab...");

            // Find existing or create from prefab
            // Try to find an existing implementer first
            var found = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var mb in found)
            {
                if (mb is IAudioManager ia)
                {
                    _audioManager = ia;
                    break;
                }
            }

            if (_audioManager == null && audioManagerPrefab != null)
            {
                // Instantiate prefab (supports either GameObject or Component prefabs)
                GameObject prefabGo = null;
                if (audioManagerPrefab is GameObject go) prefabGo = go;
                else if (audioManagerPrefab is Component comp) prefabGo = comp.gameObject;

                if (prefabGo != null)
                {
                    var clone = Object.Instantiate(prefabGo);
                    // find IAudioManager on the instantiated object
                    var comps = clone.GetComponentsInChildren<MonoBehaviour>(true);
                    foreach (var mb in comps)
                    {
                        if (mb is IAudioManager ia)
                        {
                            _audioManager = ia;
                            break;
                        }
                    }

                    if (_audioManager != null)
                        Debug.Log("[ModernAudioService]: Created UnifiedAudioManager from prefab");
                    else
                        Debug.LogWarning("[ModernAudioService]: Prefab instantiated but no IAudioManager found on it.");
                }
            }
            else if (_audioManager == null)
            {
                // Fallback to creating new instance (will attempt to find in scene)
                await InitializeAsync();
                return;
            }

            await Task.Yield();
            Debug.Log("[ModernAudioService]: Modern audio system with prefab initialized successfully");
        }

        /// <summary>
        /// Get the UnifiedAudioManager instance
        /// </summary>
        public IAudioManager GetAudioManager()
        {
            return _audioManager;
        }

        /// <summary>
        /// Play a sound effect
        /// </summary>
        public void PlaySFX(AudioClip clip, float volumeScale = 1f)
        {
            var manager = GetAudioManager();
            if (manager != null)
            {
                manager.PlaySFX(clip, volumeScale);
            }
            else
            {
                Debug.LogWarning("[ModernAudioService]: IAudioManager not available for SFX playback");
            }
        }

        /// <summary>
        /// Play music
        /// </summary>
        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            var manager = GetAudioManager();
            if (manager != null)
            {
                manager.PlayMusic(clip, loop);
            }
            else
            {
                Debug.LogWarning("[ModernAudioService]: IAudioManager not available for music playback");
            }
        }

        /// <summary>
        /// Set volume for a specific channel
        /// </summary>
        public void SetVolume(int channelType, float volume)
        {
            var manager = GetAudioManager();
            if (manager != null)
            {
                manager.SetVolume(channelType, volume);
            }
            else
            {
                Debug.LogWarning("[ModernAudioService]: IAudioManager not available for volume control");
            }
        }

        /// <summary>
        /// Get volume for a specific channel
        /// </summary>
        public float GetVolume(int channelType)
        {
            var manager = GetAudioManager();
            if (manager != null)
            {
                return manager.GetVolume(channelType);
            }

            Debug.LogWarning("[ModernAudioService]: IAudioManager not available for volume query");
            return 1f;
        }
    }
}
