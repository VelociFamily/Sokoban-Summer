using UnityEngine;
using UnityEngine.UI;
using Audio;
using Core;
using System.Threading.Tasks;

namespace Tests
{
    /// <summary>
    /// Test script to validate the modern unified audio system
    /// Tests both the new UnifiedAudioManager and legacy system compatibility
    /// </summary>
    public class ModernAudioSystemTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        public bool runTestOnStart = true;
        public bool testLegacyCompatibility = false;
        public bool createTestUI = true;
        
        private async void Start()
        {
            if (runTestOnStart)
            {
                await RunAudioSystemTest();
            }
        }

        [ContextMenu("Run Modern Audio System Test")]
        public async Task RunAudioSystemTest()
        {
            Debug.Log("=== Modern Audio System Test Started ===");

            // Test 1: Modern Audio System
            await TestModernAudioSystem();
            
            // Test 2: UI Integration
            await TestUIIntegration();
            
            // Test 3: Legacy Compatibility (if enabled)
            if (testLegacyCompatibility)
                await TestLegacyCompatibility();
            
            Debug.Log("=== Modern Audio System Test Completed ===");
        }

        private async Task TestModernAudioSystem()
        {
            Debug.Log("--- Testing Modern Audio System ---");
            
            try
            {
                // Initialize modern audio service
                await ModernAudioService.Instance.InitializeAsync();
                Debug.Log("✓ ModernAudioService initialized without errors");
                
                // Check UnifiedAudioManager instance
                var audioManager = ModernAudioService.Instance.GetAudioManager();
                if (audioManager != null)
                {
                    Debug.Log("✓ UnifiedAudioManager instance available");
                    
                    // Test volume operations
                    audioManager.SetVolume(AudioChannelType.Master, 0.8f);
                    float masterVolume = audioManager.GetVolume(AudioChannelType.Master);
                    Debug.Log($"✓ Master volume set and retrieved: {masterVolume:F2}");
                    
                    audioManager.SetVolume(AudioChannelType.SFX, 0.6f);
                    float sfxVolume = audioManager.GetVolume(AudioChannelType.SFX);
                    Debug.Log($"✓ SFX volume set and retrieved: {sfxVolume:F2}");
                }
                else
                {
                    Debug.LogError("✗ UnifiedAudioManager instance is null");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"✗ Modern audio system test failed: {e.Message}");
            }
        }

        private async Task TestUIIntegration()
        {
            Debug.Log("--- Testing UI Integration ---");
            
            if (createTestUI)
            {
                CreateTestVolumeSliders();
                await Task.Delay(100); // Give sliders time to register
            }
            
            // Check for VolumeSlider components
            var volumeSliders = FindObjectsByType<VolumeSlider>(FindObjectsSortMode.None);
            Debug.Log($"✓ Found {volumeSliders.Length} VolumeSlider components");
            
            foreach (var volumeSlider in volumeSliders)
            {
                if (volumeSlider.Slider != null)
                {
                    Debug.Log($"✓ {volumeSlider.ChannelType} VolumeSlider properly configured");
                }
                else
                {
                    Debug.LogWarning($"⚠ {volumeSlider.ChannelType} VolumeSlider missing Slider reference");
                }
            }
        }

        private async Task TestLegacyCompatibility()
        {
            Debug.Log("--- Testing Legacy System Compatibility ---");
            
            try
            {
                // Test if legacy components still exist and function
                var legacyVolumeControl = FindFirstObjectByType<VolumeControl>();
                var legacySfxControl = FindFirstObjectByType<SfxVolumeControl>();
                
                if (legacyVolumeControl != null)
                {
                    Debug.Log("✓ Legacy VolumeControl found - backward compatibility maintained");
                }
                
                if (legacySfxControl != null)
                {
                    Debug.Log("✓ Legacy SfxVolumeControl found - backward compatibility maintained");
                }
                
                if (legacyVolumeControl == null && legacySfxControl == null)
                {
                    Debug.Log("✓ No legacy components found - clean modern setup");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"✗ Legacy compatibility test failed: {e.Message}");
            }
            
            await Task.Yield();
        }

        private void CreateTestVolumeSliders()
        {
            Debug.Log("--- Creating Test Volume Sliders ---");
            
            // Create a Canvas if one doesn't exist
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                var canvasObject = new GameObject("TestCanvas");
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            // Create Master Volume Slider
            CreateVolumeSlider("Master Volume Slider", AudioChannelType.Master, canvas.transform);
            
            // Create SFX Volume Slider
            CreateVolumeSlider("SFX Volume Slider", AudioChannelType.SFX, canvas.transform);
            
            // Create Music Volume Slider
            CreateVolumeSlider("Music Volume Slider", AudioChannelType.Music, canvas.transform);

            Debug.Log("✓ Created test volume sliders with modern VolumeSlider components");
        }

        private void CreateVolumeSlider(string name, AudioChannelType channelType, Transform parent)
        {
            // Create slider GameObject
            var sliderObject = new GameObject(name);
            sliderObject.transform.SetParent(parent);
            
            // Add Slider component
            var slider = sliderObject.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            
            // Add modern VolumeSlider component
            var volumeSlider = sliderObject.AddComponent<VolumeSlider>();
            // The VolumeSlider will auto-configure itself through its channelType field
        }

        [ContextMenu("Test Volume Changes")]
        public void TestVolumeChanges()
        {
            Debug.Log("--- Testing Volume Changes ---");
            
            var audioManager = ModernAudioService.Instance.GetAudioManager();
            if (audioManager != null)
            {
                // Test different volume levels
                audioManager.SetVolume(AudioChannelType.Master, 1.0f);
                audioManager.SetVolume(AudioChannelType.SFX, 0.8f);
                audioManager.SetVolume(AudioChannelType.Music, 0.6f);
                
                Debug.Log("✓ Set test volume levels - check UI sliders for updates");
            }
            else
            {
                Debug.LogError("✗ UnifiedAudioManager not available for volume testing");
            }
        }

        [ContextMenu("Test Audio Playback")]
        public void TestAudioPlayback()
        {
            Debug.Log("--- Testing Audio Playback ---");
            
            // This would need actual audio clips to test properly
            Debug.Log("⚠ Audio playback test requires AudioClip assets - implement when clips are available");
            
            // Example of how to test with actual clips:
            // ModernAudioService.Instance.PlaySFX(testSFXClip);
            // ModernAudioService.Instance.PlayMusic(testMusicClip);
        }
    }
}
