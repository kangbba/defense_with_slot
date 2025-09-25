using UnityEngine;
using DG.Tweening;
using UniRx;
public class IceHero : Hero
{
    public override HeroType Type => HeroType.Ice;

    public override void Attack()
    {
        Debug.Log("❄️ IceHero: 관통 공격!");
    }

    protected override void OnLevelUp()
    {
        base.OnLevelUp();
        if (Level >= 2) Debug.Log("❄️ 관통 범위 증가!");
        if (Level >= 3) Debug.Log("❄️ 빙결 효과 추가!");
    }
}
