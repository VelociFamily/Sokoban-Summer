using System.Threading.Tasks;
using UnityEngine;

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

        private Audio.UnifiedAudioManager _audioManager;

        private ModernAudioService() { }

        /// <summary>
        /// Initialize the modern audio system
        /// </summary>
        public async Task InitializeAsync()
        {
            Debug.Log("[ModernAudioService]: Initializing modern audio system...");

            // Find or create UnifiedAudioManager
            _audioManager = Object.FindFirstObjectByType<Audio.UnifiedAudioManager>();
            if (_audioManager == null)
            {
                // Create UnifiedAudioManager GameObject
                var audioManagerObject = new GameObject("UnifiedAudioManager");
                _audioManager = audioManagerObject.AddComponent<Audio.UnifiedAudioManager>();
                
                Debug.Log("[ModernAudioService]: Created UnifiedAudioManager");
            }
            else
            {
                Debug.Log("[ModernAudioService]: Found existing UnifiedAudioManager");
            }

            // Wait a frame to ensure initialization completes
            await Task.Yield();
            
            Debug.Log("[ModernAudioService]: Modern audio system initialized successfully");
        }

        /// <summary>
        /// Initialize with prefab reference (for compatibility with GameInitializer)
        /// </summary>
        public async Task InitializeWithPrefabAsync(Audio.UnifiedAudioManager audioManagerPrefab)
        {
            Debug.Log("[ModernAudioService]: Initializing with prefab...");
            
            // Find existing or create from prefab
            _audioManager = Object.FindFirstObjectByType<Audio.UnifiedAudioManager>();
            if (_audioManager == null && audioManagerPrefab != null)
            {
                _audioManager = Object.Instantiate(audioManagerPrefab);
                Debug.Log("[ModernAudioService]: Created UnifiedAudioManager from prefab");
            }
            else if (_audioManager == null)
            {
                // Fallback to creating new instance
                await InitializeAsync();
                return;
            }

            await Task.Yield();
            Debug.Log("[ModernAudioService]: Modern audio system with prefab initialized successfully");
        }

        /// <summary>
        /// Get the UnifiedAudioManager instance
        /// </summary>
        public Audio.UnifiedAudioManager GetAudioManager()
        {
            return _audioManager ?? Audio.UnifiedAudioManager.Instance;
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
                Debug.LogWarning("[ModernAudioService]: UnifiedAudioManager not available for SFX playback");
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
                Debug.LogWarning("[ModernAudioService]: UnifiedAudioManager not available for music playback");
            }
        }

        /// <summary>
        /// Set volume for a specific channel
        /// </summary>
        public void SetVolume(Audio.AudioChannelType channelType, float volume)
        {
            var manager = GetAudioManager();
            if (manager != null)
            {
                manager.SetVolume(channelType, volume);
            }
            else
            {
                Debug.LogWarning("[ModernAudioService]: UnifiedAudioManager not available for volume control");
            }
        }

        /// <summary>
        /// Get volume for a specific channel
        /// </summary>
        public float GetVolume(Audio.AudioChannelType channelType)
        {
            var manager = GetAudioManager();
            if (manager != null)
            {
                return manager.GetVolume(channelType);
            }
            
            Debug.LogWarning("[ModernAudioService]: UnifiedAudioManager not available for volume query");
            return 1f;
        }
    }
}