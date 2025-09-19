using UnityEngine;

public class SingletonPersist : MonoBehaviour
{
    private static SingletonPersist instance;

    private void Awake()
    {
        var objs = GameObject.FindGameObjectsWithTag(gameObject.tag);
        if (objs.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
}
