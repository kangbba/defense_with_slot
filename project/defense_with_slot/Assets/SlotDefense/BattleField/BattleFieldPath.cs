using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public class BattleFieldPath : MonoBehaviour
{
    [Header("정사각형 Size (한 변 길이, 단위: 유닛)")]
    [Min(0.1f)] [SerializeField] private float _size = 8f;

    [Header("코너 라운드/세그먼트")]
    [Min(0f)] [SerializeField] private float _cornerRadius = 1f;
    [Range(1, 32)] [SerializeField] private int _cornerSegments = 6;

    [Header("경로 오프셋 (단위: 유닛)")]
    [Tooltip("0=기준 Rect, +1=바깥, -1=안쪽")]
    [SerializeField] private float _offsetUnits = 0f;

    [Header("Line Renderer (바인딩)")]
    [SerializeField] private LineRenderer _lineRenderer;

    private readonly List<Vector2> _pathCache = new();
    private float _lastSize;
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

    public void Invalidate() => _lastSize = float.NaN;

    private void RebuildIfNeeded()
    {
        if (Mathf.Approximately(_size, _lastSize) &&
            Mathf.Approximately(_cornerRadius, _lastRadius) &&
            _cornerSegments == _lastSeg &&
            Mathf.Approximately(_offsetUnits, _lastOffset))
            return;

        // ✅ 정사각형 Rect 자동 생성
        float half = _size * 0.5f;
        Rect rect = new Rect(-half, -half, _size, _size);

        _pathCache.Clear();
        _pathCache.AddRange(PathUtils.CreateRoundedRectPath(rect, _cornerRadius, _cornerSegments, _offsetUnits));

        _lastSize = _size;
        _lastRadius = _cornerRadius;
        _lastSeg = _cornerSegments;
        _lastOffset = _offsetUnits;

        UpdateLineRenderer();
    }

    private void UpdateLineRenderer()
    {
        if (_lineRenderer == null) return;

        if (_pathCache.Count < 2)
        {
            _lineRenderer.positionCount = 0;
            return;
        }

        _lineRenderer.loop = true;
        _lineRenderer.useWorldSpace = false;

        _lineRenderer.positionCount = _pathCache.Count;
        for (int i = 0; i < _pathCache.Count; i++)
            _lineRenderer.SetPosition(i, _pathCache[i]);
    }

    private void OnValidate()
    {
        Invalidate();
        RebuildIfNeeded();
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            RebuildIfNeeded();
        }
#endif
    }
}
