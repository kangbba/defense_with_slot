using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CellManager : MonoBehaviour
{
    [SerializeField] private Cell _cellPrefab;
    [SerializeField] private Transform _cellParent;
    [SerializeField] private float _cellSize = 1.2f;

    private Cell[,] _cells;

    // Hero ↔ Cell 매핑
    private readonly Dictionary<Hero, Cell> _heroToCell = new();
    private readonly Dictionary<Cell, Hero> _cellToHero = new();

    // ==============================
    // Init & Grid 생성
    // ==============================
    public void Init(int width, int height)
    {
        ClearGrid();
        _cells = new Cell[width, height];

        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            _cells[x, y] = CreateCell(new Vector2Int(x, y), width, height);
        }

        Debug.Log($"[CellManager] {width}x{height} grid created.");
    }

    private Cell CreateCell(Vector2Int coord, int width, int height)
    {
        Vector3 localPos = ComputeLocalPos(coord, width, height);
        var cellObj = Instantiate(_cellPrefab, _cellParent);
        cellObj.transform.localPosition = localPos;
        cellObj.Init(coord);
        return cellObj;
    }

    private Vector3 ComputeLocalPos(Vector2Int coord, int width, int height)
    {
        float halfW = (width - 1) * 0.5f;
        float halfH = (height - 1) * 0.5f;

        float x = (coord.x - halfW) * _cellSize;
        float y = (coord.y - halfH) * _cellSize;
        return new Vector3(x, y, 0f);
    }

    private void ClearGrid()
    {
        _heroToCell.Clear();
        _cellToHero.Clear();

        if (_cells == null) return;
        foreach (var c in _cells)
            if (c) Destroy(c.gameObject);

        _cells = null;
    }

    // ==============================
    // 조회
    // ==============================
    public IEnumerable<Cell> GetAllCells()
    {
        if (_cells == null) yield break;
        foreach (var c in _cells) if (c != null) yield return c;
    }

    public Cell GetRandomEmptyCell()
    {
        var empties = GetAllCells().Where(c => !_cellToHero.ContainsKey(c)).ToList();
        if (empties.Count == 0) return null;
        return empties[Random.Range(0, empties.Count)];
    }

    public Cell GetClosestCell(Vector2 worldPos)
    {
        if (_cells == null) return null;

        Cell best = null;
        float bestD2 = float.MaxValue;

        foreach (var c in _cells)
        {
            float d2 = ((Vector2)c.transform.position - worldPos).sqrMagnitude;
            if (d2 < bestD2) { bestD2 = d2; best = c; }
        }
        return best;
    }

    public Cell GetHeroCell(Hero hero) => _heroToCell.TryGetValue(hero, out var cell) ? cell : null;
    public Hero GetOccupant(Cell cell) => _cellToHero.TryGetValue(cell, out var h) ? h : null;
    public bool IsOccupied(Cell cell) => cell != null && _cellToHero.ContainsKey(cell);

    // ==============================
    // 점유 관리
    // ==============================
    public bool Occupy(Hero hero, Cell cell)
    {
        if (hero == null || cell == null) return false;
        if (_cellToHero.TryGetValue(cell, out var other) && other != hero) return false;

        if (_heroToCell.TryGetValue(hero, out var prev) && prev != null && prev != cell)
            _cellToHero.Remove(prev);

        _heroToCell[hero] = cell;
        _cellToHero[cell] = hero;

        hero.SnapTo(cell.transform.position);
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
            if (cell != null) _cellToHero.Remove(cell);
        }

        Destroy(hero.gameObject);
    }
}
