using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class GamePhaseBase
{
    /// <summary> Phase 타입 (예: Main, Battle, Result) </summary>
    public abstract GamePhase Phase { get; }

    /// <summary> Phase 진입 시 실행 (UI 연동 포함) </summary>
    public virtual void OnEnter()
    {
        Debug.Log($"[{Phase}] Enter");
        GamePhaseUIManager.Instance?.SetPhase(Phase); // ✅ 공통 UI 연동
    }

    /// <summary> Phase 루프 (조건 검사, 대기 등) </summary>
    public abstract UniTask<GamePhase> RunAsync();

    /// <summary> Phase 종료 시 실행 </summary>
    public virtual void OnExit()
    {
        Debug.Log($"[{Phase}] Exit");
    }
}
