using UnityEditor;
using UnityEngine;
using Core;

public static class SavesMenu
{
    [MenuItem("Tools/Saves/Open Saves Folder")]
    public static void OpenSavesFolder()
    {
        var path = System.IO.Path.Combine(Application.persistentDataPath, "Saves");
        System.IO.Directory.CreateDirectory(path);
        EditorUtility.RevealInFinder(path);
    }

    [MenuItem("Tools/Saves/Reset Settings")]
    public static void ResetSettings()
    {
        SaveFacade.Instance.ResetSettings();
        Debug.Log("[SavesMenu] Settings reset");
    }

    [MenuItem("Tools/Saves/Reset Achievements")]
    public static void ResetAchievements()
    {
        SaveFacade.Instance.ResetAchievements();
        Debug.Log("[SavesMenu] Achievements reset");
    }

    [MenuItem("Tools/Saves/Reset Progress")]
    public static void ResetProgress()
    {
        SaveFacade.Instance.ResetProgress();
        Debug.Log("[SavesMenu] Progress reset");
    }

    [MenuItem("Tools/Saves/Reset ALL (incl. migration flag)")]
    public static void ResetAll()
    {
        if (EditorUtility.DisplayDialog("Reset All Saves", "This will delete settings, achievements, progress JSON files and clear the migration flag. Continue?", "Yes", "No"))
        {
            SaveFacade.Instance.ResetAll();
            Debug.Log("[SavesMenu] All saves reset");
        }
    }
}
