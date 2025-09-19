using UnityEngine;

public class SingletonPersist : MonoBehaviour
{
    public static SingletonPersist Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"[SingletonPersist]: Instance created on '{gameObject.name}'");
        }
        else
        {
            Debug.LogWarning($"[SingletonPersist]: Duplicate instance detected on '{gameObject.name}' - destroying");
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Debug.Log("[SingletonPersist]: Primary instance destroyed");
            Instance = null;
        }
    }
}
