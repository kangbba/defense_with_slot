using Cysharp.Threading.Tasks;
using UnityEngine;

public class MainPhase : GamePhaseBase
{
    public override GamePhase Phase => GamePhase.Main;

    public override async UniTask<GamePhase> RunAsync()
    {
        // ✅ 스페이스바 누르면 BattlePhase로 전환
        await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        return GamePhase.Battle;
    }
}
