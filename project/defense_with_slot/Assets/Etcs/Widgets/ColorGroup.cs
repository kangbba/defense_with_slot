using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using DG.Tweening;

[DisallowMultipleComponent]
public class ColorGroup : MonoBehaviour
{

    [SerializeField]
    private Color _color = Color.white;

    public Color Color => _color;

    // 내부 렌더러 캐시
    private readonly List<Graphic> _graphics = new();
    private readonly List<SpriteRenderer> _spriteRenderers = new();
    private readonly List<TextMeshProUGUI> _tmpTexts = new();

    private void Awake()
    {
        CacheRenderers(force: true);
        ApplyColorInternal(_color);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            CacheRenderers(force: true);
            ApplyColorInternal(_color);
        }
    }

    private void OnEditorColorChanged()
    {
        if (!Application.isPlaying)
        {
            CacheRenderers(force: true);
            ApplyColorInternal(_color);
        }
    }
    
    private void ForceApply()
    {
        CacheRenderers(force: true);
        ApplyColorInternal(_color);
    }
#endif

    /// <summary>
    /// 외부 호출용: 즉시 색상 반영
    /// </summary>
    public void SetColorGroupInstant(Color color)
    {
        _color = color;
        CacheRenderers(force: false);
        ApplyColorInternal(color);
    }

    /// <summary>
    /// 하위 렌더러 캐시
    /// </summary>
    private void CacheRenderers(bool force)
    {
        if (_graphics.Count > 0 && !force) return;

        _graphics.Clear();
        _spriteRenderers.Clear();
        _tmpTexts.Clear();

        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t.TryGetComponent(out Graphic g)) _graphics.Add(g);
            if (t.TryGetComponent(out SpriteRenderer sr)) _spriteRenderers.Add(sr);
            if (t.TryGetComponent(out TextMeshProUGUI tmp)) _tmpTexts.Add(tmp);
        }
    }

    /// <summary>
    /// 내부 적용 로직 (알파 유지)
    /// </summary>
    private void ApplyColorInternal(Color color)
    {
        foreach (var g in _graphics)
        {
            if (g == null) continue;
            Color c = g.color;
            g.color = new Color(color.r, color.g, color.b, c.a);
        }

        foreach (var sr in _spriteRenderers)
        {
            if (sr == null) continue;
            Color c = sr.color;
            sr.color = new Color(color.r, color.g, color.b, c.a);
        }

        foreach (var tmp in _tmpTexts)
        {
            if (tmp == null) continue;
            Color c = tmp.color;
            tmp.color = new Color(color.r, color.g, color.b, c.a);
        }
    }
}
