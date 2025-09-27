using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class BattleFieldPath : MonoBehaviour
{
    [Header("기준 Rect (월드좌표, z무시)")]
    [SerializeField] private Rect _rect = new Rect(-4, -4, 8, 8);

    [Header("코너 라운드/세그먼트")]
    [Min(0f)][SerializeField] private float _cornerRadius = 1f;
    [Range(1, 32)][SerializeField] private int _cornerSegments = 6;

    [Header("경로 오프셋 (단위: 유닛)")]
    [Tooltip("0=기준 Rect, +1=바깥, -1=안쪽")]
    [SerializeField] private float _offsetUnits = 0f;

    private readonly List<Vector2> _pathCache = new();
    private Rect _lastRect;
    private float _lastRadius;
    private int _lastSeg;
    private float _lastOffset;

    public IReadOnlyList<Vector2> GetPath()
    {
        RebuildIfNeeded();
        return _pathCache;
    }

    public Vector2 GetStartPoint()
    {
        RebuildIfNeeded();
        return _pathCache.Count > 0 ? _pathCache[0] : Vector2.zero;
    }

    public void SetOffset(float units)
    {
        if (Mathf.Approximately(_offsetUnits, units)) return;
        _offsetUnits = units;
        Invalidate();
    }

    public void Invalidate() => _lastRect.width = float.NaN;

    private void RebuildIfNeeded()
    {
        if (_rect.Equals(_lastRect) &&
            Mathf.Approximately(_cornerRadius, _lastRadius) &&
            _cornerSegments == _lastSeg &&
            Mathf.Approximately(_offsetUnits, _lastOffset))
            return;

        _pathCache.Clear();
        _pathCache.AddRange(PathUtils.CreateRoundedRectPath(_rect, _cornerRadius, _cornerSegments, _offsetUnits));

        _lastRect = _rect;
        _lastRadius = _cornerRadius;
        _lastSeg = _cornerSegments;
        _lastOffset = _offsetUnits;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        RebuildIfNeeded();
        if (_pathCache.Count < 2) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < _pathCache.Count; i++)
        {
            Vector2 a = _pathCache[i];
            Vector2 b = _pathCache[(i + 1) % _pathCache.Count];
            Gizmos.DrawLine(a, b);
        }
    }
#endif
}
