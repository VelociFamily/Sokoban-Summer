using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Lightweight validation utility to run in batch mode.
// Usage: Unity -batchmode -projectPath <path> -executeMethod PrefabValidation.RunPrefabValidation
public static class PrefabValidation
{
    // Scenes and prefabs we want to validate as part of the pilot move
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

    // Names of singleton GameObjects to check for duplicates
    static readonly string[] SingletonNames = new[] { "MusicPlayer", "Background" };

    public static void RunPrefabValidation()
    {
        try
        {
            Debug.Log("PrefabValidation: starting validation...");
            var issues = new List<string>();

            // Validate scenes
            foreach (var scenePath in ScenesToCheck)
            {
                if (!System.IO.File.Exists(scenePath))
                {
                    Debug.LogWarning($"Scene not found: {scenePath}");
                    continue;
                }

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                Debug.Log($"Opened scene: {scenePath}");

                // missing scripts (component == null)
                foreach (var root in scene.GetRootGameObjects())
                {
                    var components = root.GetComponentsInChildren<Component>(true);
                    foreach (var c in components)
                    {
                        if (c == null)
                        {
                            issues.Add($"Missing script in scene {scenePath} on GameObject '{root.name}'");
                        }
                        else
                        {
                            try
                            {
                                var so = new SerializedObject(c);
                                var prop = so.GetIterator();
                                if (prop.NextVisible(true))
                                {
                                    while (prop.NextVisible(false))
                                    {
                                        if (prop.propertyType == SerializedPropertyType.ObjectReference)
                                        {
                                            if (prop.objectReferenceValue == null && prop.objectReferenceInstanceIDValue != 0)
                                            {
                                                issues.Add($"Missing reference in scene {scenePath} on GameObject '{root.name}', Component '{c.GetType().Name}', field '{prop.name}'");
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning($"Exception inspecting component {c.GetType().Name}: {ex.Message}");
                            }
                        }
                    }
                }

                // duplicate singletons by name
                foreach (var sname in SingletonNames)
                {
                    var found = GameObject.FindObjectsOfType<GameObject>();
                    int count = 0;
                    foreach (var go in found)
                    {
                        if (go.name == sname) count++;
                    }
                    if (count > 1)
                    {
                        issues.Add($"Duplicate singleton name '{sname}' found {count} times in scene {scenePath}");
                    }
                }
            }

            // Validate prefabs
            foreach (var prefabPath in PrefabsToCheck)
            {
                if (!System.IO.File.Exists(prefabPath))
                {
                    Debug.LogWarning($"Prefab not found: {prefabPath}");
                    continue;
                }

                var root = PrefabUtility.LoadPrefabContents(prefabPath);
                Debug.Log($"Loaded prefab: {prefabPath}");

                var components = root.GetComponentsInChildren<Component>(true);
                foreach (var c in components)
                {
                    if (c == null)
                    {
                        issues.Add($"Missing script in prefab {prefabPath} on root '{root.name}'");
                        continue;
                    }

                    try
                    {
                        var so = new SerializedObject(c);
                        var prop = so.GetIterator();
                        if (prop.NextVisible(true))
                        {
                            while (prop.NextVisible(false))
                            {
                                if (prop.propertyType == SerializedPropertyType.ObjectReference)
                                {
                                    if (prop.objectReferenceValue == null && prop.objectReferenceInstanceIDValue != 0)
                                    {
                                        issues.Add($"Missing reference in prefab {prefabPath} on Component '{c.GetType().Name}', field '{prop.name}'");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Exception inspecting component {c.GetType().Name} in prefab {prefabPath}: {ex.Message}");
                    }
                }

                PrefabUtility.UnloadPrefabContents(root);
            }

            // Print summary
            if (issues.Count == 0)
            {
                Debug.Log("PrefabValidation: SUCCESS — no missing references or duplicate singletons found in checked scenes/prefabs.");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError($"PrefabValidation: {issues.Count} issue(s) found:");
                foreach (var it in issues)
                {
                    Debug.LogError(it);
                }
                EditorApplication.Exit(1);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("PrefabValidation: unexpected exception: " + ex);
            EditorApplication.Exit(2);
        }
    }
}
