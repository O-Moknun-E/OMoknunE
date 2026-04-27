using UnityEngine;

/// <summary>
/// MonoBehaviour 기반 싱글톤 클래스
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<T>();

                if (_instance == null)
                {
                    GameObject obj = new GameObject(typeof(T).Name);

                    _instance = obj.AddComponent<T>();
                    DontDestroyOnLoad(obj);
                }
            }

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if(_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        // 이미 인스턴스가 존재하면 새 인스턴스는 파괴
        else if(_instance != this)
        {
            Destroy(gameObject);
        }
    }
}
