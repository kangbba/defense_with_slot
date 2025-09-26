using UnityEngine;
using DG.Tweening;
using UniRx;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Draggable))]
public abstract class Hero : MonoBehaviour
{
    [Header("Visuals")]
    private SpriteRenderer _spriteRenderer;

    [Header("Level Settings")]
    [SerializeField] private int _level = 1; // private backing field
    public int Level => _level;              // 외부는 읽기 전용

    public abstract HeroType Type { get; }
    private Draggable _draggable;
    private Vector2 _baseScale;

    private System.Action<Hero> _onDragStart;
    private System.Action<Hero, Vector2> _onDragEnd;

    private Tween _moveTween;
    private Tween _scaleTween;

    private Vector2 _dragStartPos;
    private Cell _dragStartCell;

    // ===============================
    // 초기화
    // ===============================
    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _spriteRenderer.sortingOrder = RenderOrder.HERO_ORDER;
    }
    public void Init(
        int level,
        System.Action<Hero> onDragStart,
        System.Action<Hero, Vector2> onDragEnd)
    {
        _onDragStart = onDragStart;
        _onDragEnd   = onDragEnd;

        if (_draggable == null) 
            _draggable = GetComponent<Draggable>();

        _baseScale = new Vector2(transform.localScale.x, transform.localScale.y);

        SetLevel(level, applyScale:true);

        // 드래그 시작
        _draggable.IsDragging
            .DistinctUntilChanged()
            .Where(on => on)
            .Subscribe(_ =>
            {
                var bf = FieldManager.Instance?.CurrentField;
                _dragStartPos  = new Vector2(transform.position.x, transform.position.y);
                _dragStartCell = bf?.GetHeroCell(this);

                _onDragStart?.Invoke(this);
            })
            .AddTo(this);

        // 드래그 끝
        _draggable.IsDragging
            .DistinctUntilChanged()
            .Where(on => !on)
            .WithLatestFrom(_draggable.CurrentWorldPos, (_, pos) => pos)
            .Subscribe(pos => _onDragEnd?.Invoke(this, pos))
            .AddTo(this);

        // ✅ 전투 루프 시작
        var combat = GetComponent<HeroCombat>();
        if (combat != null)
            combat.StartAttackLoop();
    }


    // ===============================
    // 이동
    // ===============================
    public void SnapTo(Vector2 to, float duration = 0.15f)
    {
        _moveTween?.Kill(false);
        _moveTween = transform.DOMove(new Vector3(to.x, to.y, 0f), duration).SetEase(Ease.OutQuad);
    }

    public void RevertToStart(float duration = 0.15f)
    {
        Vector2 back = _dragStartCell != null 
            ? (Vector2)_dragStartCell.WorldPosition 
            : _dragStartPos;

        SnapTo(back, duration);
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
        return _level == other._level;
    }

    public Hero MergeWith(Hero other)
    {
        if (!CanMergeWith(other)) return this;

        LevelUp();
        return this;
    }

}
