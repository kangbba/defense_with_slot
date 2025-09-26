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
    [SerializeField] protected float attackInterval = 1f;   // 공격 간격
    [SerializeField] protected float attackRange = 5f;      // 사정거리
    [SerializeField] protected TargetingMode targetingMode = TargetingMode.NearestEveryTime;

    protected Hero hero;
    protected Enemy currentTarget; // LockOn 모드 전용
    private CancellationTokenSource _cts;

    protected virtual void Awake()
    {
        hero = GetComponent<Hero>();
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
            var draggable = hero.GetComponent<Draggable>();
            if (draggable != null && draggable.IsDragging.Value)
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
                    // 🔹 공격 동작이 끝날 때까지 기다림
                    await AttackAsync(target);
                }
            }

            // 🔹 공격 이후 딜레이
            await UniTask.Delay(TimeSpan.FromSeconds(attackInterval), cancellationToken: token);
        }
    }

    protected virtual Enemy SelectTarget()
    {
        switch (targetingMode)
        {
            case TargetingMode.NearestEveryTime:
                return FindNearestEnemy();

            case TargetingMode.LockOnOne:
                if (currentTarget == null || currentTarget.Hp.Value <= 0)
                {
                    currentTarget = FindNearestEnemy();
                }
                else
                {
                    // 🔹 사정거리 벗어나면 해제 후 다시 찾음
                    float sqrDist = (currentTarget.transform.position - transform.position).sqrMagnitude;
                    if (sqrDist > attackRange * attackRange)
                    {
                        currentTarget = FindNearestEnemy();
                    }
                }
                return currentTarget;

            case TargetingMode.RandomInRange:
                return FindRandomEnemyInRange();

            default:
                return null;
        }
    }

    protected Enemy FindNearestEnemy()
    {
        var enemies = EnemyManager.Instance.ActiveEnemies;
        if (enemies == null || enemies.Count == 0) return null;

        Enemy nearest = null;
        float minDist = float.MaxValue;
        Vector3 myPos = transform.position;

        foreach (var e in enemies)
        {
            if (e == null || e.Hp.Value <= 0) continue;
            float dist = (e.transform.position - myPos).sqrMagnitude;
            if (dist < minDist && dist <= attackRange * attackRange)
            {
                minDist = dist;
                nearest = e;
            }
        }
        return nearest;
    }

    protected Enemy FindRandomEnemyInRange()
    {
        var enemies = EnemyManager.Instance.ActiveEnemies;
        if (enemies == null || enemies.Count == 0) return null;

        var list = new System.Collections.Generic.List<Enemy>();
        Vector3 myPos = transform.position;

        foreach (var e in enemies)
        {
            if (e == null || e.Hp.Value <= 0) continue;
            float dist = (e.transform.position - myPos).sqrMagnitude;
            if (dist <= attackRange * attackRange)
                list.Add(e);
        }

        if (list.Count == 0) return null;
        int idx = UnityEngine.Random.Range(0, list.Count);
        return list[idx];
    }

    // 🔹 영웅별로 구현 (즉시형은 CompletedTask 반환, 투척/폭탄형은 travelTime 끝날 때까지 기다림)
    protected abstract UniTask AttackAsync(Enemy target);
}
