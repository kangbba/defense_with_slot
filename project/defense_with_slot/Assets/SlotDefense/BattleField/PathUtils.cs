// PathUtils.cs (코너/각도 수정: 시계방향, 바람개비 현상 제거)
using System.Collections.Generic;
using UnityEngine;

public static class PathUtils
{
    /// <summary>
    /// 라운드 사각형 경로 생성 (시작점: 좌상단 모서리 직선 시작, 시계방향)
    /// offsetUnits: 0=기준, +n=바깥으로 n, -n=안쪽으로 n
    /// </summary>
    public static List<Vector2> CreateRoundedRectPath(Rect baseRect, float cornerRadius, int cornerSegments, float offsetUnits = 0f)
    {
        var points = new List<Vector2>(cornerSegments * 4 + 8);

        // 1) Rect 팽창/수축
        Rect rect = InflateRect(baseRect, offsetUnits);

        // 2) 코너 반경 평행 오프셋 & Clamp
        float r = Mathf.Max(0f, cornerRadius + offsetUnits);
        float maxR = Mathf.Min(Mathf.Abs(rect.width), Mathf.Abs(rect.height)) * 0.5f - 1e-3f;
        r = Mathf.Clamp(r, 0f, Mathf.Max(0f, maxR));
        int seg = Mathf.Max(1, cornerSegments);

        // 3) 코너 중심 (좌표계: x→오른쪽, y→위)
        Vector2 TL = new(rect.xMin + r, rect.yMax - r);
        Vector2 TR = new(rect.xMax - r, rect.yMax - r);
        Vector2 BR = new(rect.xMax - r, rect.yMin + r);
        Vector2 BL = new(rect.xMin + r, rect.yMin + r);

        // 4) 시작점: 좌상단 직선 시작 (xMin+r, yMax)
        Vector2 startTopLeft = new(rect.xMin + r, rect.yMax);
        points.Add(startTopLeft);

        // ─ Top edge → (xMax - r, yMax)
        Vector2 endTop = new(rect.xMax - r, rect.yMax);
        if (endTop != points[^1]) points.Add(endTop);

        // ↷ TR corner arc: 90° → 0°
        AddArc(points, TR, r, 90f, 0f, seg);

        // │ Right edge ↓ → (xMax, yMin + r)
        Vector2 endRight = new(rect.xMax, rect.yMin + r);
        if (endRight != points[^1]) points.Add(endRight);

        // ↷ BR corner arc: 0° → -90°(=270°)
        AddArc(points, BR, r, 0f, -90f, seg);

        // ─ Bottom edge ← → (xMin + r, yMin)
        Vector2 endBottom = new(rect.xMin + r, rect.yMin);
        if (endBottom != points[^1]) points.Add(endBottom);

        // ↷ BL corner arc: -90°(=270°) → -180°(=180°)
        AddArc(points, BL, r, -90f, -180f, seg);

        // │ Left edge ↑ → (xMin, yMax - r)
        Vector2 endLeft = new(rect.xMin, rect.yMax - r);
        if (endLeft != points[^1]) points.Add(endLeft);

        // ↷ TL corner arc: 180° → 90°
        AddArc(points, TL, r, 180f, 90f, seg);

        return points;
    }

    private static Rect InflateRect(Rect rect, float d)
    {
        rect.xMin -= d; rect.xMax += d;
        rect.yMin -= d; rect.yMax += d;
        return rect;
    }

    private static void AddArc(List<Vector2> list, Vector2 center, float r, float startDeg, float endDeg, int segments)
    {
        if (r <= 0f)
        {
            // 반경 0이면 코너는 모서리 점으로 대체 (center에서 각도 보정 없이 한 점)
            list.Add(center);
            return;
        }

        float start = startDeg * Mathf.Deg2Rad;
        float end   = endDeg   * Mathf.Deg2Rad;

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float a = Mathf.Lerp(start, end, t);
            Vector2 p = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r;
            if (list.Count == 0 || list[^1] != p)
                list.Add(p);
        }
    }
}
