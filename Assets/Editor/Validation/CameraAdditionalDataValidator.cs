#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Scans target scenes for Cameras missing the render-pipeline-specific
// "Additional Camera Data" component and offers an auto-fix.
//
// Menu:
// - Tools/Validation/Cameras/Scan Missing Additional Camera Data (Dry Run)
// - Tools/Validation/Cameras/Auto Add Additional Camera Data (Apply)
public static class CameraAdditionalDataValidator
{
    // Keep in sync with other validators; expand if needed.
    static readonly string[] ScenesToCheck = new[]
    {
        "Assets/_Project/SokobanSummer/Scenes/Main Menu.unity",
        "Assets/_Project/SokobanSummer/Scenes/Game.unity",
    };

    [MenuItem("Tools/Validation/Cameras/Scan Missing Additional Camera Data (Dry Run)")]
    public static void ScanDryRun() => Run(apply: false);

    [MenuItem("Tools/Validation/Cameras/Auto Add Additional Camera Data (Apply)")]
    public static void ScanApply() => Run(apply: true);

    // Build Settings scenes
    [MenuItem("Tools/Validation/Cameras/Scan Build Settings Scenes (Dry Run)")]
    public static void ScanBuildSettingsDry() => RunForBuildSettingsScenes(apply: false);

    [MenuItem("Tools/Validation/Cameras/Auto Add for Build Settings Scenes (Apply)")]
    public static void ScanBuildSettingsApply() => RunForBuildSettingsScenes(apply: true);

    // All prefabs in project
    [MenuItem("Tools/Validation/Cameras/Scan All Prefabs (Dry Run)")]
    public static void ScanAllPrefabsDry() => RunForAllPrefabs(apply: false);

    [MenuItem("Tools/Validation/Cameras/Auto Add for All Prefabs (Apply)")]
    public static void ScanAllPrefabsApply() => RunForAllPrefabs(apply: true);

    // Batch entry points
    public static void Batch_ScanBuildScenes_Dry() => RunForBuildSettingsScenes(false);
    public static void Batch_ScanBuildScenes_Apply() => RunForBuildSettingsScenes(true);
    public static void Batch_ScanAllPrefabs_Dry() => RunForAllPrefabs(false);
    public static void Batch_ScanAllPrefabs_Apply() => RunForAllPrefabs(true);

