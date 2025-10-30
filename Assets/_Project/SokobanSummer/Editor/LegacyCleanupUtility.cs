#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EditorTools
{
    /// <summary>
    /// Editor utility to help migrate away from legacy SceneSelector/SceneButton and adopt DynamicLevel system.
    /// </summary>
    public static class LegacyCleanupUtility
    {
        [MenuItem("Tools/Dynamic Levels/Scan Legacy References")]
        public static void ScanLegacyReferences()
        {
            var prefabPaths = AssetDatabase.GetAllAssetPaths()
                .Where(p => p.EndsWith(".prefab"))
                .ToList();
            var scenePaths = AssetDatabase.GetAllAssetPaths()
                .Where(p => p.EndsWith(".unity"))
                .ToList();

            var prefabsWithMissing = new List<string>();
            foreach (var path in prefabPaths)
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go == null) continue;
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go) > 0)
                {
                    prefabsWithMissing.Add(path);
                }
            }

            var scenesWithMissing = new List<string>();
            foreach (var path in scenePaths)
            {
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                var roots = scene.GetRootGameObjects();
                var missing = roots.Sum(r => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(r));
                if (missing > 0)
                {
                    scenesWithMissing.Add(path);
                }
                EditorSceneManager.CloseScene(scene, true);
            }

            Debug.Log($"[LegacyCleanup] Prefabs with missing scripts: {prefabsWithMissing.Count}\n" + string.Join("\n  • ", prefabsWithMissing.Select(s => s)));
            Debug.Log($"[LegacyCleanup] Scenes with missing scripts: {scenesWithMissing.Count}\n" + string.Join("\n  • ", scenesWithMissing.Select(s => s)));

            // Quick check for assets likely to be old level buttons
            var candidateButtons = prefabPaths.Where(p => Path.GetFileNameWithoutExtension(p).ToLower().Contains("levelbutton")
                                                          || p.ToLower().Contains("prefabs/ui")).ToList();
            Debug.Log($"[LegacyCleanup] Candidate level button prefabs: {candidateButtons.Count}\n" + string.Join("\n  • ", candidateButtons));
        }

        [MenuItem("Tools/Dynamic Levels/Auto-Convert Level Button Prefabs")]
        public static void AutoConvertLevelButtons()
        {
            var prefabPaths = AssetDatabase.GetAllAssetPaths()
                .Where(p => p.EndsWith(".prefab"))
                .ToList();

            var converted = 0;
            foreach (var path in prefabPaths)
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go == null) continue;

                // Only convert button-like prefabs (have Button component at root)
                var button = go.GetComponent<Button>();
                if (button == null) continue;

                // Ensure DynamicLevelButton component exists
                var dyn = go.GetComponent<UI.DynamicLevelButton>();
                if (dyn == null)
                {
                    dyn = go.AddComponent<UI.DynamicLevelButton>();
                }

                // Wire references if empty using naming conventions
                if (dyn.button == null) dyn.button = button;

                if (dyn.levelNameText == null)
                {
                    var nameText = go.GetComponentsInChildren<TextMeshProUGUI>(true)
                        .FirstOrDefault(t => t.name.ToLower().Contains("level") || t.name.ToLower().Contains("name"));
                    dyn.levelNameText = nameText;
                }

                if (dyn.goalText == null)
                {
                    var goalText = go.GetComponentsInChildren<TextMeshProUGUI>(true)
                        .FirstOrDefault(t => t.name.ToLower().Contains("goal") || t.name.ToLower().Contains("par"));
                    dyn.goalText = goalText;
                }

                if (dyn.previewImage == null)
                {
                    var preview = go.GetComponentsInChildren<Image>(true)
                        .FirstOrDefault(i => i.name.ToLower().Contains("preview") || i.name.ToLower().Contains("thumb"));
                    dyn.previewImage = preview;
                }

                if (dyn.lockOverlay == null)
                {
                    var lockObj = go.GetComponentsInChildren<Transform>(true)
                        .FirstOrDefault(t => t.name.ToLower().Contains("lock"));
                    dyn.lockOverlay = lockObj != null ? lockObj.gameObject : null;
                }

                PrefabUtility.SavePrefabAsset(go);
                converted++;
            }

            Debug.Log($"[LegacyCleanup] Converted {converted} button prefabs to use DynamicLevelButton.");
        }
    }
}
#endif
