using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
public enum GamePhase
{
    None   = -1, // 🚨 시작 전 상태
    Main   = 0,
    Battle = 1,
    Result = 2,
}

public class GamePhaseManager : SingletonMono<GamePhaseManager>
{
    private readonly Dictionary<GamePhase, GamePhaseBase> _phases = new();
    private GamePhaseBase _currentPhase;

    protected override bool UseDontDestroyOnLoad => true;
    protected override void Release() { }

    private void Start()
    {
        // ✅ Phase 등록
        RegisterPhase(new MainPhase());
        RegisterPhase(new BattlePhase());
        RegisterPhase(new ResultPhase());

        // 🚨 첫 Phase는 None → 이후 MainPhase로 넘어감
        ChangePhase(GamePhase.None).Forget();
    }

    private void RegisterPhase(GamePhaseBase phase)
    {
        if (!_phases.ContainsKey(phase.Phase))
            _phases[phase.Phase] = phase;
    }

    public async UniTaskVoid ChangePhase(GamePhase newPhase)
    {
        if (_currentPhase != null)
            _currentPhase.OnExit();

        if (newPhase == GamePhase.None)
        {
            Debug.Log("[GamePhaseManager] None 상태 진입 → MainPhase 준비중...");
            await UniTask.Yield(); // 한 프레임 대기
            ChangePhase(GamePhase.Main).Forget();
            return;
        }

        if (!_phases.TryGetValue(newPhase, out var phase))
        {
            Debug.LogError($"[GamePhaseManager] {newPhase} Phase 등록 안됨");
            return;
        }

        _currentPhase = phase;
        _currentPhase.OnEnter();

        // ✅ Phase 고유 루프 실행
        var nextPhase = await _currentPhase.RunAsync();
        ChangePhase(nextPhase).Forget();
    }
}
