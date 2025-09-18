using System.Collections.Generic;
using UnityEngine;

public class Ifpressedddelete : MonoBehaviour
{
    public ParticleSystem smokeexplosion;
    public List<GameObject> objectsToDestroy = new List<GameObject>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            DestroyAllObjects();
        }
        smokeexplosion.Play();
    }
    public void DestroyAllObjects()
    {
        foreach (var obj in objectsToDestroy)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        // Optionally clear the list afterwards
        objectsToDestroy.Clear();
    }
}

