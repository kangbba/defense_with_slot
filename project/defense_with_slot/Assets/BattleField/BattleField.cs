// BattleField.cs — 셀/배치 + 적 타겟팅 + UniTask 스폰루프 (정리본)
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;

public class BattleField : MonoBehaviour, IEnemyProvider
{
    // ==============================
    // Serialized Components
    // ==============================
    [Header("Components")]
    [SerializeField] private CellMaker _cellMaker;         // 셀 프리팹 생성기
    [SerializeField] private BattleFieldPath _path;        // 적 이동 경로
    [SerializeField] private EnemyManager _enemyManager;   // 적 생성/관리

    [Header("Enemy Spawn Rule")]
    [SerializeField, Min(0.05f)] private float _spawnInterval = 0.5f;

    // ==============================
    // Private Fields
    // ==============================
    private Cell[,] _cells;
    private CancellationTokenSource _spawnCts;

    // 히어로 ↔ 셀 매핑
    private readonly Dictionary<Hero, Cell> _heroToCell = new();
    private readonly Dictionary<Cell, Hero> _cellToHero = new();

    // ==============================
    // Properties
    // ==============================
    public BattleFieldPath Path => _path;

    // IEnemyProvider 구현 (읽기전용 스냅샷로 반환)
    public List<Enemy> ActiveEnemies => _enemyManager.ActiveEnemies.ToList();

    // ==============================
    // Unity Lifecycle
    // ==============================
    private void OnDisable() => StopEnemySpawn(); // 씬 전환 안전
    private void OnDestroy() => StopEnemySpawn();

    // ==============================
    // Public API : Grid Init / Query
    // ==============================
    /// <summary>필드를 width x height 그리드로 초기화</summary>
    public void Init(int width, int height)
    {
        ClearGrid();

        _cells = new Cell[width, height];
        Vector2 center = (Vector2)transform.position;

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            _cells[x, y] = _cellMaker.CreateCell(new Vector2Int(x, y), width, height, center);
        }

        Debug.Log($"[BattleField] {width}x{height} field created (center {center}).");
    }

    /// <summary>모든 셀 이터레이터</summary>
    public IEnumerable<Cell> GetAllCells()
    {
        if (_cells == null) yield break;
        foreach (var c in _cells) if (c != null) yield return c;
    }

    /// <summary>비어있는 셀 중 무작위</summary>
    public Cell GetRandomEmptyCell()
    {
        if (_cells == null) return null;

        var empties = GetAllCells()
            .Where(c => !_cellToHero.ContainsKey(c))
            .ToList();

        if (empties.Count == 0) return null;
        return empties[Random.Range(0, empties.Count)];
    }

    /// <summary>월드 좌표에 가장 가까운 셀</summary>
    public Cell GetClosestCell(Vector2 worldPos)
    {
        if (_cells == null) return null;

        Cell best = null;
        float bestD2 = float.MaxValue;

        foreach (var c in _cells)
        {
            if (c == null) continue;
            float d2 = ((Vector2)c.WorldPosition - worldPos).sqrMagnitude;
            if (d2 < bestD2) { bestD2 = d2; best = c; }
        }
        return best;
    }

    // ==============================
    // Public API : Occupancy
    // ==============================
    public Cell GetHeroCell(Hero hero)
    {
        _heroToCell.TryGetValue(hero, out var cell);
        return cell;
    }

    public Hero GetOccupant(Cell cell)
    {
        _cellToHero.TryGetValue(cell, out var h);
        return h;
    }

    public bool IsOccupied(Cell cell) => cell != null && _cellToHero.ContainsKey(cell);

    /// <summary>히어로를 셀에 배치(성공 시 위치 스냅)</summary>
    public bool Occupy(Hero hero, Cell cell)
    {
        if (hero == null || cell == null) return false;

        // 이미 다른 히어로가 점유 중이면 실패
        if (_cellToHero.TryGetValue(cell, out var other) && other != hero)
            return false;

        // 이전 자리 비우기
        if (_heroToCell.TryGetValue(hero, out var prev) && prev != null && prev != cell)
            _cellToHero.Remove(prev);

        // 매핑 갱신
        _heroToCell[hero] = cell;
        _cellToHero[cell] = hero;

        // 위치 스냅
        hero.SnapTo(cell.WorldPosition);

        return true;
    }

    public void Vacate(Hero hero)
    {
        if (!_heroToCell.TryGetValue(hero, out var cell) || cell == null) return;
        _heroToCell.Remove(hero);
        _cellToHero.Remove(cell);
    }

    public void RemoveHero(Hero hero)
    {
        if (hero == null) return;

        if (_heroToCell.TryGetValue(hero, out var cell))
        {
            _heroToCell.Remove(hero);
            if (cell != null && _cellToHero.ContainsKey(cell))
                _cellToHero.Remove(cell);
        }

        Destroy(hero.gameObject);
    }

    // ==============================
    // Public API : Enemy Queries (IEnemyProvider)
    // ==============================
    public Enemy FindNearest(Vector3 from, float range)
    {
        float r2 = range * range;
        return _enemyManager.ActiveEnemies
            .Where(e => e != null && e.Hp.Value > 0)
            .OrderBy(e => (e.transform.position - from).sqrMagnitude)
            .FirstOrDefault(e => (e.transform.position - from).sqrMagnitude <= r2);
    }

    public Enemy FindRandomInRange(Vector3 from, float range
    ){
        float r2 = range * range;
        var list = _enemyManager.ActiveEnemies
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

    // ==============================
    // Public API : Enemy Spawn Control (UniTask)
    // ==============================
    public void StartEnemySpawn()
    {
        StopEnemySpawn();                  // 중복 방지
        _spawnCts = new CancellationTokenSource();
        SpawnLoopAsync(_spawnCts.Token).Forget();
    }

    public void StopEnemySpawn()
    {
        if (_spawnCts == null) return;
        if (!_spawnCts.IsCancellationRequested)
            _spawnCts.Cancel();

        _spawnCts.Dispose();
        _spawnCts = null;
    }

    // ==============================
    // Private Helpers
    // ==============================
    private void ClearGrid()
    {
        _heroToCell.Clear();
        _cellToHero.Clear();

        if (_cells == null) return;
        foreach (var c in _cells)
            if (c) Destroy(c.gameObject);

        _cells = null;
    }

    private async UniTaskVoid SpawnLoopAsync(CancellationToken token)
    {
        // 경로 준비 대기
        await UniTask.WaitUntil(
            () => _path != null && _path.GetPath()?.Count >= 2,
            cancellationToken: token
        );

        // 루프
        var delay = System.TimeSpan.FromSeconds(_spawnInterval);
        while (!token.IsCancellationRequested)
        {
            _enemyManager.SpawnEnemy(this);
            await UniTask.Delay(delay, cancellationToken: token);
        }
    }
}
