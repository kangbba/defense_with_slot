using UnityEngine;
using TMPro;
using UniRx;

public class EnemyCountDisplayer : PhaseUI
{
    [SerializeField] private TextMeshProUGUI _enemyCountText;

    private CompositeDisposable _disposable = new();

    private void Start()
    {
        // EnemyManager가 생길 때까지 대기
        Observable.EveryUpdate()
            .Select(_ => FieldManager.Instance?.CurBattleField?.EnemyManager)
            .Where(mgr => mgr != null)
            .First()
            .Subscribe(mgr =>
            {
                // 생기자마자 구독 시작
                mgr.AliveEnemyCount
                    .Subscribe(count =>
                    {
                        _enemyCountText.text = $"Alive Enemies : {count}";
                    })
                    .AddTo(_disposable);
            })
            .AddTo(this);
    }

    private void OnDisable()
    {
        _disposable.Clear();
    }
}
