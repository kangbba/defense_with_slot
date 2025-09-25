using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleField : MonoBehaviour
{
    [SerializeField] private CellMaker _cellMaker;
    [SerializeField] private BattleFieldPath _path; // 경로 바인딩

    private Cell[,] _cells;

    private readonly Dictionary<Hero, Cell> _heroToCell = new();
    private readonly Dictionary<Cell, Hero> _cellToHero = new();

    public BattleFieldPath Path => _path;

    // -------- 생성/정리 --------
    public void Init(int width, int height)
    {
        Clear();

        _cells = new Cell[width, height];
        Vector2 center = new Vector2(transform.position.x, transform.position.y);

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            _cells[x, y] = _cellMaker.CreateCell(new Vector2Int(x, y), width, height, center, transform);
        }

        Debug.Log($"[BattleField] {width}x{height} field created (center {center}).");
    }

    private void Clear()
    {
        _heroToCell.Clear();
        _cellToHero.Clear();

        if (_cells == null) return;
        foreach (var c in _cells)
        {
            if (c) Object.Destroy(c.gameObject);
        }
    }

    // -------- 조회 유틸 --------
    public IEnumerable<Cell> GetAllCells()
    {
        foreach (var c in _cells) if (c != null) yield return c;
    }

    public Cell GetRandomEmptyCell()
    {
        if (_cells == null) return null;

        var empties = GetAllCells()
            .Where(c => !_cellToHero.ContainsKey(c))
            .ToList();

        if (empties.Count == 0)
        {
            Debug.LogWarning("[BattleField] No empty cells available.");
            return null;
        }

        return empties[Random.Range(0, empties.Count)];
    }

    public Cell GetClosestCell(Vector2 worldPos)
    {
        if (_cells == null) return null;

        Cell best = null;
        float bestD2 = float.MaxValue;

        foreach (var c in _cells)
        {
            if (c == null) continue;
            Vector2 p = c.WorldPosition;
            float d2 = (p - worldPos).sqrMagnitude;
            if (d2 < bestD2) { bestD2 = d2; best = c; }
        }
        return best;
    }

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

    public bool Occupy(Hero hero, Cell cell)
    {
        if (hero == null || cell == null) return false;
        if (_cellToHero.TryGetValue(cell, out var other) && other != hero)
            return false;

        if (_heroToCell.TryGetValue(hero, out var prev) && prev != null && prev != cell)
            _cellToHero.Remove(prev);

        _heroToCell[hero] = cell;
        _cellToHero[cell] = hero;
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

        Object.Destroy(hero.gameObject);
    }
}
