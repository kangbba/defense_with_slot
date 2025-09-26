using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class LightningHeroCombat : HeroCombat
{
    [Header("Lightning Settings")]
    [SerializeField] private int damage = 20;
    [SerializeField] private float strikeRadius = 1f;
    [SerializeField] private float chargeTime = 0.5f; // 기 모으는 시간

    [Header("Effect Settings")]
    [SerializeField] private Color lightningColor = Color.cyan;
    [SerializeField] private float effectScale = 1.5f;

    protected override async UniTask AttackAsync(Enemy target)
    {
        if (target == null) return;

        // ⚡ 기 모으는 이펙트 (자신 위치)
        EffectUtility.PlayEffect(transform.position, lightningColor, 0.3f, effectScale, chargeTime);

        await UniTask.Delay(TimeSpan.FromSeconds(chargeTime));

        if (target == null || target.Hp.Value <= 0) return;

        // ⚡ 낙뢰 이펙트 (타겟 위치)
        EffectUtility.PlayEffect(target.transform.position, lightningColor, effectScale, effectScale * 2f, 0.3f);

        // 범위 피해
        var hits = Physics2D.OverlapCircleAll(target.transform.position, strikeRadius);
        foreach (var h in hits)
        {
            var e = h.GetComponent<Enemy>();
            if (e != null) e.TakeDamage(damage);
        }
    }
}
