using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UI;
using TMPro;
using UnityEngine.UI;

namespace Editor
{
    /// <summary>
    /// Validation tool to check HUD structure and report issues with Static/Dynamic canvas split.
    /// Accessible via Tools > Sokoban Summer > Validate HUD Structure
    /// </summary>
    public static class HudValidationTool
    {
        private class ValidationResult
        {
            public bool IsValid = true;
            public List<string> Errors = new List<string>();
            public List<string> Warnings = new List<string>();
            public List<string> Info = new List<string>();
        }

        [MenuItem("Tools/Sokoban Summer/Validate HUD Structure")]
        public static void ValidateCurrentScene()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid() || string.IsNullOrEmpty(activeScene.path))
            {
                EditorUtility.DisplayDialog("HUD Validation", "No valid scene loaded. Please open a gameplay scene first.", "OK");
                return;
            }

            Debug.Log($"[HudValidationTool] Validating scene: {activeScene.name}");
            var result = new ValidationResult();

            // Step 1: Check for HudStructureInitializer
            var initializer = Object.FindFirstObjectByType<HudStructureInitializer>();
            if (initializer == null)
            {
                result.IsValid = false;
                result.Errors.Add("No HudStructureInitializer found in scene. Run migration tool first.");
            }
            else
            {
                result.Info.Add($"✓ HudStructureInitializer found on '{initializer.name}'");

                // Step 2: Check canvases exist
                Canvas staticCanvas = FindCanvasByName("StaticHUD");
                Canvas dynamicCanvas = FindCanvasByName("DynamicHUD");

                if (staticCanvas == null)
                {
                    result.IsValid = false;
                    result.Errors.Add("StaticHUD canvas not found. Ensure HudStructureInitializer has run.");
                }
                else
                {
                    result.Info.Add($"✓ StaticHUD canvas found (SortOrder: {staticCanvas.sortingOrder})");
                    ValidateCanvas(staticCanvas, result, isDynamic: false);
                }

                if (dynamicCanvas == null)
                {
                    result.IsValid = false;
                    result.Errors.Add("DynamicHUD canvas not found. Ensure HudStructureInitializer has run.");
                }
                else
                {
                    result.Info.Add($"✓ DynamicHUD canvas found (SortOrder: {dynamicCanvas.sortingOrder})");
                    ValidateCanvas(dynamicCanvas, result, isDynamic: true);
                }

                // Step 3: Check for profilers
                if (staticCanvas != null)
                {
                    var profiler = staticCanvas.GetComponent<CanvasRebuildProfiler>();
                    if (profiler == null)
                    {
                        result.Warnings.Add("StaticHUD missing CanvasRebuildProfiler (optional but recommended for profiling)");
                    }
                    else
                    {
                        result.Info.Add("✓ StaticHUD has CanvasRebuildProfiler");
                    }
                }

                if (dynamicCanvas != null)
                {
                    var profiler = dynamicCanvas.GetComponent<CanvasRebuildProfiler>();
                    if (profiler == null)
                    {
                        result.Warnings.Add("DynamicHUD missing CanvasRebuildProfiler (optional but recommended for profiling)");
                    }
                    else
                    {
                        result.Info.Add("✓ DynamicHUD has CanvasRebuildProfiler");
                    }
                }

                // Step 4: Check for misplaced elements
                CheckForMisplacedElements(staticCanvas, dynamicCanvas, result);
            }

