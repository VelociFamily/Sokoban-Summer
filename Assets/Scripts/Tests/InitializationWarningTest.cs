using UnityEngine;
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
            
            // Test 1: AchievementManager initialization
            await TestAchievementManagerInitialization();
            
            Debug.Log("=== Initialization Warning Test Completed ===");
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
    }
}
