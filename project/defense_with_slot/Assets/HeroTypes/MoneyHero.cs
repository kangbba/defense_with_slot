using UnityEngine;
using DG.Tweening;
using UniRx;
public class MoneyHero : Hero
{
    public override HeroType Type => HeroType.Money;

    public override void Attack()
    {
        Debug.Log("💰 MoneyHero: 돈 생산!");
    }

    protected override void OnLevelUp()
    {
        base.OnLevelUp();
        if (Level >= 2) Debug.Log("💰 생산량 증가!");
        if (Level >= 3) Debug.Log("💰 추가 골드 보너스!");
    }
}
