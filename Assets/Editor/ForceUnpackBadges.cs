using UnityEngine;
using UnityEditor;

public class ForceUnpackBadges
{
    public static void Execute()
    {
        string[] paths = {
            "Menu Canvas/Achieverment Text/Badges Container/Tutorial",
            "Menu Canvas/Achieverment Text/Badges Container/SpeedAndConfuse",
            "Menu Canvas/Achieverment Text/CompleteLevelTwo",
            "Menu Canvas/Achieverment Text/Badges Container/CompleteLevelTwo" // Just in case
        };

        foreach (string path in paths)
        {
            GameObject go = GameObject.Find(path);
            if (go != null && PrefabUtility.IsPartOfPrefabInstance(go))
            {
                PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                Debug.Log("Unpacked " + path);
            }
        }
    }
}
