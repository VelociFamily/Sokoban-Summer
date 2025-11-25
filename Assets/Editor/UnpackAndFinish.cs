using UnityEngine;
using UnityEditor;

public class UnpackAndFinish
{
    public static void Execute()
    {
        string rootPath = "Menu Canvas/Achieverment Text";
        GameObject root = GameObject.Find(rootPath);
        if (root == null) { Debug.LogError("Root not found"); return; }

        // Check if it's a prefab instance
        if (PrefabUtility.IsPartOfPrefabInstance(root))
        {
            GameObject outermost = PrefabUtility.GetOutermostPrefabInstanceRoot(root);
            if (outermost != null)
            {
                PrefabUtility.UnpackPrefabInstance(outermost, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                Debug.Log("Unpacked prefab: " + outermost.name);
            }
        }

        // Now run the organization logic
        FinishOrganizingAchievements.Execute();
    }
}
