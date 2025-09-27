using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

public class FireHeroCombat : HeroCombat
{
    [Header("Fire Settings")]
    [SerializeField] private int damage = 8;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private float travelTime = 1.2f;   // 폭탄 날아가는 시간
    [SerializeField] private AnimationCurve arcCurve;   // 곡선 궤적 (Inspector에서 지정 가능)

    [Header("Projectile Visual")]
    [SerializeField] private GameObject bombVisualPrefab;
    [SerializeField] private Color explosionColor = new Color(1f, 0.5f, 0f, 0.8f);
    [SerializeField] private float effectScale = 1.5f;

    protected override async UniTask AttackAsync(Enemy target)
    {
        if (target == null) return;

        // 🔹 공격 시작 시점에 목표 위치 확정
        Vector3 startPos = transform.position;
        Vector3 targetPos = target.transform.position;

        // 🔹 경고 원 생성 (런타임 코드로 직접 생성)
        GameObject warningCircle = CreateWarningCircle(targetPos, explosionRadius);

        // 🔹 폭탄 비주얼 생성
        GameObject bomb = null;
        if (bombVisualPrefab != null)
            bomb = Object.Instantiate(bombVisualPrefab, startPos, Quaternion.identity);

        // 🔹 곡선 궤적 이동
        float elapsed = 0f;
        while (elapsed < travelTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / travelTime);

            Vector3 pos = Vector3.Lerp(startPos, targetPos, t);
            if (arcCurve != null)
            {
                float height = arcCurve.Evaluate(t);
                pos.y += height;
            }

            if (bomb != null)
                bomb.transform.position = pos;

            await UniTask.Yield();
        }

        // 🔹 폭발 이펙트
        EffectUtility.PlayEffect(targetPos, explosionColor, 0.3f, effectScale * 2f, 0.4f);

        // 🔹 범위 피해
        var hits = Physics2D.OverlapCircleAll(targetPos, explosionRadius);
        foreach (var h in hits)
        {
            var e = h.GetComponent<Enemy>();
            if (e != null) e.TakeDamage(damage);
        }

        // 🔹 정리
        if (bomb != null) Destroy(bomb);
        if (warningCircle != null) Destroy(warningCircle); // 폭발 시 경고 원 제거
    }

    /// <summary>
    /// 런타임에서 간단한 경고 원 생성
    /// </summary>
    private GameObject CreateWarningCircle(Vector3 position, float radius)
    {
        GameObject circle = new GameObject("WarningCircle");
        circle.transform.position = position;

        var sr = circle.AddComponent<SpriteRenderer>();
        sr.sprite = GenerateCircleSprite();
        sr.color = new Color(1f, 0f, 0f, 0.3f); // 반투명 빨강
        circle.transform.localScale = Vector3.one * radius * 2f;

        return circle;
    }

    /// <summary>
    /// 단순 원형 텍스처 생성 → Sprite 변환
    /// (최적화 필요하면 미리 에셋으로 준비 권장)
    /// </summary>
    private Sprite GenerateCircleSprite()
    {
        Texture2D tex = new Texture2D(64, 64);
        Color[] colors = new Color[64 * 64];

        Vector2 center = new Vector2(32, 32);
        float r = 30f;

        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist <= r) colors[y * 64 + x] = Color.white;
                else colors[y * 64 + x] = Color.clear;
            }
        }

        tex.SetPixels(colors);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
    }
}
