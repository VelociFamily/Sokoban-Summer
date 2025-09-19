using System.Collections.Generic;
using UnityEngine;

public class ObjectDestroyer : MonoBehaviour
{
    public ParticleSystem smokeexplosion;
    public List<GameObject> objectsToDestroy = new List<GameObject>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"ObjectDestroyer: Player triggered destruction of {objectsToDestroy.Count} objects");
            
            if (smokeexplosion != null)
                smokeexplosion.Play();
            else
                Debug.LogWarning("ObjectDestroyer: Smoke explosion effect is not assigned!");
                
            DestroyAllObjects();
        }
    }
    public void DestroyAllObjects()
    {
        var destroyedCount = 0;
        foreach (var obj in objectsToDestroy)
        {
            if (obj != null)
            {
                Debug.Log($"ObjectDestroyer: Destroying object '{obj.name}'");
                Destroy(obj);
                destroyedCount++;
            }
        }
        
        Debug.Log($"ObjectDestroyer: Successfully destroyed {destroyedCount} objects");

        // Optionally clear the list afterwards
        objectsToDestroy.Clear();
    }
}

