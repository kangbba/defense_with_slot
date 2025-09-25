using UnityEngine;
using DG.Tweening;
using UniRx;

public class FireHero : Hero
{
    public override HeroType Type => HeroType.Fire;

    public override void Attack()
    {
        Debug.Log("🔥 FireHero: 범위 폭발 공격!");
    }

    protected override void OnLevelUp()
    {
        base.OnLevelUp();
        if (Level >= 2) Debug.Log("🔥 폭발 범위 강화!");
        if (Level >= 3) Debug.Log("🔥 DOT 불길 추가!");
    }
}
