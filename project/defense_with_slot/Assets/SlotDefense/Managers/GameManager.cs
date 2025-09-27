// GameManager.cs
using UnityEngine;
using System.Collections;

public class GameManager : SingletonMono<GameManager>
{
    protected override bool UseDontDestroyOnLoad => true;
    protected override void Release() { /* 필요시 리소스 정리 */ }

    private void Start()
    {
        // 1) 전장 생성
        var bf = FieldManager.Instance.MakeBattleField();
        bf.StartEnemySpawn();
    }

}
