using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class RockHeroCombat : HeroCombat
{
    [Header("Rock Settings")]
    [SerializeField] private int impactDamage = 15;          // 충돌 피해
    [SerializeField] private float travelTime = 2f;          // 유도탄 수명
    [SerializeField] private float projectileSpeed = 15f;    // 유도탄 속도
    [SerializeField] private Projectile projectilePrefab;    // 유도탄 프리팹

    [Header("Effect Settings")]
    [SerializeField] private Color rockColor = new Color(0.5f, 0.4f, 0.3f, 0.9f);
    [SerializeField] private float effectScale = 1.2f;

    protected override async UniTask AttackAsync(Enemy target)
    {
        if (target == null) return;

        Vector2 spawnPos = transform.position;

        // 🪨 유도탄 생성
        if (projectilePrefab != null)
        {
            var proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            proj.LaunchHoming(
                target,
                speed: projectileSpeed,
                lifeTime: travelTime,
                damage: impactDamage,
                maxHitCount: 1,
                onHit: e =>
                {
                    if (e == null) {
                    Debug.LogWarning("dd"); return ; }
                    e.TakeDamage(impactDamage);

                    // Knockback 추가
                    Vector2 dir = ((Vector2)e.transform.position - (Vector2)spawnPos).normalized;
                    e.ApplyKnockback(dir, 0.5f, 0.3f);

                    // 💥 피격 이펙트
                    EffectUtility.PlayEffect(
                        e.transform.position,
                        rockColor,
                        0.2f,
                        effectScale,
                        0.2f
                    );
                }
            );
        }

        // 바위 영웅은 "빠른 공격"이므로, 발사 후 바로 리턴
        await UniTask.CompletedTask;
    }
}
