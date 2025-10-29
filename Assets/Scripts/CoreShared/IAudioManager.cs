using UnityEngine;

namespace CoreShared
{
    /// <summary>
    /// Minimal interface for audio manager access from Core.
    /// Implement this in Audio.UnifiedAudioManager.
    /// </summary>
    public interface IAudioManager
    {
        void PlaySFX(AudioClip clip, float volumeScale = 1f);
        void PlayMusic(AudioClip clip, bool loop = true);
        void SetVolume(int channelType, float volume);
        float GetVolume(int channelType);
    }
}
