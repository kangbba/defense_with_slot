using UnityEngine;
using DG.Tweening;
using UniRx;
public class PoisonHero : Hero
{
    public override HeroType Type => HeroType.Poison;

    public override void Attack()
    {
        Debug.Log("☠️ PoisonHero: 범위 중독 공격!");
    }

    protected override void OnLevelUp()
    {
        base.OnLevelUp();
        if (Level >= 2) Debug.Log("☠️ 중독 지속시간 증가!");
        if (Level >= 3) Debug.Log("☠️ 광역 확산 추가!");
    }
}
