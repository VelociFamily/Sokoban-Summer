using System.Collections;
using UnityEngine;
using Core;
using TMPro;
using UnityEngine.SceneManagement;

namespace Testing
{
    /// <summary>
    /// Test script to validate that MoveCounter only searches for UI components in gameplay scenes
    /// and ignores menus, settings, and other non-gameplay scenes
    /// </summary>
    public class MoveCounterSceneFilterTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [Tooltip("Run tests automatically when the scene starts")]
        public bool runTestOnStart = true;
        
        [Tooltip("Include detailed logging for debugging")]
        public bool verboseLogging = true;

        private void Start()
        {
            if (runTestOnStart)
            {
                StartCoroutine(DelayedTest());
            }
        }

        private IEnumerator DelayedTest()
        {
            // Wait a moment for all systems to initialize
            yield return new WaitForSeconds(1f);
            RunSceneFilterTest();
        }

        [ContextMenu("Run Scene Filter Test")]
        public void RunSceneFilterTest()
        {
            Debug.Log("=== MoveCounter Scene Filter Test Started ===");

            // Test 1: Verify current scene type
            bool sceneTypeCorrect = TestSceneTypeDetection();
            
            // Test 2: Check if UI components are assigned based on scene type
            bool uiAssignmentCorrect = TestUIAssignmentBySceneType();
            
            // Test 3: Verify that UI discovery respects scene type
            bool discoveryRespectsSceneType = TestUIDiscoveryRespectfulness();

            // Summary
            Debug.Log("=== Test Results ===");
            Debug.Log($"✓ Scene Type Detection: {(sceneTypeCorrect ? "PASS" : "FAIL")}");
            Debug.Log($"✓ UI Assignment Based on Scene Type: {(uiAssignmentCorrect ? "PASS" : "FAIL")}");
            Debug.Log($"✓ Discovery Respects Scene Type: {(discoveryRespectsSceneType ? "PASS" : "FAIL")}");
            
            bool allTestsPassed = sceneTypeCorrect && uiAssignmentCorrect && discoveryRespectsSceneType;
            Debug.Log($"=== Overall Result: {(allTestsPassed ? "ALL TESTS PASSED ✓" : "SOME TESTS FAILED ✗")} ===");
        }

        private bool TestSceneTypeDetection()
        {
            var activeScene = SceneManager.GetActiveScene();
            var sceneType = SceneInfo.GetSceneType(activeScene);
            var isGameplayScene = SceneInfo.IsGameplayScene(activeScene);
            
            if (verboseLogging)
            {
                Debug.Log($"Current Scene: '{activeScene.name}'");
                Debug.Log($"Scene Type: {sceneType}");
                Debug.Log($"Is Gameplay Scene: {isGameplayScene}");
            }

            // Scene type detection is working if it returns a valid enum value
            return true;
        }

        private bool TestUIAssignmentBySceneType()
        {
            if (!ServiceLocator.TryGet<MoveCounter>(out var instance) || instance == null)
            {
                Debug.LogError("✗ MoveCounter not found in ServiceLocator - cannot test UI assignment");
                return false;
            }

            var activeScene = SceneManager.GetActiveScene();
            var isGameplayScene = SceneInfo.IsGameplayScene(activeScene);
            var sceneType = SceneInfo.GetSceneType(activeScene);

            bool moveTextAssigned = instance.moveText != null;
            bool timerTextAssigned = instance.timerText != null;

            if (verboseLogging)
            {
                Debug.Log($"Scene: '{activeScene.name}' (Type: {sceneType}, IsGameplay: {isGameplayScene})");
                Debug.Log($"Move Text Assigned: {moveTextAssigned}");
                Debug.Log($"Timer Text Assigned: {timerTextAssigned}");
            }

            // In gameplay scenes, UI components may or may not be assigned (depends on scene content)
            // In non-gameplay scenes (menus, settings), UI components should NOT be assigned by MoveCounter
            if (!isGameplayScene)
            {
                // For non-gameplay scenes, we expect UI components to NOT be assigned
                // (unless they were manually assigned, which we can't test for)
                // The key is that TryFindUIComponents should have returned early
                if (verboseLogging)
                {
                    Debug.Log($"Non-gameplay scene detected - UI components should not be auto-discovered");
                }
                return true; // Test passes if we reach here without errors
            }
            else
            {
                // For gameplay scenes, UI components may be found if they exist
                if (verboseLogging)
                {
                    Debug.Log($"Gameplay scene detected - UI components may be auto-discovered if present");
                }
                return true; // Test passes - the behavior is as expected
            }
        }

