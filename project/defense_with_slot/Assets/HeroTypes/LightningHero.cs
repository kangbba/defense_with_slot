using UnityEngine;
using DG.Tweening;
using UniRx;

[RequireComponent(typeof(LightningHeroCombat))] // 🔹 LightningHeroCombat 반드시 필요
public class LightningHero : Hero
{
    public override HeroType Type => HeroType.Lightning;

    protected override void OnLevelUp()
    {
        base.OnLevelUp();
        if (Level >= 2) Debug.Log("⚡ 공격력 대폭 증가!");
        if (Level >= 3) Debug.Log("⚡ 연쇄 번개 추가!");
    }
}
