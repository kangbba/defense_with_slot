using UnityEngine;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public interface IAttackable
{
    void StartAttackLoop();
    void StopAttackLoop();
}

public enum TargetingMode
{
    NearestEveryTime, // 매 공격마다 가장 가까운 적
    LockOnOne,        // 한 번 잡으면 죽을 때까지 그놈만 (범위 벗어나면 해제)
    RandomInRange     // 사정거리 내에서 무작위
}

[RequireComponent(typeof(Hero))]
public abstract class HeroCombat : MonoBehaviour, IAttackable
{
    [Header("Combat Settings")]
    [SerializeField] protected float attackInterval = 1f;
    [SerializeField] protected float attackRange = 5f;
    [SerializeField] protected TargetingMode targetingMode = TargetingMode.NearestEveryTime;

    protected Hero hero;
    protected Enemy currentTarget;
    private CancellationTokenSource _cts;

    private EnemyManager _enemyManager;

    protected virtual void Awake()
    {
        hero = GetComponent<Hero>();
    }

    public void Init(EnemyManager enemyManager)
    {
        _enemyManager = enemyManager;
    }

    private void OnDisable() => StopAttackLoop();
    private void OnDestroy() => StopAttackLoop();

    public void StartAttackLoop()
    {
        StopAttackLoop();
        _cts = new CancellationTokenSource();
        AttackLoopAsync(_cts.Token).Forget();
    }

    public void StopAttackLoop()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    private async UniTask AttackLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            // ✅ 드래그 중이면 공격 보류
            if (hero.IsDragging.Value)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(attackInterval), cancellationToken: token);
                continue;
            }

            Enemy target = SelectTarget();
            if (target != null)
            {
                float sqrDist = (target.transform.position - transform.position).sqrMagnitude;
                if (sqrDist <= attackRange * attackRange)
                {
                    await AttackAsync(target);
                }
            }

            await UniTask.Delay(TimeSpan.FromSeconds(attackInterval), cancellationToken: token);
        }
    }

    protected virtual Enemy SelectTarget()
    {
        if (_enemyManager == null) return null;

        switch (targetingMode)
        {
            case TargetingMode.NearestEveryTime:
                return _enemyManager.FindNearest(transform.position, attackRange);

            case TargetingMode.LockOnOne:
                currentTarget = _enemyManager.LockOnOne(transform.position, attackRange, currentTarget);
                return currentTarget;

            case TargetingMode.RandomInRange:
                return _enemyManager.FindRandomInRange(transform.position, attackRange);

            default:
                return null;
        }
    }

    protected abstract UniTask AttackAsync(Enemy target);
}
