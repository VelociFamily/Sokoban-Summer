using UnityEngine;
using UnityEditor;

public class ForceUnpack
{
    public static void Execute()
    {
        GameObject go = GameObject.Find("Menu Canvas");
        if (go != null && PrefabUtility.IsPartOfPrefabInstance(go))
        {
            PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            Debug.Log("Unpacked Menu Canvas");
        }
        else
        {
            Debug.Log("Menu Canvas not found or not a prefab instance");
        }
        
        // Also check Achieverment Text specifically
        GameObject at = GameObject.Find("Menu Canvas/Achieverment Text");
        if (at != null && PrefabUtility.IsPartOfPrefabInstance(at))
        {
             PrefabUtility.UnpackPrefabInstance(at, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
             Debug.Log("Unpacked Achieverment Text");
        }
    }
}
