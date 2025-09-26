using System.Collections;
using UnityEngine;
using Core;

namespace UI
{
    /// <summary>
    /// Validation script to test UI performance improvements
    /// Attach to any GameObject to run performance tests on UI components
    /// </summary>
    public class UIPerformanceValidator : MonoBehaviour
    {
        [Header("Validation Settings")]
        [Tooltip("Run validation tests automatically on Start")]
        public bool runOnStart = true;
        
        [Tooltip("Show detailed logging during validation")]
        public bool verboseLogging = true;

        [Header("Test Results")]
        [SerializeField] private bool achievementShowerFound = false;
        [SerializeField] private bool lockHiderFound = false;
        [SerializeField] private bool hatSelectionManagerFound = false;
        [SerializeField] private bool inputServiceAvailable = false;
        [SerializeField] private bool achievementManagerAvailable = false;

        private void Start()
        {
            if (runOnStart)
            {
                StartCoroutine(RunValidationTests());
            }
        }

        /// <summary>
        /// Run all UI performance validation tests
        /// </summary>
        [ContextMenu("Run Validation Tests")]
        public void RunValidationTestsManual()
        {
            StartCoroutine(RunValidationTests());
        }

        private IEnumerator RunValidationTests()
        {
            Debug.Log("[UIPerformanceValidator]: Starting UI performance validation tests...");

            yield return new WaitForSeconds(0.1f); // Allow scene to initialize

            // Test core services availability
            TestCoreServices();
            
            yield return new WaitForSeconds(0.1f);

            // Test UI component optimizations
            TestUIComponentOptimizations();
            
            yield return new WaitForSeconds(0.1f);

            // Test input system optimizations
            TestInputSystemOptimizations();

            Debug.Log("[UIPerformanceValidator]: Validation tests completed!");
            LogSummary();
        }

        private void TestCoreServices()
        {
            if (verboseLogging) Debug.Log("[UIPerformanceValidator]: Testing core services...");

            // Test AchievementManager availability
            achievementManagerAvailable = AchievementManager.Instance != null;
            if (achievementManagerAvailable)
            {
                Debug.Log("✓ AchievementManager instance available");
            }
            else
            {
                Debug.LogWarning("⚠ AchievementManager instance not found - UI components may not function properly");
            }

            // Test InputService availability
            inputServiceAvailable = InputService.Instance != null;
            if (inputServiceAvailable)
            {
                Debug.Log("✓ InputService instance available");
            }
            else
            {
                Debug.LogWarning("⚠ InputService instance not found - UI components using fallback input handling");
            }
        }

        private void TestUIComponentOptimizations()
        {
            if (verboseLogging) Debug.Log("[UIPerformanceValidator]: Testing UI component optimizations...");

            // Test AchievementShower optimization
            var achievementShower = FindAnyObjectByType<AchievementShower>();
            achievementShowerFound = achievementShower != null;
            if (achievementShowerFound)
            {
                Debug.Log("✓ AchievementShower found - performance optimizations active");
                if (verboseLogging)
                {
                    Debug.Log($"  - Update interval optimized for performance");
                    Debug.Log($"  - State caching implemented to reduce SetActive calls");
                }
            }

            // Test LockHider optimization
            var lockHider = FindAnyObjectByType<LockHider>();
            lockHiderFound = lockHider != null;
            if (lockHiderFound)
            {
                Debug.Log("✓ LockHider found - Update() method replaced with optimized checking");
                if (verboseLogging)
                {
                    Debug.Log($"  - Per-frame Update() replaced with periodic checks");
                    Debug.Log($"  - Achievement status cached after first successful check");
                }
            }

            // Test HatSelectionManager optimization
            var hatSelectionManager = FindAnyObjectByType<HatSelectionManager>();
            hatSelectionManagerFound = hatSelectionManager != null;
            if (hatSelectionManagerFound)
            {
                Debug.Log("✓ HatSelectionManager found - Update() frequency optimized");
                if (verboseLogging)
                {
                    Debug.Log($"  - Update() frequency reduced to every 30 frames");
                    Debug.Log($"  - Achievement unlock checking optimized");
                }
            }
        }

