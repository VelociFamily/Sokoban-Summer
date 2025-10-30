using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public class ObjectDestroyer : MonoBehaviour
    {
        public ParticleSystem smokeexplosion;
        public List<GameObject> objectsToDestroy = new List<GameObject>();

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (objectsToDestroy.Count > 0)
                    Debug.Log($"[ObjectDestroyer]: Player triggered destruction of {objectsToDestroy.Count} objects");
            
                if (smokeexplosion != null)
                    smokeexplosion.Play();
                else
                    Debug.LogWarning("[ObjectDestroyer]: Smoke explosion effect not assigned - visual feedback disabled");
                
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
                    Destroy(obj);
                    destroyedCount++;
                }
            }
        
            if (destroyedCount > 0)
                Debug.Log($"[ObjectDestroyer]: Successfully destroyed {destroyedCount} objects");

            // Clear the list afterwards
            objectsToDestroy.Clear();
        }
    }
}

