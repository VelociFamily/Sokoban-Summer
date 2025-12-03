using UnityEngine;
using UnityEditor;

public class RemoveAnimatorsFromGrass
{
    public static void Execute()
    {
        var grassRoot = GameObject.Find("Sprites/gROUND");
        if (grassRoot == null)
        {
            Debug.LogError("Sprites/gROUND not found");
            return;
        }

        var animators = grassRoot.GetComponentsInChildren<Animator>();
        Debug.Log($"Found {animators.Length} Animators on grass objects. Removing them...");

        foreach (var anim in animators)
        {
            Debug.Log($"Removing Animator from {anim.gameObject.name}");
            Object.DestroyImmediate(anim);
        }
    }
}