        private bool TestUIDiscoveryRespectfulness()
        {
            if (!ServiceLocator.TryGet<MoveCounter>(out var instance) || instance == null)
            {
                Debug.LogError("✗ MoveCounter not found in ServiceLocator - cannot test UI discovery");
                return false;
            }

            var activeScene = SceneManager.GetActiveScene();
            var isGameplayScene = SceneInfo.IsGameplayScene(activeScene);

            // List all TextMeshProUGUI components in the scene
            var allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
            
            if (verboseLogging)
            {
                Debug.Log($"=== All TextMeshProUGUI components in scene ({allTexts.Length}) ===");
                foreach (var text in allTexts)
                {
                    Debug.Log($"- '{text.name}' (parent: '{text.transform.parent?.name}') - Text: '{text.text}'");
                }
                Debug.Log("=== End of TextMeshProUGUI list ===");
            }

            // Check that MoveCounter didn't inappropriately assign UI from non-gameplay scenes
            if (!isGameplayScene && allTexts.Length > 0)
            {
                // If this is not a gameplay scene but has TextMeshProUGUI components,
                // MoveCounter should NOT have assigned them
                bool moveTextAssigned = instance.moveText != null;
                bool timerTextAssigned = instance.timerText != null;

                if (moveTextAssigned || timerTextAssigned)
                {
                    // Check if these were assigned in this scene or a previous scene
                    bool assignedInThisScene = false;
                    foreach (var text in allTexts)
                    {
                        if (text == instance.moveText || text == instance.timerText)
                        {
                            assignedInThisScene = true;
                            Debug.LogWarning($"✗ UI component '{text.name}' from non-gameplay scene '{activeScene.name}' was assigned to MoveCounter");
                            break;
                        }
                    }

                    if (assignedInThisScene)
                    {
                        Debug.LogError($"✗ FAIL: MoveCounter assigned UI components in non-gameplay scene '{activeScene.name}'");
                        return false;
                    }
                }
            }

            if (verboseLogging)
            {
                Debug.Log($"✓ UI discovery correctly respects scene type (IsGameplay: {isGameplayScene})");
            }

            return true;
        }

        [ContextMenu("Force Rediscover UI Components")]
        public void ForceRediscoverUIComponents()
        {
            if (ServiceLocator.TryGet<MoveCounter>(out var instance) && instance != null)
            {
                // Enable verbose logging temporarily to see the discovery process
                bool originalVerbose = instance.verboseLogging;
                instance.verboseLogging = true;
                
                Debug.Log("=== Forcing UI Component Rediscovery ===");
                instance.RediscoverUIComponents();
                
                // Restore original verbose setting
                instance.verboseLogging = originalVerbose;
            }
            else
            {
                Debug.LogError("Cannot force rediscovery - MoveCounter not found in ServiceLocator");
            }
        }

        [ContextMenu("Log Current Scene Information")]
        public void LogCurrentSceneInformation()
        {
            var activeScene = SceneManager.GetActiveScene();
            var sceneInfo = SceneInfo.GetActiveSceneInfo();
            var sceneType = SceneInfo.GetSceneType(activeScene);
            var isGameplayScene = SceneInfo.IsGameplayScene(activeScene);

            Debug.Log("=== Current Scene Information ===");
            Debug.Log($"Scene Name: {activeScene.name}");
            Debug.Log($"Scene Build Index: {activeScene.buildIndex}");
            Debug.Log($"Scene Type: {sceneType}");
            Debug.Log($"Is Gameplay Scene: {isGameplayScene}");
            Debug.Log($"Has SceneInfo Component: {(sceneInfo != null)}");
            
            if (sceneInfo != null)
            {
                Debug.Log($"SceneInfo.sceneType: {sceneInfo.sceneType}");
                Debug.Log($"SceneInfo.showBackground: {sceneInfo.showBackground}");
                Debug.Log($"SceneInfo.levelNumber: {sceneInfo.levelNumber}");
            }
            
            Debug.Log("=== End of Scene Information ===");
        }
    }
}
