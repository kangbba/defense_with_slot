using UnityEngine;
using DG.Tweening;
using UniRx;

[RequireComponent(typeof(FireHeroCombat))] // 🔹 FireHeroCombat 반드시 필요
public class FireHero : Hero
{
    public override HeroType Type => HeroType.Fire;

    protected override void OnLevelUp()
    {
        base.OnLevelUp();
        if (Level >= 2) Debug.Log("🔥 폭발 범위 강화!");
        if (Level >= 3) Debug.Log("🔥 DOT 불길 추가!");
    }
}
