using UnityEngine;
using DG.Tweening;
using UniRx;

[RequireComponent(typeof(ElementHeroCombat))] // 🔹 ElementHeroCombat 반드시 필요
public class ElementalHero : Hero
{
    // ⚠️ HeroType 정의에 맞게 수정 필요
    public override HeroType Type => HeroType.Element; 

    protected override void OnLevelUp()
    {
        base.OnLevelUp();
        if (Level >= 2) Debug.Log("✨ 원소 생성 속도 증가!");
        if (Level >= 3) Debug.Log("✨ 생성량 두 배!");
    }
}
