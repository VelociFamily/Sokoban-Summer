using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FixGrassAnimators
{
    public static void Execute()
    {
        Debug.Log("Starting FixGrassAnimators...");
        
        // Find the root object
        var grassRoot = GameObject.Find("Sprites/gROUND");
        if (grassRoot == null)
        {
            Debug.LogError("Could not find 'Sprites/gROUND' object!");
            return;
        }

        // Get all animators in children
        var animators = grassRoot.GetComponentsInChildren<Animator>(true);
        Debug.Log($"Found {animators.Length} Animators to remove.");

        int removedCount = 0;
        foreach (var anim in animators)
        {
            // Verify it's on a grass object (sanity check)
            if (anim.gameObject.name.Contains("Grass"))
            {
                Debug.Log($"Removing Animator from '{anim.gameObject.name}'");
                Object.DestroyImmediate(anim);
                removedCount++;
            }
            else
            {
                Debug.LogWarning($"Skipping Animator on '{anim.gameObject.name}' - name does not contain 'Grass'");
            }
        }

        if (removedCount > 0)
        {
            Debug.Log($"Successfully removed {removedCount} Animators.");
            // Mark scene as dirty to ensure save
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(grassRoot.scene);
        }
        else
        {
            Debug.Log("No Animators found on Grass objects.");
        }
    }
}
