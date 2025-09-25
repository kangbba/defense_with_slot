using UnityEngine;
using UniRx;

public class Cell : MonoBehaviour
{
    private Vector2Int _gridPosition;
    private Vector2 _worldPosition;

    [SerializeField] private SpriteRenderer _renderer;

    private readonly ReactiveProperty<bool> _isHighlighted = new(false);
    public IReadOnlyReactiveProperty<bool> IsHighlighted => _isHighlighted;

    public void Init(Vector2Int gridPos, Vector2 worldPos)
    {
        _gridPosition = gridPos;
        _worldPosition = worldPos;

        _isHighlighted
            .Subscribe(on =>
            {
                if (_renderer != null)
                    _renderer.color = on ? Color.blue : Color.white;
            })
            .AddTo(this);
    }


    public Vector2Int GridPosition => _gridPosition;
    public Vector2 WorldPosition => _worldPosition;
}