            // Display results
            DisplayValidationResults(result);
        }

        private static void ValidateCanvas(Canvas canvas, ValidationResult result, bool isDynamic)
        {
            if (canvas == null) return;

            // Check render mode
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                result.Warnings.Add($"{canvas.name}: Non-overlay render mode detected ({canvas.renderMode}). May impact performance.");
            }

            // Check GraphicRaycaster
            var raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                result.Warnings.Add($"{canvas.name}: Missing GraphicRaycaster (UI events won't work)");
            }

            // Count graphics and check raycast targets
            var graphics = canvas.GetComponentsInChildren<Graphic>(true);
            int raycastEnabledCount = 0;
            int selectableCount = 0;

            foreach (var g in graphics)
            {
                if (g == null) continue;
                
                if (g.raycastTarget)
                {
                    raycastEnabledCount++;
                    
                    // Check if it's part of an interactive control
                    bool hasSelectable = g.GetComponent<Selectable>() != null || g.GetComponentInParent<Selectable>() != null;
                    if (hasSelectable)
                    {
                        selectableCount++;
                    }
                    else if (!isDynamic)
                    {
                        // Static canvas shouldn't have many raycast targets
                        result.Warnings.Add($"{canvas.name}: Non-interactive graphic '{g.name}' has raycastTarget enabled (consider disabling)");
                    }
                }
            }

            result.Info.Add($"{canvas.name}: {graphics.Length} graphics total, {raycastEnabledCount} raycast-enabled, {selectableCount} interactive");

            // Check child count
            int childCount = canvas.transform.childCount;
            if (childCount == 0)
            {
                result.Warnings.Add($"{canvas.name}: Canvas is empty. Consider adding HUD elements.");
            }
            else
            {
                result.Info.Add($"{canvas.name}: {childCount} direct children");
            }
        }

        private static void CheckForMisplacedElements(Canvas staticCanvas, Canvas dynamicCanvas, ValidationResult result)
        {
            if (staticCanvas == null || dynamicCanvas == null) return;

            // Check static canvas for dynamic-looking elements
            var staticElements = staticCanvas.GetComponentsInChildren<Transform>(true);
            foreach (var element in staticElements)
            {
                if (element == staticCanvas.transform) continue;
                
                string nameLower = element.name.ToLower();
                if (nameLower.Contains("move") || nameLower.Contains("timer") || 
                    nameLower.Contains("counter") || nameLower.Contains("achievement"))
                {
                    result.Warnings.Add($"Possible misplaced element in StaticHUD: '{element.name}' (consider moving to DynamicHUD)");
                }
            }

            // Check for TextMeshProUGUI that might be frequently updated
            var staticTexts = staticCanvas.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in staticTexts)
            {
                string nameLower = text.name.ToLower();
                if (nameLower.Contains("move") || nameLower.Contains("timer") || nameLower.Contains("time"))
                {
                    result.Warnings.Add($"Text component '{text.name}' in StaticHUD may be frequently updated (consider DynamicHUD)");
                }
            }

            // Check dynamic canvas for static-looking elements
            var dynamicElements = dynamicCanvas.GetComponentsInChildren<Transform>(true);
            int dynamicChildCount = dynamicElements.Length - 1; // Exclude canvas itself
            
            if (dynamicChildCount == 0)
            {
                result.Warnings.Add("DynamicHUD is empty. Add move counter, timer, or other dynamic UI here.");
            }

            result.Info.Add($"Element placement check complete.");
        }

        private static Canvas FindCanvasByName(string name)
        {
            return Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)
                .FirstOrDefault(c => c != null && c.gameObject.scene.IsValid() && c.name == name);
        }

        private static void DisplayValidationResults(ValidationResult result)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== HUD Validation Results ===\n");

            if (result.IsValid)
            {
                sb.AppendLine("✓ VALIDATION PASSED\n");
            }
            else
            {
                sb.AppendLine("✗ VALIDATION FAILED\n");
            }

            if (result.Errors.Count > 0)
            {
                sb.AppendLine("ERRORS:");
                foreach (var error in result.Errors)
                {
                    sb.AppendLine($"  ✗ {error}");
                }
                sb.AppendLine();
            }

            if (result.Warnings.Count > 0)
            {
                sb.AppendLine("WARNINGS:");
                foreach (var warning in result.Warnings)
                {
                    sb.AppendLine($"  ⚠ {warning}");
                }
                sb.AppendLine();
            }

            if (result.Info.Count > 0)
            {
                sb.AppendLine("INFO:");
                foreach (var info in result.Info)
                {
                    sb.AppendLine($"  {info}");
                }
            }

            string message = sb.ToString();
            Debug.Log($"[HudValidationTool]\n{message}");

            // Show dialog with summary
            string dialogMessage = result.IsValid 
                ? $"Validation passed!\n\n{result.Warnings.Count} warnings\n{result.Info.Count} info items\n\nSee console for details."
                : $"Validation failed!\n\n{result.Errors.Count} errors\n{result.Warnings.Count} warnings\n\nSee console for details.";
            
            EditorUtility.DisplayDialog("HUD Validation", dialogMessage, "OK");
        }

        [MenuItem("Tools/Sokoban Summer/Validate HUD Structure", true)]
        public static bool ValidateMenuEnabled()
        {
            return SceneManager.GetActiveScene().IsValid();
        }
    }
}
