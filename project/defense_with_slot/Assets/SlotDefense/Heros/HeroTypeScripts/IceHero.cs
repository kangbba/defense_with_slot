using UnityEngine;
using DG.Tweening;
using UniRx;

[RequireComponent(typeof(IceHeroCombat))] // 🔹 IceHeroCombat 반드시 필요
public class IceHero : Hero
{
    public override HeroType Type => HeroType.Ice;

    protected override void OnLevelUp()
    {
        base.OnLevelUp();
        if (Level >= 2) Debug.Log("❄️ 관통 범위 증가!");
        if (Level >= 3) Debug.Log("❄️ 빙결 효과 추가!");
    }
}
