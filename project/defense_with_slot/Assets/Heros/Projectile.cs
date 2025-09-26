using UnityEngine;
using System;

public enum ProjectileType
{
    Straight,
    Toward,
    Bomb,
    Homing
}

public class Projectile : MonoBehaviour
{
    private ProjectileType _type;

    private Vector2 _direction;
    private Vector2 _startPos;
    private Vector2 _targetPos;
    private Enemy _homingTarget;
    private Vector2 _lastTargetPos;

    private float _speed;
    private float _lifeTime;
    private int _remainingHitCount;
    private int _damage;
    private float _explosionRadius;
    private float _arcHeight;

    private Action<Enemy> _onHit;
    private Color _explosionColor;

    private float _elapsed;

    // -------------------------
    // 직선 발사
    // -------------------------
    public void Launch(Vector2 dir, float speed, float lifeTime, int maxHitCount, int damage, Action<Enemy> onHit)
    {
        _type = ProjectileType.Straight;

        _direction = dir.normalized;
        _speed = speed;
        _lifeTime = lifeTime;
        _remainingHitCount = maxHitCount;
        _damage = damage;
        _onHit = onHit;

        Destroy(gameObject, _lifeTime);
    }

    // -------------------------
    // 특정 위치로 발사 (투척)
    // -------------------------
    public void LaunchToward(Vector2 targetPos, float speed, float lifeTime, int damage, Action<Enemy> onHit)
    {
        _type = ProjectileType.Toward;

        _targetPos = targetPos;
        _speed = speed;
        _lifeTime = lifeTime;
        _remainingHitCount = 1;
        _damage = damage;
        _onHit = onHit;

        Destroy(gameObject, _lifeTime);
    }

    // -------------------------
    // 폭탄 투척 (포물선 + 폭발)
    // -------------------------
    public void LaunchBomb(Vector2 targetPos, float travelTime, float arcHeight,
                           int damage, int maxHitCount, float explosionRadius,
                           Color explosionColor, Action<Enemy> onHit)
    {
        _type = ProjectileType.Bomb;

        _startPos = transform.position;
        _targetPos = targetPos;
        _lifeTime = travelTime;
        _elapsed = 0f;
        _arcHeight = arcHeight;
        _damage = damage;
        _remainingHitCount = maxHitCount;
        _explosionRadius = explosionRadius;
        _explosionColor = explosionColor;
        _onHit = onHit;
    }

    // -------------------------
    // 유도탄 (타겟 따라감, 무조건 명중)
    // -------------------------
    public void LaunchHoming(Enemy target, float speed, float lifeTime, int damage, int maxHitCount, Action<Enemy> onHit)
    {
        _type = ProjectileType.Homing;

        _homingTarget = target;
        if (target != null) _lastTargetPos = target.transform.position;

        _speed = speed;
        _lifeTime = lifeTime;
        _damage = damage;
        _remainingHitCount = maxHitCount;
        _onHit = onHit;

        Destroy(gameObject, _lifeTime);
    }

    // -------------------------
    // Update
    // -------------------------
    private void Update()
    {
        switch (_type)
        {
            case ProjectileType.Straight:
                transform.position = new Vector3(
                    transform.position.x + _direction.x * (_speed * Time.deltaTime),
                    transform.position.y + _direction.y * (_speed * Time.deltaTime),
                    0f
                );
                break;

            case ProjectileType.Toward:
                transform.position = Vector2.MoveTowards(transform.position, _targetPos, _speed * Time.deltaTime);
                transform.position = new Vector3(transform.position.x, transform.position.y, 0f);
                if (((Vector2)transform.position - _targetPos).sqrMagnitude < 0.01f)
                    Destroy(gameObject);
                break;

            case ProjectileType.Bomb:
                _elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(_elapsed / _lifeTime);

                Vector2 pos = Vector2.Lerp(_startPos, _targetPos, t);
                pos.y += _arcHeight * Mathf.Sin(Mathf.PI * t);
                transform.position = new Vector3(pos.x, pos.y, 0f);

                if (t >= 1f)
                    Explode();
                break;

            case ProjectileType.Homing:
                if (_homingTarget == null || _homingTarget.Hp.Value <= 0)
                {
                    transform.position = new Vector3(_lastTargetPos.x, _lastTargetPos.y, 0f);
                    Destroy(gameObject);
                    return;
                }

                _lastTargetPos = _homingTarget.transform.position;
                Vector2 dir = (_lastTargetPos - (Vector2)transform.position).normalized;
                transform.position = new Vector3(
                    transform.position.x + dir.x * (_speed * Time.deltaTime),
                    transform.position.y + dir.y * (_speed * Time.deltaTime),
                    0f
                );
                break;
        }
    }

    // -------------------------
    // 폭탄 폭발 처리
    // -------------------------
    private void Explode()
    {
        EffectUtility.PlayEffect(transform.position, _explosionColor, 0.3f, 1.5f, 0.5f);

        int hitCount = 0;
        var hits = Physics2D.OverlapCircleAll(transform.position, _explosionRadius);
        foreach (var h in hits)
        {
            var e = h.GetComponent<Enemy>();
            if (e != null)
            {
                _onHit?.Invoke(e);
                hitCount++;
                if (hitCount >= _remainingHitCount)
                    break;
            }
        }
        Destroy(gameObject);
    }

    // -------------------------
    // 충돌 처리 (직선/호밍용)
    // -------------------------
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_type == ProjectileType.Bomb || _type == ProjectileType.Toward) return;

        var enemy = other.GetComponent<Enemy>();
        if (enemy == null) return;

        _onHit?.Invoke(enemy);

        _remainingHitCount--;
        if (_remainingHitCount <= 0)
            Destroy(gameObject);
    }
}
