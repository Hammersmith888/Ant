using UnityEngine;

public class MB_Singleton<T> : MonoBehaviour where T : MB_Singleton<T>
{
    public static T Instance { get; private set; }
    private void OnDestroy()
    {
        Instance = null;
    }
    protected virtual void Awake()
    {
        Instance = Tools.SingletonPattern((T)this, Instance);
    }
}

