// Located at: Assets/Scripts/Core/Singleton.cs
using UnityEngine;

/// <summary>
/// A generic base class for creating singletons.
/// Ensures that only one instance of the class exists.
/// </summary>
/// <typeparam name="T">The type of the singleton class.</typeparam>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T s_instance;
    private static readonly object s_lock = new object();

    /// <summary>
    /// Gets the singleton instance. If it doesn't exist, it will be created.
    /// </summary>
    public static T Instance
    {
        get
        {
            lock (s_lock)
            {
                if (s_instance == null)
                {
                    // Search for an existing instance in the scene using the new API.
                    s_instance = FindAnyObjectByType<T>();

                    // If no instance exists, create a new one.
                    if (s_instance == null)
                    {
                        var singletonObject = new GameObject(typeof(T).Name);
                        s_instance = singletonObject.AddComponent<T>();
                    }
                }
                return s_instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (s_instance != null && s_instance != this)
        {
            // A duplicate instance was found. Destroy it immediately and
            // stop any further execution of this component's script.
            Debug.LogWarning($"[Singleton] A duplicate instance of type '{typeof(T).Name}' was found on GameObject '{this.gameObject.name}'. The duplicate is being destroyed.", this.gameObject);
            Destroy(gameObject);
            return; // **THE FIX**: This prevents OnEnable() and OnDisable() from running on the duplicate.
        }
        else
        {
            s_instance = this as T;
            Debug.Log($"[Singleton] The single instance of type '{typeof(T).Name}' has been set to the one on GameObject '{this.gameObject.name}'. It will not be destroyed on load.", this.gameObject);
            DontDestroyOnLoad(gameObject);
        }
    }
}
