using UnityEngine;
using System.Collections.Generic;

public class HeroManager : SingletonMono<HeroManager>
{
    private const string HERO_PREFAB_PATH = "Heros/{0}Hero"; 
    private readonly Dictionary<HeroType, Hero> _heroPrefabs = new();

    [Header("Hierarchy")]
    [SerializeField] private Transform _root;

    protected override bool UseDontDestroyOnLoad => false;

    protected override void Awake()
    {
        base.Awake();
        EnsureRoot();
        PreloadHeroPrefabs();
    }

    // ======================================
    // 초기화 / 프리팹 캐싱
    // ======================================
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

    // ======================================
    // 생성 관련
    // ======================================
    public Hero SpawnHeroAtCell(HeroType type, int level, Cell cell)
    {
        if (!_heroPrefabs.TryGetValue(type, out var prefab))
        {
            Debug.LogError($"[HeroManager] No prefab cached for {type}");
            return null;
        }

        var bf = FieldManager.Instance.CurBattleField;
        if (cell == null || bf == null)
        {
            Debug.LogWarning("[HeroManager] Target cell or BattleField is null");
            return null;
        }

        EnsureRoot();

        var hero = Instantiate(
            prefab,
            new Vector3(cell.WorldPosition.x, cell.WorldPosition.y, 0f),
            Quaternion.identity,
            _root
        );

        // 🔹 전투 가능하도록 IEnemyProvider까지 같이 주입
        hero.Init(level, bf);

        // 전장에 등록
        bf.Occupy(hero, cell);

        Debug.Log($"[HeroManager] {type}Hero 생성 at {cell.GridPosition}, level {level}");
        return hero;
    }

    public Hero MakeRandomHeroOnRandomCell()
    {
        var bf = FieldManager.Instance.CurBattleField;
        if (bf == null) return null;

        var cell = bf.GetRandomEmptyCell();
        if (cell == null) return null;

        var randomType = GetRandomHeroType();
        return SpawnHeroAtCell(randomType, 1, cell);
    }

    public HeroType GetRandomHeroType()
    {
        var values = (HeroType[])System.Enum.GetValues(typeof(HeroType));
        return values[Random.Range(0, values.Length)];
    }

    // ======================================
    // 정리
    // ======================================
    protected override void Release()
    {
        if (_root != null)
        {
            for (int i = _root.childCount - 1; i >= 0; i--)
                Destroy(_root.GetChild(i).gameObject);
        }
    }
}
