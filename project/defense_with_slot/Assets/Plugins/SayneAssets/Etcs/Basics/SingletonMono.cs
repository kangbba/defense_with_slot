using System;
using UnityEngine;

public abstract class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
                throw new Exception($"[{typeof(T)}] instance is null (probably destroyed or not initialized)");

            if (_instance == null) // Unity null 오버로드
                throw new Exception($"[{typeof(T)}] has been destroyed (Unity null)");

            return _instance;
        }
    }
    public static bool HasInstance => _instance != null;

    /// <summary>
    /// 하위 클래스에서 반드시 설정해야 하는 DontDestroyOnLoad 여부
    /// </summary>
    protected abstract bool UseDontDestroyOnLoad { get; }

    /// <summary>
    /// 하위 클래스에서 반드시 구현해야 하는 리소스 해제 로직
    /// </summary>
    protected abstract void Release();

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this as T;

        if (UseDontDestroyOnLoad)
        {
            if (transform.parent != null)
            {
                Debug.LogError($"[SingletonMono<{typeof(T).Name}>] '{gameObject.name}'은 DontDestroyOnLoad 대상이지만 부모가 있습니다. " +
                            $"부모가 존재하면 DontDestroyOnLoad는 무시되므로, 반드시 계층상 최상위로 옮겨야 합니다.");
    #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false; // 에디터에서 바로 정지 (선택)
    #endif
                throw new InvalidOperationException($"{gameObject.name} 은 DontDestroyOnLoad 상태에서 부모가 있을 수 없습니다.");
            }

            DontDestroyOnLoad(gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            try
            {
                Release(); // ✅ 리소스 해제 강제
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{typeof(T)}] Release() 중 예외 발생: {ex}");
            }

            _instance = null;
        }
    }
}
