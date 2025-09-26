using UnityEngine;
using DG.Tweening;
using UniRx;

[RequireComponent(typeof(PoisonHeroCombat))] // 🔹 PoisonHeroCombat 반드시 필요
public class PoisonHero : Hero
{
    public override HeroType Type => HeroType.Poison;

    protected override void OnLevelUp()
    {
        base.OnLevelUp();
        if (Level >= 2) Debug.Log("☠️ 중독 지속시간 증가!");
        if (Level >= 3) Debug.Log("☠️ 광역 확산 추가!");
    }
}
