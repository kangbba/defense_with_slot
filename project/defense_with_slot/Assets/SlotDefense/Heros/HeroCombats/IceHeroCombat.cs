using UnityEngine;
using Cysharp.Threading.Tasks;

public class IceHeroCombat : HeroCombat
{
    [Header("Ice Settings")]
    [SerializeField] private Projectile iceProjectilePrefab;
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float projectileLifeTime = 3f;
    [SerializeField] private int damage = 8;
    [SerializeField] private int penetration = 3;   // 관통 가능 수
    [SerializeField] private float slowDuration = 2f; // 둔화 지속 시간
    [SerializeField] private float slowFactor = 0.5f; // 원래 속도의 몇 배로 줄일지

    protected override UniTask AttackAsync(Enemy target)
    {
        if (iceProjectilePrefab == null || target == null)
            return UniTask.CompletedTask;

        Vector2 spawnPos = transform.position;
        Vector2 dir = ((Vector2)target.transform.position - spawnPos).normalized;

        Projectile proj = Instantiate(iceProjectilePrefab, spawnPos, Quaternion.identity);
        proj.Launch(
            dir: dir,
            speed: projectileSpeed,
            lifeTime: projectileLifeTime,
            maxHitCount: penetration,
            damage: damage,
            onHit: enemy =>
            {
                enemy.TakeDamage(damage);
                enemy.ApplySlow(slowFactor, slowDuration); // ❄️ 둔화 효과
            }
        );

        // 즉발형이므로 바로 CompletedTask 반환
        return UniTask.CompletedTask;
    }
}
