using UnityEngine;
using System.Collections.Generic;
using UniRx;

public class EnemyManager : MonoBehaviour
{
    [Header("Prefab & Root")]
    [SerializeField] private Enemy _enemyPrefab;
    [SerializeField] private Transform _root;

    // 현재 살아있는 적 목록
    private readonly List<Enemy> _activeEnemies = new();
    public IReadOnlyList<Enemy> ActiveEnemies => _activeEnemies;

    // ReactiveProperty: 살아있는 적 수
    public readonly ReactiveProperty<int> ActiveEnemyCount = new ReactiveProperty<int>(0);

    private void Awake()
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

    /// <summary>
    /// 단일 적 생성 (BattleField에서 호출)
    /// </summary>
    public Enemy SpawnEnemy(BattleField bf)
    {
        if (bf == null || bf.Path == null) return null;

        var path = bf.Path.GetPath();
        if (path == null || path.Count == 0) return null;

        EnsureRoot();

        Vector2 start = bf.Path.GetStartPoint();
        var enemy = Instantiate(
            _enemyPrefab,
            new Vector3(start.x, start.y, 0f),
            Quaternion.identity,
            _root
        );

        // 초기화 (죽을 때 리스트에서 제거 + 카운트 갱신)
        enemy.Init(
            new List<Vector2>(path),
            onDie: () =>
            {
                _activeEnemies.Remove(enemy);
                ActiveEnemyCount.Value = _activeEnemies.Count;
            });

        _activeEnemies.Add(enemy);
        ActiveEnemyCount.Value = _activeEnemies.Count;

        // HP바 생성
        EnemyUIManager.Instance.MakeHpBar(enemy);

        return enemy;
    }

    /// <summary>
    /// 모든 적 제거
    /// </summary>
    public void DespawnAll()
    {
        foreach (var e in _activeEnemies)
            if (e != null) Destroy(e.gameObject);

        _activeEnemies.Clear();
        ActiveEnemyCount.Value = 0;

        if (_root != null)
        {
            for (int i = _root.childCount - 1; i >= 0; i--)
                Destroy(_root.GetChild(i).gameObject);
        }
    }
}
