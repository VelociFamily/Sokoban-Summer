using System;
using System.Collections.Generic;
using UnityEngine;

namespace Audio
{
    /// <summary>
    /// ScriptableObject that defines volume settings for different audio channels
    /// Allows for data-driven audio configuration
    /// </summary>
    [CreateAssetMenu(fileName = "AudioVolumeSettings", menuName = "Audio/Volume Settings")]
    public class AudioVolumeSettings : ScriptableObject
    {
        [SerializeField] private List<VolumeChannelData> volumeChannels = new List<VolumeChannelData>
        {
            new VolumeChannelData
            {
                ChannelType = AudioChannelType.Master,
                DisplayName = "Master Volume",
                PrefsKey = "Volume_Master",
                MixerParameter = "MasterVolume",
                DefaultVolume = 1f
            },
            new VolumeChannelData
            {
                ChannelType = AudioChannelType.Music,
                DisplayName = "Music Volume", 
                PrefsKey = "Volume_Music",
                MixerParameter = "MusicVolume",
                DefaultVolume = 1f
            },
            new VolumeChannelData
            {
                ChannelType = AudioChannelType.SFX,
                DisplayName = "SFX Volume",
                PrefsKey = "Volume_SFX", 
                MixerParameter = "SFXVolume",
                DefaultVolume = 1f
            }
        };
        
        public List<VolumeChannelData> VolumeChannels => volumeChannels;
        
        public VolumeChannelData GetVolumeChannel(AudioChannelType channelType)
        {
            return volumeChannels.Find(x => x.ChannelType == channelType);
        }
    }
    
    /// <summary>
    /// Data structure for volume channel configuration
    /// </summary>
    [Serializable]
    public class VolumeChannelData
    {
        [Header("Channel Configuration")]
        public AudioChannelType ChannelType;
        public string DisplayName;
        
        [Header("Storage")]
        public string PrefsKey;
        public string MixerParameter;
        
        [Header("Default Settings")]
        [Range(0f, 1f)]
        public float DefaultVolume = 1f;
    }
}
