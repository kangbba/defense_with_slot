using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnemyManager : SingletonMono<EnemyManager>
{
    [SerializeField] private Enemy _enemyPrefab;
    [SerializeField] private HpBar _hpBarPrefab; // UI Prefab (World Canvas에 붙는)
    [SerializeField] private Transform _uiRoot; // WorldCanvas 밑에 둘 루트
    [SerializeField] private Transform _root; // 적들을 담을 부모(없으면 자동 생성)

    [Header("Enemy Auto-Spawn")]
    [SerializeField, Min(0.05f)] private float _spawnInterval = 0.5f;

    private Coroutine _spawnRoutine;

    private readonly List<Enemy> _activeEnemies = new();

    protected override bool UseDontDestroyOnLoad => false;

    public List<Enemy> ActiveEnemies => _activeEnemies;

    protected override void Release()
    {
        DespawnAll();
    }

    private void Start()
    {
        EnsureRoot();
    }

    private void EnsureRoot()
    {
        if (_root != null) return;

        var go = new GameObject("Enemies");
        _root = go.transform;
        _root.SetParent(transform, false);
        _root.localPosition = Vector3.zero;
    }

    /// <summary>현재 BattleField의 경로 첫 지점에서 적을 소환하고, 경로를 따라 이동시킵니다.</summary>
    public void SpawnEnemy()
    {
        var bf = FieldManager.Instance?.CurrentField as BattleField;
        if (bf == null)
        {
            Debug.LogError("[EnemyManager] BattleField가 없습니다.");
            return;
        }

        var pathComp = bf.Path;
        if (pathComp == null)
        {
            Debug.LogError("[EnemyManager] BattleFieldPath 바인딩이 없습니다.");
            return;
        }

        var pathReadonly = pathComp.GetPath();
        if (pathReadonly == null || pathReadonly.Count == 0)
        {
            Debug.LogError("[EnemyManager] 경로가 비어있습니다.");
            return;
        }

        EnsureRoot();

        // 시작 위치(좌상단 기준)에서 스폰 + 부모 지정
        Vector2 start = pathComp.GetStartPoint();
        var enemy = Instantiate(_enemyPrefab, new Vector3(start.x, start.y, 0f), Quaternion.identity, _root);

        // Enemy 초기화 (경로 전달: 복사본)
        enemy.Init(new List<Vector2>(pathReadonly), onDie: () => _activeEnemies.Remove(enemy));

        var hpBar = Instantiate(_hpBarPrefab, _uiRoot);
        hpBar.Bind(enemy);

        _activeEnemies.Add(enemy);
    }

    /// <summary>활성화된 적 전부 제거</summary>
    public void DespawnAll()
    {
        // 리스트 기준 정리
        foreach (var e in _activeEnemies)
            if (e != null) Destroy(e.gameObject);
        _activeEnemies.Clear();

        // 혹시 모를 잔여 하위 오브젝트도 정리
        if (_root != null)
        {
            for (int i = _root.childCount - 1; i >= 0; i--)
                Destroy(_root.GetChild(i).gameObject);
        }
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
            SpawnEnemy();

            yield return wait;
        }
    }
}
