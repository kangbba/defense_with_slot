using UnityEngine;
using DG.Tweening;
using UniRx;

[RequireComponent(typeof(RockHeroCombat))] // 🔹 RockHeroCombat 반드시 필요
public class RockHero : Hero
{
    public override HeroType Type => HeroType.Rock;

    protected override void OnLevelUp()
    {
        base.OnLevelUp();
        if (Level >= 2) Debug.Log("🪨 공격 속도 증가!");
        if (Level >= 3) Debug.Log("🪨 연속 타격 추가!");
    }
}