    public static void Run(bool apply)
    {
        var report = new List<string>();

#if UNITY_RENDER_PIPELINE_UNIVERSAL
        report.Add("Pipeline: URP");
        foreach (var scenePath in ScenesToCheck)
        {
            if (!System.IO.File.Exists(scenePath))
            {
                report.Add($"[WARN] Scene not found: {scenePath}");
                continue;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var cams = GameObject.FindObjectsOfType<Camera>(true);
            foreach (var cam in cams)
            {
                if (cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>() == null)
                {
                    report.Add($"[MISSING] {scenePath} -> Camera '{cam.name}' is missing URP 'Universal Additional Camera Data'.");
                    if (apply)
                    {
                        cam.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                        EditorSceneManager.MarkSceneDirty(scene);
                        report.Add($"[APPLY] Added to '{cam.name}'.");
                    }
                }
            }
            if (apply)
            {
                EditorSceneManager.SaveScene(scene);
            }
        }
#elif UNITY_RENDER_PIPELINE_HDRP
        report.Add("Pipeline: HDRP");
        foreach (var scenePath in ScenesToCheck)
        {
            if (!System.IO.File.Exists(scenePath))
            {
                report.Add($"[WARN] Scene not found: {scenePath}");
                continue;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var cams = GameObject.FindObjectsOfType<Camera>(true);
            foreach (var cam in cams)
            {
                if (cam.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>() == null)
                {
                    report.Add($"[MISSING] {scenePath} -> Camera '{cam.name}' is missing HDRP 'HD Additional Camera Data'.");
                    if (apply)
                    {
                        cam.gameObject.AddComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
                        EditorSceneManager.MarkSceneDirty(scene);
                        report.Add($"[APPLY] Added to '{cam.name}'.");
                    }
                }
            }
            if (apply)
            {
                EditorSceneManager.SaveScene(scene);
            }
        }
#else
        report.Add("Pipeline: Built-in (no Additional Camera Data required). Nothing to scan.");
#endif

        Debug.Log("CameraAdditionalDataValidator report:\n" + string.Join("\n", report));
    }

    static void RunForBuildSettingsScenes(bool apply)
    {
        var report = new List<string>();

        var scenes = EditorBuildSettings.scenes;
        if (scenes == null || scenes.Length == 0)
        {
            Debug.Log("CameraAdditionalDataValidator: No scenes in Build Settings.");
            return;
        }

#if UNITY_RENDER_PIPELINE_UNIVERSAL
        report.Add("Pipeline: URP");
        foreach (var s in scenes)
        {
            if (!s.enabled) continue;
            var path = s.path;
            if (!System.IO.File.Exists(path)) { report.Add($"[WARN] Scene not found: {path}"); continue; }
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var cams = GameObject.FindObjectsOfType<Camera>(true);
            foreach (var cam in cams)
            {
                if (cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>() == null)
                {
                    report.Add($"[MISSING] {path} -> Camera '{cam.name}' is missing URP 'Universal Additional Camera Data'.");
                    if (apply)
                    {
                        cam.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                        EditorSceneManager.MarkSceneDirty(scene);
                        report.Add($"[APPLY] Added to '{cam.name}'.");
                    }
                }
            }
            if (apply) EditorSceneManager.SaveScene(scene);
        }
#elif UNITY_RENDER_PIPELINE_HDRP
        report.Add("Pipeline: HDRP");
        foreach (var s in scenes)
        {
            if (!s.enabled) continue;
            var path = s.path;
            if (!System.IO.File.Exists(path)) { report.Add($"[WARN] Scene not found: {path}"); continue; }
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var cams = GameObject.FindObjectsOfType<Camera>(true);
            foreach (var cam in cams)
            {
                if (cam.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>() == null)
                {
                    report.Add($"[MISSING] {path} -> Camera '{cam.name}' is missing HDRP 'HD Additional Camera Data'.");
                    if (apply)
                    {
                        cam.gameObject.AddComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
                        EditorSceneManager.MarkSceneDirty(scene);
                        report.Add($"[APPLY] Added to '{cam.name}'.");
                    }
                }
            }
            if (apply) EditorSceneManager.SaveScene(scene);
        }
#else
        report.Add("Pipeline: Built-in (no Additional Camera Data required). Nothing to scan.");
#endif

        Debug.Log("CameraAdditionalDataValidator (Build Settings) report:\n" + string.Join("\n", report));
    }

    static void RunForAllPrefabs(bool apply)
    {
        var report = new List<string>();
        var guids = AssetDatabase.FindAssets("t:Prefab");
        if (guids == null || guids.Length == 0)
        {
            Debug.Log("CameraAdditionalDataValidator: No prefabs found.");
            return;
        }

#if UNITY_RENDER_PIPELINE_UNIVERSAL
        report.Add("Pipeline: URP");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) continue;
            var root = PrefabUtility.LoadPrefabContents(path);
            bool changed = false;
            var cams = root.GetComponentsInChildren<Camera>(true);
            foreach (var cam in cams)
            {
                if (cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>() == null)
                {
                    report.Add($"[MISSING] Prefab:{path} -> Camera '{cam.name}' is missing URP 'Universal Additional Camera Data'.");
                    if (apply)
                    {
                        cam.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                        changed = true;
                        report.Add($"[APPLY] Added to '{cam.name}'.");
                    }
                }
            }
            if (apply && changed)
            {
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            PrefabUtility.UnloadPrefabContents(root);
        }
#elif UNITY_RENDER_PIPELINE_HDRP
        report.Add("Pipeline: HDRP");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) continue;
            var root = PrefabUtility.LoadPrefabContents(path);
            bool changed = false;
            var cams = root.GetComponentsInChildren<Camera>(true);
            foreach (var cam in cams)
            {
                if (cam.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>() == null)
                {
                    report.Add($"[MISSING] Prefab:{path} -> Camera '{cam.name}' is missing HDRP 'HD Additional Camera Data'.");
                    if (apply)
                    {
                        cam.gameObject.AddComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
                        changed = true;
                        report.Add($"[APPLY] Added to '{cam.name}'.");
                    }
                }
            }
            if (apply && changed)
            {
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            PrefabUtility.UnloadPrefabContents(root);
        }
#else
        report.Add("Pipeline: Built-in (no Additional Camera Data required). Nothing to scan.");
#endif

        Debug.Log("CameraAdditionalDataValidator (All Prefabs) report:\n" + string.Join("\n", report));
    }
}
#endif
