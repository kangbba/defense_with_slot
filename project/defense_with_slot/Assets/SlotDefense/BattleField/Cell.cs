using UnityEngine;
using UniRx;

public class Cell : MonoBehaviour
{
    private Vector2Int _coord;

    [SerializeField] private SpriteRenderer _renderer;

    private readonly ReactiveProperty<bool> _isHighlighted = new(false);
    public IReadOnlyReactiveProperty<bool> IsHighlighted => _isHighlighted;

    public void Init(Vector2Int coord)
    {
        _coord = coord;

        _isHighlighted
            .Subscribe(on =>
            {
                if (_renderer != null)
                    _renderer.color = on ? Color.blue : Color.white;
            })
            .AddTo(this);
    }

    public Vector2Int Coord => _coord;
    public Vector2 WorldPosition => transform.position;
}
