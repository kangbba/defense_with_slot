using UnityEngine;
using DG.Tweening;

public static class EffectUtility
{
    // 🔹 원형 스프라이트 캐싱
    private static Sprite _circleSprite;
    private static Sprite GetCircleSprite()
    {
        if (_circleSprite != null) return _circleSprite;

        Texture2D tex = new Texture2D(16, 16);
        Color[] pixels = new Color[16 * 16];
        Vector2 center = new Vector2(7.5f, 7.5f);
        float radius = 7.5f;

        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                pixels[y * 16 + x] = dist <= radius ? Color.white : Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;

        _circleSprite = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16);
        return _circleSprite;
    }

    /// <summary>
    /// 임시 이펙트 생성 (DOTween 애니메이션 포함)
    /// </summary>
    public static GameObject PlayEffect(Vector2 pos, Color color, float startScale, float endScale, float duration, int sortingOrder = 100)
    {
        GameObject fx = new GameObject("TempEffect");
        fx.transform.position = pos;

        var sr = fx.AddComponent<SpriteRenderer>();
        sr.sprite = GetCircleSprite();
        sr.color = color;
        sr.sortingOrder = sortingOrder;

        fx.transform.localScale = Vector3.one * startScale;

        // DOTween 애니메이션
        fx.transform.DOScale(endScale, duration).SetEase(Ease.OutQuad);
        sr.DOFade(0f, duration).OnComplete(() => Object.Destroy(fx));

        return fx;
    }
}
