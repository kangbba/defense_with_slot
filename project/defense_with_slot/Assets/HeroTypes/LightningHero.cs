using UnityEngine;
using DG.Tweening;
using UniRx;
public class LightningHero : Hero
{
    public override HeroType Type => HeroType.Lightning;
    public override void Attack()
    {
        Debug.Log("⚡ LightningHero: 단일 강력 공격!");
    }

    protected override void OnLevelUp()
    {
        base.OnLevelUp();
        if (Level >= 2) Debug.Log("⚡ 공격력 대폭 증가!");
        if (Level >= 3) Debug.Log("⚡ 연쇄 번개 추가!");
    }
}
