using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Auto-fixer: scans scenes and prefabs for missing object references and attempts
// to re-link them by using the GUID found in the YAML and AssetDatabase.GUIDToAssetPath.
// Provides a dry-run and an apply mode. Intended for small, careful fixes only.
public static class PrefabAutoFixer
{
    // Files the pilot validated. You can expand this list or scan project-wide.
    static readonly string[] ScenesToCheck = new[]
    {
        "Assets/_Project/SokobanSummer/Scenes/Main Menu.unity",
        "Assets/_Project/SokobanSummer/Scenes/Game.unity",
    };

    static readonly string[] PrefabsToCheck = new[]
    {
        "Assets/_Project/SokobanSummer/Prefabs/canvases/Level Complete.prefab",
        "Assets/_Project/SokobanSummer/Prefabs/canvases/Pause Components.prefab",
    };

    [MenuItem("Tools/Prefab Naming/Auto Fix Missing References (Dry Run)")]
    public static void MenuDryRun() => RunAutoFixer(dryRun: true);

    [MenuItem("Tools/Prefab Naming/Auto Fix Missing References (Apply)")]
    public static void MenuApply() => RunAutoFixer(dryRun: false);

    // Batch entry points for -executeMethod
    public static void RunAutoFixerDryRun() => RunAutoFixer(dryRun: true);
    public static void RunAutoFixerApply() => RunAutoFixer(dryRun: false);

    public static void RunAutoFixer(bool dryRun)
    {
        try
        {
            Debug.Log($"PrefabAutoFixer: starting (dryRun={dryRun})...");
            var report = new List<string>();

            // Scenes
            foreach (var scenePath in ScenesToCheck)
            {
                if (!File.Exists(scenePath))
                {
                    report.Add($"Scene not found: {scenePath}");
                    continue;
                }

                var text = File.ReadAllText(scenePath);
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                report.Add($"Opened scene: {scenePath}");

                foreach (var root in scene.GetRootGameObjects())
                {
                    var comps = root.GetComponentsInChildren<Component>(true);
                    foreach (var c in comps)
                    {
                        if (c == null) continue;
                        var so = new SerializedObject(c);
                        var prop = so.GetIterator();
                        while (prop.NextVisible(true))
                        {
                            if (prop.propertyType == SerializedPropertyType.ObjectReference)
                            {
                                if (prop.objectReferenceValue == null && prop.objectReferenceInstanceIDValue != 0)
                                {
                                    // Try to find a GUID in the scene YAML for this property name
                                    var name = prop.name;
                                    var guid = FindGuidForPropertyInText(text, name);
                                    if (!string.IsNullOrEmpty(guid))
                                    {
                                        var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                                        if (!string.IsNullOrEmpty(assetPath))
                                        {
                                            report.Add($"[SUGGEST] Scene:{scenePath} GameObject:{root.name} Component:{c.GetType().Name} Field:{name} -> {assetPath}");
                                            if (!dryRun)
                                            {
                                                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                                                prop.objectReferenceValue = asset;
                                                so.ApplyModifiedProperties();
                                                EditorSceneManager.MarkSceneDirty(scene);
                                                report.Add($"[APPLY] Re-linked field '{name}' on component {c.GetType().Name} (GameObject {root.name}) to {assetPath}");
                                            }
                                        }
                                        else
                                        {
                                            report.Add($"[MISSING-ASSET] GUID {guid} found for property '{name}' but no asset path resolved.");
                                        }
                                    }
                                    else
                                    {
                                        report.Add($"[NO-GUID] Missing reference: Scene:{scenePath} GameObject:{root.name} Component:{c.GetType().Name} Field:{name} (no GUID in YAML)");
                                    }
                                }
                            }
                        }
                    }
                }

                if (!dryRun)
                {
                    EditorSceneManager.SaveScene(scene);
                }
            }

            // Prefabs
            foreach (var prefabPath in PrefabsToCheck)
            {
                if (!File.Exists(prefabPath))
                {
                    report.Add($"Prefab not found: {prefabPath}");
                    continue;
                }

                var text = File.ReadAllText(prefabPath);
                var root = PrefabUtility.LoadPrefabContents(prefabPath);
                var comps = root.GetComponentsInChildren<Component>(true);
                foreach (var c in comps)
                {
                    if (c == null) continue;
                    var so = new SerializedObject(c);
                    var prop = so.GetIterator();
                    while (prop.NextVisible(true))
                    {
                        if (prop.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            if (prop.objectReferenceValue == null && prop.objectReferenceInstanceIDValue != 0)
                            {
                                var name = prop.name;
                                var guid = FindGuidForPropertyInText(text, name);
                                if (!string.IsNullOrEmpty(guid))
                                {
                                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                                    if (!string.IsNullOrEmpty(assetPath))
                                    {
                                        report.Add($"[SUGGEST] Prefab:{prefabPath} Component:{c.GetType().Name} Field:{name} -> {assetPath}");
                                        if (!dryRun)
                                        {
                                            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                                            prop.objectReferenceValue = asset;
                                            so.ApplyModifiedProperties();
                                            report.Add($"[APPLY] Re-linked prefab field '{name}' on component {c.GetType().Name} to {assetPath}");
                                        }
                                    }
                                    else
                                    {
                                        report.Add($"[MISSING-ASSET] GUID {guid} found for prefab property '{name}' but no asset path resolved.");
                                    }
                                }
                                else
                                {
                                    report.Add($"[NO-GUID] Missing reference in prefab {prefabPath} Component:{c.GetType().Name} Field:{name} (no GUID in YAML)");
                                }
                            }
                        }
                    }
                }

                if (!dryRun)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                }
                PrefabUtility.UnloadPrefabContents(root);
            }

            // Print report
            Debug.Log("PrefabAutoFixer report:\n" + string.Join("\n", report));
            if (!dryRun)
            {
                Debug.Log("PrefabAutoFixer: apply run complete.");
            }
            else
            {
                Debug.Log("PrefabAutoFixer: dry-run complete. Review the suggestions above before applying.");
            }

            // Exit cleanly for batch runs
            EditorApplication.Exit(0);
        }
        catch (Exception ex)
        {
            Debug.LogError("PrefabAutoFixer: unexpected exception: " + ex);
            EditorApplication.Exit(2);
        }
    }

        static string FindGuidForPropertyInText(string text, string propertyName)
    {
        // Look for lines like: "  UnifiedAudioManagerPrefab: {fileID: 4684732665795058736, guid: 2fac25f5d6ca84f4b9fb94f468a5c181,"
        var pattern = Regex.Escape(propertyName) + @":\s*\{fileID:\s*[-0-9]+,\s*guid:\s*([0-9a-fA-F]+),";
        var m = Regex.Match(text, pattern);
        if (m.Success && m.Groups.Count > 1)
        {
            return m.Groups[1].Value;
        }
        return null;
    }
}
