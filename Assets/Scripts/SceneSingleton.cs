using UnityEngine;

/// <summary>
/// 씬 전환 시 파괴되는 MonoBehaviour 기반 싱글톤 클래스
/// </summary>
public class SceneSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<T>();

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
        }
        // 이미 인스턴스가 존재하면 새 인스턴스는 파괴
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }
}
