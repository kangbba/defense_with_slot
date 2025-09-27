using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class ElementHeroCombat : HeroCombat
{
    [Header("Element Settings")]
    [SerializeField] private int orbAmount = 1;

    protected override async UniTask AttackAsync(Enemy target)
    {
        if (target == null) return;

        // 🔹 Orb 대신 그냥 형식만 유지 (아직 구현 X)
        await UniTask.Yield(); // 형식상 async 유지
    }
}
