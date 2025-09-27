using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UniRx;

public class EnemyManager : MonoBehaviour
{
    [Header("Prefab & Root")]
    [SerializeField] private Enemy _enemyPrefab;
    [SerializeField] private Transform _root;

    private readonly List<Enemy> _activeEnemies = new();
    public IReadOnlyList<Enemy> ActiveEnemies => _activeEnemies;

    // Reactive enemy count
    private readonly ReactiveProperty<int> _aliveEnemyCount = new(0);
    public IReadOnlyReactiveProperty<int> AliveEnemyCount => _aliveEnemyCount;

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

    // ==============================
    // Enemy Spawn / Despawn
    // ==============================
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

        enemy.Init(
            new List<Vector2>(path),
            onDie: () =>
            {
                _activeEnemies.Remove(enemy);
                _aliveEnemyCount.Value = _activeEnemies.Count;
            });

        EnemyUIManager.Instance.MakeHpBarFor(enemy);

        _activeEnemies.Add(enemy);
        _aliveEnemyCount.Value = _activeEnemies.Count;

        return enemy;
    }

    public void DespawnAll()
    {
        foreach (var e in _activeEnemies)
            if (e != null) Destroy(e.gameObject);

        _activeEnemies.Clear();
        _aliveEnemyCount.Value = 0;

        if (_root != null)
        {
            for (int i = _root.childCount - 1; i >= 0; i--)
                Destroy(_root.GetChild(i).gameObject);
        }
    }

    // ==============================
    // Enemy Queries
    // ==============================
    public Enemy FindNearest(Vector3 from, float range)
    {
        float r2 = range * range;
        return _activeEnemies
            .Where(e => e != null && e.Hp.Value > 0)
            .OrderBy(e => (e.transform.position - from).sqrMagnitude)
            .FirstOrDefault(e => (e.transform.position - from).sqrMagnitude <= r2);
    }

    public Enemy FindRandomInRange(Vector3 from, float range)
    {
        float r2 = range * range;
        var list = _activeEnemies
            .Where(e => e != null && e.Hp.Value > 0)
            .Where(e => (e.transform.position - from).sqrMagnitude <= r2)
            .ToList();

        return list.Count > 0 ? list[Random.Range(0, list.Count)] : null;
    }

    public Enemy LockOnOne(Vector3 from, float range, Enemy currentTarget)
    {
        if (currentTarget == null || currentTarget.Hp.Value <= 0)
            return FindNearest(from, range);

        float r2 = range * range;
        float d2 = (currentTarget.transform.position - from).sqrMagnitude;
        return d2 > r2 ? FindNearest(from, range) : currentTarget;
    }
}
