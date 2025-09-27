// PoisonHeroCombat.cs
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using DG.Tweening; // DOTween 사용 (구름 페이드용)

public class PoisonHeroCombat : HeroCombat
{
    [Header("Poison Settings")]
    [SerializeField] private int   impactDamage        = 5;
    [SerializeField] private float poisonRadius        = 2f;
    [SerializeField] private float poisonDuration      = 5f;
    [SerializeField] private int   poisonTickDamage    = 1;
    [SerializeField] private float poisonTickInterval  = 1f;
    [SerializeField] private float travelTime          = 0.8f;
    [SerializeField] private Projectile projectilePrefab;

    [Header("Effect Settings")]
    [SerializeField] private Color poisonColor = new Color(0.6f, 0.1f, 0.7f, 0.8f);
    [SerializeField] private float effectScale = 1.5f;

    protected override async UniTask AttackAsync(Enemy target)
    {
        if (target == null) return;

        Vector2 targetPos = target.transform.position;
        Vector2 spawnPos  = transform.position;

        // 투척체 비주얼
        if (projectilePrefab != null)
        {
            var proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            proj.LaunchToward(
                targetPos,
                speed: Vector2.Distance(spawnPos, targetPos) / travelTime,
                lifeTime: travelTime,
                damage: 0,
                onHit: _ => { }
            );
        }

        // 투척 시간 대기
        await UniTask.Delay(TimeSpan.FromSeconds(travelTime));

        // 구름 지속 피해 루프 실행
        RunPoisonCloud(targetPos, poisonColor, poisonDuration,
                       poisonTickInterval, poisonTickDamage,
                       impactDamage, effectScale * 2f).Forget();
    }

    // ===== 지속성 구름 처리 =====
    private async UniTaskVoid RunPoisonCloud(
        Vector3 pos, Color color, float duration, float interval,
        int tickDamage, int impactDamage, float worldScale)
    {
        var cloud = SpawnPoisonCloud(pos, color, duration, worldScale);

        float elapsed = 0f;
        bool firstTick = true;

        while (elapsed < duration)
        {
            var hits = Physics2D.OverlapCircleAll(pos, poisonRadius);
            foreach (var h in hits)
            {
                var e = h.GetComponent<Enemy>();
                if (e == null) continue;

                if (firstTick)
                {
                    // 첫 틱: 즉발 피해 + DOT 부여
                    e.TakeDamage(impactDamage);
                    e.ApplyPoison(tickDamage, duration, interval);
                }
                else
                {
                    // 이후 틱: DOT은 중첩 방지, 대신 즉발 틱 피해만
                    e.TakeDamage(tickDamage);
                }
            }

            firstTick = false;
            await UniTask.Delay(TimeSpan.FromSeconds(interval));
            elapsed += interval;
        }

        if (cloud != null) Destroy(cloud);
    }

    // ===== 시각 효과 =====
    private GameObject SpawnPoisonCloud(Vector3 pos, Color color, float duration, float worldScale)
    {
        var go = new GameObject("PoisonCloud");
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GenerateSoftCircleSprite();
        sr.color  = new Color(color.r, color.g, color.b, 0f);
        go.transform.localScale = Vector3.one * worldScale;

        float fadeIn  = Mathf.Min(0.15f, duration * 0.1f);
        float hold    = Mathf.Max(0f, duration - fadeIn - 0.2f);
        float fadeOut = 0.2f;

        var seq = DOTween.Sequence();
        seq.Append(sr.DOFade(color.a, fadeIn));
        if (hold > 0f) seq.AppendInterval(hold);
        seq.Append(sr.DOFade(0f, fadeOut));

        return go;
    }

    // 간단한 소프트 원 스프라이트 생성
    private Sprite GenerateSoftCircleSprite(int size = 96, float feather = 6f)
    {
        var tex = new Texture2D(size, size, TextureFormat.ARGB32, false) { filterMode = FilterMode.Bilinear };
        var cols = new Color[size * size];
        Vector2 c = new Vector2(size - 1, size - 1) * 0.5f;
        float r   = (size - 2) * 0.5f;

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Vector2.Distance(new Vector2(x, y), c);
            float t = Mathf.InverseLerp(r, r - feather, d);
            float a = Mathf.Clamp01(1f - t);
            cols[y * size + x] = new Color(1f, 1f, 1f, a);
        }

        tex.SetPixels(cols);
        tex.Apply(false, false);
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
