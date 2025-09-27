using Cysharp.Threading.Tasks;
using UnityEngine;

public class BattlePhase : GamePhaseBase
{
    public override GamePhase Phase => GamePhase.Battle;

    public override async UniTask<GamePhase> RunAsync()
    {
        // ✅ 예시: 적이 다 죽을 때까지 대기
        await UniTask.WaitUntil(() =>
            FieldManager.Instance?.CurBattleField?.EnemyManager?.AliveEnemyCount.Value == 0
        );

        return GamePhase.Result;
    }
}
