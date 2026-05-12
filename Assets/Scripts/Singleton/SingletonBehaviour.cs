using UnityEngine;

public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
{
    private static T _instance;

    public static T Singleton
    {
        get
        {
            if (_instance != null) return _instance;
            _instance = FindAnyObjectByType<T>();
            if (_instance != null) return _instance;
            var go = new GameObject(typeof(T).Name);
            _instance = go.AddComponent<T>();
            DontDestroyOnLoad(go);
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null) _instance = (T)this;
        else if (_instance != this) { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);
        OnAwake();
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    protected virtual void OnAwake() { }
}
