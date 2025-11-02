using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Helper to launch PrefabValidation automatically when running Unity in batch mode
// with the custom argument -runPrefabValidation
[InitializeOnLoad]
public static class BatchPrefabValidationLauncher
{
    static bool ran = false;

    static BatchPrefabValidationLauncher()
    {
        try
        {
            if (!Application.isBatchMode) return;

            var args = Environment.GetCommandLineArgs();
            if (!args.Any(a => string.Equals(a, "-runPrefabValidation", StringComparison.OrdinalIgnoreCase))) return;
            if (ran) return;
            ran = true;

            Debug.Log("BatchPrefabValidationLauncher: detected -runPrefabValidation, executing PrefabValidation...");
            PrefabValidation.RunPrefabValidation();
        }
        catch (Exception ex)
        {
            Debug.LogError("BatchPrefabValidationLauncher exception: " + ex);
            EditorApplication.Exit(2);
        }
    }
}