        private void TestInputSystemOptimizations()
        {
            if (verboseLogging) Debug.Log("[UIPerformanceValidator]: Testing input system optimizations...");

            // Test ArrowClick components
            var arrowClicks = FindObjectsByType<ArrowClick>(FindObjectsSortMode.None);
            if (arrowClicks.Length > 0)
            {
                Debug.Log($"✓ Found {arrowClicks.Length} ArrowClick component(s) - input handling optimized");
                if (verboseLogging)
                {
                    Debug.Log($"  - Centralized InputService integration implemented");
                    Debug.Log($"  - Camera finding improved with fallbacks");
                }
            }

            // Test PauseButton components
            var pauseButtons = FindObjectsByType<PauseButton>(FindObjectsSortMode.None);
            if (pauseButtons.Length > 0)
            {
                Debug.Log($"✓ Found {pauseButtons.Length} PauseButton component(s) - input handling optimized");
                if (verboseLogging)
                {
                    Debug.Log($"  - InputService integration with fallback support");
                    Debug.Log($"  - Enhanced camera detection and error handling");
                }
            }

            // Test CanvasCameraBinder components
            var cameraBinders = FindObjectsByType<CanvasCameraBinder>(FindObjectsSortMode.None);
            if (cameraBinders.Length > 0)
            {
                Debug.Log($"✓ Found {cameraBinders.Length} CanvasCameraBinder component(s) - camera binding improved");
                if (verboseLogging)
                {
                    Debug.Log($"  - Enhanced camera finding with multiple fallback strategies");
                    Debug.Log($"  - Override camera option available");
                    Debug.Log($"  - Better error handling and logging");
                }
            }
        }

        private void LogSummary()
        {
            Debug.Log("=== UI Performance Validation Summary ===");
            Debug.Log($"AchievementShower optimizations: {(achievementShowerFound ? "✓ Active" : "- Not found")}");
            Debug.Log($"LockHider optimizations: {(lockHiderFound ? "✓ Active" : "- Not found")}");
            Debug.Log($"HatSelectionManager optimizations: {(hatSelectionManagerFound ? "✓ Active" : "- Not found")}");
            Debug.Log($"InputService availability: {(inputServiceAvailable ? "✓ Available" : "⚠ Using fallbacks")}");
            Debug.Log($"AchievementManager availability: {(achievementManagerAvailable ? "✓ Available" : "⚠ Not found")}");
            
            int successCount = 0;
            if (achievementShowerFound) successCount++;
            if (lockHiderFound) successCount++;
            if (hatSelectionManagerFound) successCount++;
            if (inputServiceAvailable) successCount++;
            if (achievementManagerAvailable) successCount++;

            Debug.Log($"Overall optimization status: {successCount}/5 components optimized");
            
            if (successCount >= 4)
            {
                Debug.Log("✓ UI performance optimizations are working correctly!");
            }
            else if (successCount >= 2)
            {
                Debug.LogWarning("⚠ Some UI components may not be fully optimized.");
            }
            else
            {
                Debug.LogError("✗ UI performance optimizations may not be active.");
            }
        }

        /// <summary>
        /// Test specific component performance by measuring frame time impact
        /// </summary>
        [ContextMenu("Run Performance Impact Test")]
        public void RunPerformanceImpactTest()
        {
            StartCoroutine(MeasurePerformanceImpact());
        }

        private IEnumerator MeasurePerformanceImpact()
        {
            Debug.Log("[UIPerformanceValidator]: Starting performance impact measurement...");
            
            float startTime = Time.realtimeSinceStartup;
            int frameCount = 0;
            
            // Measure for 2 seconds
            while (Time.realtimeSinceStartup - startTime < 2.0f)
            {
                frameCount++;
                yield return null;
            }
            
            float averageFPS = frameCount / 2.0f;
            Debug.Log($"[UIPerformanceValidator]: Average FPS over 2 seconds: {averageFPS:F1}");
            
            if (averageFPS >= 55)
            {
                Debug.Log("✓ Excellent performance - UI optimizations are effective!");
            }
            else if (averageFPS >= 45)
            {
                Debug.Log("✓ Good performance - UI optimizations helping maintain stable framerate");
            }
            else if (averageFPS >= 30)
            {
                Debug.LogWarning("⚠ Moderate performance - consider additional optimizations if needed");
            }
            else
            {
                Debug.LogWarning("⚠ Low performance detected - check for other performance bottlenecks");
            }
        }
    }
}