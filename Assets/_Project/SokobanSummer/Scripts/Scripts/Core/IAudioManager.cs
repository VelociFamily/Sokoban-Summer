// This file was intentionally disabled: the canonical IAudioManager contract
// lives in `Assets/Scripts/CoreShared/IAudioManager.cs` (namespace CoreShared).
// Keep this file present for history but exclude its type from compilation
// to avoid ambiguous references between Core.IAudioManager and CoreShared.IAudioManager.

#if false
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Minimal interface for audio manager access from Core.
    /// Implement this in Audio.UnifiedAudioManager.
    /// </summary>
    public interface IAudioManager
    {
        void PlaySFX(AudioClip clip, float volumeScale = 1f);
        void PlayMusic(AudioClip clip, bool loop = true);
        void SetVolume(int channelType, float volume); // Use int or enum for channelType
        float GetVolume(int channelType);
    }
}
#endif
