using UnityEngine;
using DG.Tweening;

public abstract class Hero : MonoBehaviour
{
    [Header("Visuals")]
    private SpriteRenderer _spriteRenderer;

    [Header("Level Settings")]
    [SerializeField] private int _level = 1;
    public int Level => _level;

    public abstract HeroType Type { get; }

    private Vector2 _baseScale;
    private Tween _moveTween;
    private Tween _scaleTween;

    // 🔹 Combat을 Hero가 직접 바인딩
    [SerializeField] private HeroCombat _combat;
    public HeroCombat Combat => _combat;

    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _spriteRenderer.sortingOrder = RenderOrder.HERO_ORDER;
        _baseScale = transform.localScale;

        // 인스펙터에 없으면 자동 캐싱
        if (_combat == null)
            _combat = GetComponent<HeroCombat>();

        if (_combat == null)
            Debug.LogWarning($"[Hero] {name} 에 HeroCombat 없음");
    }

    // ===============================
    // 초기화
    // ===============================
    public void Init(int level, IEnemyProvider provider)
    {
        SetLevel(level, applyScale: true);

        if (_combat != null && provider != null)
        {
            _combat.Init(provider);
            _combat.StartAttackLoop();
        }
    }

    // ===============================
    // 이동
    // ===============================
    public void SnapTo(Vector2 to, float duration = 0.15f)
    {
        _moveTween?.Kill(false);
        _moveTween = transform.DOMove(new Vector3(to.x, to.y, 0f), duration).SetEase(Ease.OutQuad);
    }

    public void RevertTo(Vector2 pos, float duration = 0.15f)
    {
        SnapTo(pos, duration);
    }

    // ===============================
    // 레벨 관리
    // ===============================
    public void SetLevel(int lv, bool applyScale = false)
    {
        _level = Mathf.Max(1, lv);
        if (applyScale) ApplyLevelScale(_level);
    }

    public void LevelUp()
    {
        _level++;
        ApplyLevelScale(_level);
        OnLevelUp();
    }

    protected virtual void OnLevelUp() { }

    private void ApplyLevelScale(int level)
    {
        const float step = 0.2f;
        float factor = 1f + (level - 1) * step;

        _scaleTween?.Kill(false);
        _scaleTween = transform
            .DOScale(new Vector3(_baseScale.x * factor, _baseScale.y * factor, 1f), 0.3f)
            .SetEase(Ease.OutBack);
    }

    // ===============================
    // 병합 로직
    // ===============================
    public bool CanMergeWith(Hero other)
    {
        if (other == null) return false;
        if (Type != other.Type) return false;
        return _level == other.Level;
    }

    public Hero MergeWith(Hero other)
    {
        if (!CanMergeWith(other)) return this;
        LevelUp();
        return this;
    }
}
