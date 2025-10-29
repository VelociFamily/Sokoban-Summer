using Core;
using UnityEngine;

namespace Audio
{
    /// <summary>
    /// Extension methods that provide convenience overloads for ModernAudioService
    /// allowing callers to use Audio.AudioChannelType without manual casts.
    /// Placed in the Audio assembly so Core doesn't need to reference Audio.
    /// </summary>
    public static class ModernAudioServiceExtensions
    {
        /// <summary>
        /// Set volume using AudioChannelType enum.
        /// </summary>
        public static void SetVolume(this ModernAudioService svc, AudioChannelType channel, float volume)
        {
            if (svc == null) return;
            svc.SetVolume((int)channel, volume);
        }

        /// <summary>
        /// Get volume using AudioChannelType enum.
        /// </summary>
        public static float GetVolume(this ModernAudioService svc, AudioChannelType channel)
        {
            if (svc == null) return 1f;
            return svc.GetVolume((int)channel);
        }
    }
}
