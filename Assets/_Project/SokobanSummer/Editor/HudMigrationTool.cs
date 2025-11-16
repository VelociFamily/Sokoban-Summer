using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UI;
using TMPro;

namespace Editor
{
    /// <summary>
    /// Editor tool to migrate existing HUD elements from a single canvas to StaticHUD and DynamicHUD structure.
    /// Accessible via Tools > Sokoban Summer > Migrate HUD to Static/Dynamic Split
    /// </summary>
    public static class HudMigrationTool
    {
        private static readonly HashSet<string> DynamicElementNames = new HashSet<string>
        {
            "moves", "move", "step", "steps", "counter",
            "timer", "time", "clock",
            "achievement", "notification", "popup"
        };

        [MenuItem("Tools/Sokoban Summer/Migrate HUD to Static/Dynamic Split")]
        public static void MigrateCurrentScene()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid() || string.IsNullOrEmpty(activeScene.path))
            {
                EditorUtility.DisplayDialog("HUD Migration", "No valid scene loaded. Please open a gameplay scene first.", "OK");
                return;
            }

            Debug.Log($"[HudMigrationTool] Starting migration for scene: {activeScene.name}");

            // Step 1: Find or create HudStructureInitializer
            var initializer = Object.FindFirstObjectByType<HudStructureInitializer>();
            GameObject hudRoot;

            if (initializer == null)
            {
                hudRoot = new GameObject("GameplayHUD");
                initializer = hudRoot.AddComponent<HudStructureInitializer>();
                Undo.RegisterCreatedObjectUndo(hudRoot, "Create HUD Root");
                Debug.Log("[HudMigrationTool] Created GameplayHUD with HudStructureInitializer");
            }
            else
            {
                hudRoot = initializer.gameObject;
                Debug.Log("[HudMigrationTool] Found existing HudStructureInitializer");
            }

            // Step 2: Trigger canvas creation (simulate Awake)
            initializer.SendMessage("Awake", SendMessageOptions.DontRequireReceiver);
            
            Canvas staticCanvas = initializer.StaticHudCanvas;
            Canvas dynamicCanvas = initializer.DynamicHudCanvas;

            if (staticCanvas == null || dynamicCanvas == null)
            {
                EditorUtility.DisplayDialog("HUD Migration", "Failed to create canvases. Check console for errors.", "OK");
                return;
            }

            // Step 3: Find all existing canvases with UI elements
            var allCanvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)
                .Where(c => c != staticCanvas && c != dynamicCanvas && c.gameObject.scene == activeScene)
                .ToList();

            int staticCount = 0, dynamicCount = 0;

            foreach (var oldCanvas in allCanvases)
            {
                // Skip if this is a level complete canvas or menu canvas
                if (oldCanvas.name.ToLower().Contains("complete") || 
                    oldCanvas.name.ToLower().Contains("menu") ||
                    oldCanvas.name.ToLower().Contains("pause"))
                {
                    Debug.Log($"[HudMigrationTool] Skipping canvas: {oldCanvas.name}");
                    continue;
                }

                Debug.Log($"[HudMigrationTool] Processing canvas: {oldCanvas.name}");

                // Get all top-level UI elements (direct children)
                var rootElements = new List<Transform>();
                for (int i = 0; i < oldCanvas.transform.childCount; i++)
                {
                    rootElements.Add(oldCanvas.transform.GetChild(i));
                }

                foreach (var element in rootElements)
                {
                    if (element == null) continue;

                    bool isDynamic = IsDynamicElement(element.gameObject);
                    Canvas targetCanvas = isDynamic ? dynamicCanvas : staticCanvas;

                    Undo.SetTransformParent(element, targetCanvas.transform, "Migrate HUD Element");
                    
                    if (isDynamic)
                    {
                        dynamicCount++;
                        Debug.Log($"[HudMigrationTool] → DynamicHUD: {element.name}");
                    }
                    else
                    {
                        staticCount++;
                        Debug.Log($"[HudMigrationTool] → StaticHUD: {element.name}");
                    }
                }

                // Optionally remove old empty canvas
                if (oldCanvas.transform.childCount == 0)
                {
                    Debug.Log($"[HudMigrationTool] Removing empty canvas: {oldCanvas.name}");
                    Undo.DestroyObjectImmediate(oldCanvas.gameObject);
                }
            }

            // Step 4: Add profilers
            if (staticCanvas.GetComponent<CanvasRebuildProfiler>() == null)
            {
                Undo.AddComponent<CanvasRebuildProfiler>(staticCanvas.gameObject);
            }
            if (dynamicCanvas.GetComponent<CanvasRebuildProfiler>() == null)
            {
                Undo.AddComponent<CanvasRebuildProfiler>(dynamicCanvas.gameObject);
            }

            // Mark scene dirty
            EditorSceneManager.MarkSceneDirty(activeScene);

            string message = $"HUD migration complete!\n\n" +
                           $"Static elements: {staticCount}\n" +
                           $"Dynamic elements: {dynamicCount}\n\n" +
                           $"Review the hierarchy and adjust manually if needed.";
            
            EditorUtility.DisplayDialog("HUD Migration", message, "OK");
            Debug.Log($"[HudMigrationTool] Migration complete. Static: {staticCount}, Dynamic: {dynamicCount}");
        }

        private static bool IsDynamicElement(GameObject go)
        {
            if (go == null) return false;

            string nameLower = go.name.ToLower();
            
            // Check name against dynamic keywords
            foreach (var keyword in DynamicElementNames)
            {
                if (nameLower.Contains(keyword))
                    return true;
            }

            // Check if it contains TextMeshProUGUI that might be updated frequently
            var tmpTexts = go.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in tmpTexts)
            {
                string textNameLower = text.name.ToLower();
                foreach (var keyword in DynamicElementNames)
                {
                    if (textNameLower.Contains(keyword))
                        return true;
                }
            }

            return false;
        }

        [MenuItem("Tools/Sokoban Summer/Migrate HUD to Static/Dynamic Split", true)]
        public static bool ValidateMigrateCurrentScene()
        {
            return SceneManager.GetActiveScene().IsValid();
        }
    }
}
