// GameManager.cs
using UnityEngine;
using System.Collections;

public class GameManager : SingletonMono<GameManager>
{
    [Header("Enemy Auto-Spawn")]
    [SerializeField] private bool _autoSpawn = true;
    [SerializeField, Min(0.05f)] private float _spawnInterval = 0.5f;

    protected override bool UseDontDestroyOnLoad => true;
    protected override void Release() { /* 필요시 리소스 정리 */ }

    private Coroutine _spawnRoutine;

    private void Start()
    {
        // 1) 전장 생성
        FieldManager.Instance.MakeBattleField();

        // 2) 자동 스폰 시작
        if (_autoSpawn) StartEnemySpawn();
    }

    public void StartEnemySpawn()
    {
        if (_spawnRoutine != null) return;
        _spawnRoutine = StartCoroutine(SpawnLoop());
    }

    public void StopEnemySpawn()
    {
        if (_spawnRoutine == null) return;
        StopCoroutine(_spawnRoutine);
        _spawnRoutine = null;
    }

    private IEnumerator SpawnLoop()
    {
        // 전장/경로 준비될 때까지 대기
        while (true)
        {
            var bf = FieldManager.Instance?.CurrentField as BattleField;
            if (bf != null && bf.Path != null)
            {
                var path = bf.Path.GetPath();
                if (path != null && path.Count > 1) break;
            }
            yield return null;
        }

        var wait = new WaitForSeconds(_spawnInterval);
        while (true)
        {
            if (EnemyManager.HasInstance)
                EnemyManager.Instance.SpawnEnemy();

            yield return wait;
        }
    }
}
