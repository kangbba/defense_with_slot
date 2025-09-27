using UnityEngine;
using DG.Tweening;
using UniRx;

public abstract class Hero : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Header("Level Settings")]
    [SerializeField] private int _level = 1;
    public int Level => _level;

    public abstract HeroType Type { get; }

    private Vector2 _baseScale;
    private Tween _moveTween;
    private Tween _scaleTween;

    [Header("Combat Binding")]
    [SerializeField] private HeroCombat _combat;
    public HeroCombat Combat => _combat;

    // ✅ 드래그 상태 (외부에서 토글)
    private readonly BoolReactiveProperty _isDragging = new(false);
    public IReadOnlyReactiveProperty<bool> IsDragging => _isDragging;

    private void Awake()
    {
        _spriteRenderer.sortingOrder = RenderOrder.HERO_ORDER;
        _baseScale = transform.localScale;
    }

    // ===============================
    // 초기화
    // ===============================
    public void Init(int level, EnemyManager em)
    {
        SetLevel(level, applyScale: true);

        _combat.Init(em);
        _combat.StartAttackLoop();
    }

    // ===============================
    // 드래그 상태 토글
    // ===============================
    public void SetDragging(bool dragging)
    {
        _isDragging.Value = dragging;
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
