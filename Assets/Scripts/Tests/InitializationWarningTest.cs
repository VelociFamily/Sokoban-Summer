using UnityEngine;
using UnityEngine.UI;
using Audio;
using Core;
using System.Threading.Tasks;

namespace Tests
{
    /// <summary>
    /// Test script to validate the initialization warning fixes
    /// This should be attached to a GameObject in a test scene to verify warnings are resolved
    /// </summary>
    public class InitializationWarningTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        public bool runTestOnStart = true;
        public bool createTestSliders = true;
        
        private async void Start()
        {
            if (runTestOnStart)
            {
                await RunInitializationTest();
            }
        }

        [ContextMenu("Run Initialization Test")]
        public async Task RunInitializationTest()
        {
            Debug.Log("=== Initialization Warning Test Started ===");

            // Create test UI elements if requested
            if (createTestSliders)
            {
                CreateTestSliders();
                await Task.Delay(100); // Give time for objects to be created
            }

            // Test 1: SfxVolumeControl initialization
            await TestSfxVolumeControlInitialization();
            
            // Test 2: AchievementManager initialization
            await TestAchievementManagerInitialization();
            
            Debug.Log("=== Initialization Warning Test Completed ===");
        }

        private async Task TestSfxVolumeControlInitialization()
        {
            Debug.Log("--- Testing SfxVolumeControl Initialization ---");
            
            // Clear any existing instances
            var existingSfx = FindObjectsByType<SfxVolumeControl>(FindObjectsSortMode.None);
            foreach (var sfx in existingSfx)
            {
                DestroyImmediate(sfx.gameObject);
            }
            
            // Create a new SfxVolumeControl
            var sfxObject = new GameObject("TestSfxVolumeControl");
            var sfxVolumeControl = sfxObject.AddComponent<SfxVolumeControl>();
            
            // Initialize it like AudioService does
            try
            {
                sfxVolumeControl.Initialize();
                Debug.Log("✓ SfxVolumeControl initialized without exceptions");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"✗ SfxVolumeControl initialization failed: {e.Message}");
            }
            
            await Task.Yield();
        }

        private async Task TestAchievementManagerInitialization()
        {
            Debug.Log("--- Testing AchievementManager Initialization ---");
            
            // Clear any existing instances
            var existingAchievement = FindObjectsByType<AchievementManager>(FindObjectsSortMode.None);
            foreach (var achievement in existingAchievement)
            {
                DestroyImmediate(achievement.gameObject);
            }
            
            // Test the GameInitializer's AchievementManager initialization logic
            try
            {
                var achievementManagerObj = FindFirstObjectByType<AchievementManager>();
                if (achievementManagerObj != null)
                {
                    await achievementManagerObj.InitializeAsync();
                    Debug.Log("✓ Found existing AchievementManager and initialized");
                }
                else
                {
                    // This mimics the fixed GameInitializer logic
                    var achievementManagerGameObject = new GameObject("TestAchievementManager");
                    var achievementManager = achievementManagerGameObject.AddComponent<AchievementManager>();
                    await achievementManager.InitializeAsync();
                    Debug.Log("✓ Created new AchievementManager and initialized");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"✗ AchievementManager initialization failed: {e.Message}");
            }
            
            await Task.Yield();
        }

        private void CreateTestSliders()
        {
            Debug.Log("--- Creating Test UI Sliders ---");
            
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

            // Create SFX Slider with proper naming/tagging
            var sfxSliderObject = new GameObject("SFX Slider");
            sfxSliderObject.transform.SetParent(canvas.transform);
            sfxSliderObject.tag = "SFXSlider"; // This should help the fallback logic
            
            var sfxSlider = sfxSliderObject.AddComponent<Slider>();
            sfxSlider.value = 1.0f;

            // Create VolumeSliderConnector
            var connectorObject = new GameObject("VolumeSliderConnector");
            connectorObject.transform.SetParent(canvas.transform);
            var connector = connectorObject.AddComponent<VolumeSliderConnector>();
            connector.sfxSlider = sfxSlider;

            Debug.Log("✓ Created test sliders and connector");
        }
    }
}
