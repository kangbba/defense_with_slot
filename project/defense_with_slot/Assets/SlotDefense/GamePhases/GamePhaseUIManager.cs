using UnityEngine;
using System;
using System.Collections.Generic;
using UniRx;

[Serializable]
public struct PhaseUIEntry
{
    public GamePhase Phase;
    public List<PhaseUI> UIs;
}

public class GamePhaseUIManager : SingletonMono<GamePhaseUIManager>
{
    [SerializeField] private List<PhaseUIEntry> _entries = new();

    private readonly ReactiveProperty<GamePhase> _currentPhase = new(GamePhase.Main);
    public IReadOnlyReactiveProperty<GamePhase> CurrentPhase => _currentPhase;

    protected override bool UseDontDestroyOnLoad => false;
    protected override void Release() { }

    // ==============================
    // Phase 전환 (상태만 바꾼다)
    // ==============================
    public void SetPhase(GamePhase newPhase)
    {
        _currentPhase.Value = newPhase;
    }

    // ==============================
    // 특정 Phase의 UI 찾기
    // ==============================
    public IReadOnlyList<PhaseUI> GetUIs(GamePhase phase)
    {
        foreach (var entry in _entries)
        {
            if (entry.Phase == phase) return entry.UIs;
        }
        return Array.Empty<PhaseUI>();
    }

    // ==============================
    // 초기 구독 설정
    // ==============================
    private void Start()
    {
        _currentPhase
            .StartWith(_currentPhase.Value) // ✅ 첫 시작 상태 포함
            .Subscribe(OnPhaseChanged)
            .AddTo(this);
    }

    private void OnPhaseChanged(GamePhase phase)
    {
        foreach (var entry in _entries)
        {
            bool isActivePhase = entry.Phase == phase;
            foreach (var ui in entry.UIs)
            {
                if (isActivePhase) ui.Show();
                else ui.Hide();
            }
        }
    }
}
