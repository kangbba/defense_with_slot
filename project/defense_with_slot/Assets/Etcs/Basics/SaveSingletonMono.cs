using System;
using UnityEngine;

/// <summary>
/// SaveData 의존 매니저용 베이스.
/// - 기존 SingletonMono 스타일의 수명주기(UseDontDestroyOnLoad, Release, Awake/OnDestroy) 그대로 유지
/// - 외부에서 BindSaveData(save)를 호출하면 base가 내부 current를 할당하고, OnSaveDataBound(save)를 자동 호출
/// - 파생 클래스는 base.OnSaveDataBound를 직접 호출할 필요 없음
/// - CurrentSaveData가 미바인딩이면 예외는 던지지 않고 경고 로그만 찍고 null 반환
/// </summary>
public abstract class SaveSingletonMono<T> : SaveSingletonBase where T : MonoBehaviour
{
    // ======= Singleton 영역 (원본 패턴 준수) =======
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
                throw new Exception($"[{typeof(T)}] instance is null (probably destroyed or not initialized)");
            if (_instance == null) // Unity null 오버로드 방지
                throw new Exception($"[{typeof(T)}] has been destroyed (Unity null)");
            return _instance;
        }
    }

    public static bool HasInstance => _instance != null;

    /// <summary>파생에서 필수 지정: 씬 전환 유지 여부</summary>
    protected abstract bool UseDontDestroyOnLoad { get; }

    /// <summary>파생에서 필수 구현: 해제 로직(구독/리소스 정리 등)</summary>
    protected abstract void Release();

}
