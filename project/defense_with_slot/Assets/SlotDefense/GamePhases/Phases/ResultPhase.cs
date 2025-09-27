using Cysharp.Threading.Tasks;
using UnityEngine;

public class ResultPhase : GamePhaseBase
{
    public override GamePhase Phase => GamePhase.Result;

    public override async UniTask<GamePhase> RunAsync()
    {
        // ✅ 예시: 플레이어가 R 키를 누르면 메인으로 돌아감
        await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.R));
        return GamePhase.Main;
    }
}
