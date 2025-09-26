using UnityEngine;
using System.Collections.Generic;

public enum HeroType
{
    Fire,      // 범위 폭발
    Element,     // 돈 생산
    Lightning, // 단일 강공
    Ice,       // 관통
    Poison,    // 범위 중독
    Rock       // 빠른 공격
}

public class HeroManager : SingletonMono<HeroManager>
{
    // ===============================
    // Constants
    // ===============================
    private const string HERO_PREFAB_PATH = "Heros/{0}Hero"; 
    // e.g. Resources/Heros/FireHero.prefab

    // ===============================
    // Prefab Cache
    // ===============================
    private readonly Dictionary<HeroType, Hero> _heroPrefabs = new Dictionary<HeroType, Hero>();

    [Header("Hierarchy")]
    [SerializeField] private Transform _root; // 히어로 부모 (없으면 자동 생성)

    protected override bool UseDontDestroyOnLoad => false;
   
    protected override void Awake()
    {
        base.Awake(); 
        EnsureRoot();
        PreloadHeroPrefabs(); 
    }

    private void EnsureRoot()
    {
        if (_root != null) return;

        var go = new GameObject("Heroes");
        _root = go.transform;
        _root.SetParent(transform, false);
        _root.localPosition = Vector3.zero;
    }

    private void PreloadHeroPrefabs()
    {
        foreach (HeroType type in System.Enum.GetValues(typeof(HeroType)))
        {
            string path = string.Format(HERO_PREFAB_PATH, type);
            var prefab = Resources.Load<Hero>(path);

            if (prefab == null)
            {
                Debug.LogError($"[HeroManager] Hero prefab not found at {path}");
                continue;
            }

            _heroPrefabs[type] = prefab;
        }

        Debug.Log($"[HeroManager] {_heroPrefabs.Count} hero prefabs loaded.");
    }

    // ===============================
    // Public API
    // ===============================
    public void MakeRandomHeroOnRandomCell()
    {
        var field = FieldManager.Instance.CurrentField;
        if (field == null) return;

        var cell = field.GetRandomEmptyCell();
        if (cell == null)
        {
            Debug.LogWarning("[HeroManager] 빈 셀이 없음");
            return;
        }

        HeroType heroType = GetRandomHeroType();
        SpawnHeroAtCell(heroType, 1, cell);
    }

    public Hero SpawnHeroAtCell(HeroType type, int level, Cell cell)
    {
        if (!_heroPrefabs.TryGetValue(type, out var prefab))
        {
            Debug.LogError($"[HeroManager] No prefab cached for {type}");
            return null;
        }

        EnsureRoot();

        Vector2 pos2 = cell.WorldPosition;
        var hero = Instantiate(
            prefab, 
            new Vector3(pos2.x, pos2.y, 0f), 
            Quaternion.identity,
            _root // ✅ 부모 지정
        );

        hero.Init(
            level,
            onDragStart: h => { },
            onDragEnd:   OnHeroDragEnd
        );

        FieldManager.Instance.CurrentField.Occupy(hero, cell);
        hero.SnapTo(cell.WorldPosition, 0f);

        Debug.Log($"[HeroManager] {type}Hero 생성 at {cell.GridPosition}, level {level}");
        return hero;
    }

    public void OnHeroDragEnd(Hero hero, Vector2 dropPos)
    {
        var bf = FieldManager.Instance?.CurrentField;
        if (bf == null) return;

        var targetCell = bf.GetClosestCell(dropPos);
        if (targetCell == null)
        {
            hero.RevertToStart();
            return;
        }

        var prevCell = bf.GetHeroCell(hero);
        var occupant = bf.GetOccupant(targetCell);

        if (prevCell == targetCell)
        {
            hero.SnapTo(targetCell.WorldPosition);
            Debug.Log("[HeroManager] SameCell 정렬");
            return;
        }

        if (occupant != null && occupant != hero)
        {
            if (occupant.Type == hero.Type)
            {
                hero.RevertToStart();
                Debug.Log($"[HeroManager] Merge candidate {hero.Type}");
                MergeManager.Instance.TryMerge(hero, occupant);
            }
            else
            {
                hero.RevertToStart();
                Debug.Log($"[HeroManager] Blocked by {occupant.Type}");
            }
            return;
        }

        if (bf.Occupy(hero, targetCell))
        {
            hero.SnapTo(targetCell.WorldPosition);
            Debug.Log($"[HeroManager] {hero.Type} placed at {targetCell.GridPosition}");
        }
        else
        {
            hero.RevertToStart();
            Debug.Log("[HeroManager] Occupy 실패. Revert");
        }
    }

    private HeroType GetRandomHeroType()
    {
        var values = (HeroType[])System.Enum.GetValues(typeof(HeroType));
        return values[Random.Range(0, values.Length)];
    }

    protected override void Release() 
    { 
        // 선택: 남은 히어로들 정리
        if (_root != null)
        {
            for (int i = _root.childCount - 1; i >= 0; i--)
                Destroy(_root.GetChild(i).gameObject);
        }
    }
}
