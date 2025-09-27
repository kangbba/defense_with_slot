using UnityEngine;
using System;
using System.Collections.Generic;
using UniRx;
using Cysharp.Threading.Tasks;
using System.Threading;
using DG.Tweening; // ✅ DOTween 사용

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int _maxHp = 10;
    [SerializeField] private float _baseSpeed = 2f;
    [SerializeField] private float _spinSpeed = 90f;
    [SerializeField] private SpriteRenderer _renderer;

    public ReactiveProperty<int> Hp { get; private set; }
    public ReactiveProperty<bool> IsPoisoned { get; private set; } = new(false);
    public ReactiveProperty<bool> IsSlowed { get; private set; } = new(false);

    private Transform _visual;
    private List<Vector2> _path;
    private int _currentIndex;
    private Action _onDie;

    private float _currentSpeed;
    private CancellationTokenSource _statusCts;

    private const float ArriveEps = 0.01f;

    private void Awake()
    {
        if (_renderer == null)
            _renderer = GetComponentInChildren<SpriteRenderer>();
        if (_renderer != null)
            _visual = _renderer.transform;

        Hp = new ReactiveProperty<int>(_maxHp);
        _currentSpeed = _baseSpeed;

        Hp.Where(cur => cur <= 0)
          .Subscribe(_ => Die())
          .AddTo(this);
    }

    public void Init(List<Vector2> path, Action onDie)
    {
        _path = path;
        _currentIndex = 0;
        _onDie = onDie;

        if (_path != null && _path.Count > 0)
            transform.position = _path[0];
    }

    private void Update()
    {
        if (_path == null || _path.Count == 0) return;

        Vector2 target = _path[_currentIndex];
        transform.position = Vector2.MoveTowards(transform.position, target, _currentSpeed * Time.deltaTime);

        if (((Vector2)transform.position - target).sqrMagnitude <= ArriveEps * ArriveEps)
        {
            _currentIndex++;
            if (_currentIndex >= _path.Count)
            {
                Die();
                return;
            }
        }

        if (_visual != null && _spinSpeed != 0f)
            _visual.Rotate(0f, 0f, _spinSpeed * Time.deltaTime, Space.Self);
    }

    public void TakeDamage(int dmg)
    {
        if (Hp.Value <= 0) return;
        Hp.Value = Mathf.Max(0, Hp.Value - dmg);
    }

    // ✅ 독 (중첩 없음)
    public void ApplyPoison(int tickDamage, float duration, float interval)
    {
        if (IsPoisoned.Value) return;

        IsPoisoned.Value = true;
        var cts = new CancellationTokenSource();
        _statusCts = cts;

        RunPoison(tickDamage, duration, interval, cts.Token).Forget();
    }

    private async UniTaskVoid RunPoison(int tickDamage, float duration, float interval, CancellationToken token)
    {
        float elapsed = 0f;
        while (elapsed < duration && !token.IsCancellationRequested && Hp.Value > 0)
        {
            TakeDamage(tickDamage);
            await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: token);
            elapsed += interval;
        }

        IsPoisoned.Value = false;
    }

    // ✅ 슬로우 (기본 속도의 0.9배)
    public void ApplySlow(float factor, float duration)
    {
        // ResetStatus();
        // IsSlowed.Value = true;

        // _statusCts = new CancellationTokenSource();
        // RunSlow(factor, duration, _statusCts.Token).Forget();
    }

    private async UniTaskVoid RunSlow(float factor, float duration, CancellationToken token)
    {
        _currentSpeed = _baseSpeed * factor;
        await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: token);
        _currentSpeed = _baseSpeed;
        IsSlowed.Value = false;
    }

    // ✅ 넉백 (이동 경로는 유지, 비주얼만 튕겼다 돌아옴)
    public void ApplyKnockback(Vector2 direction, float distance, float duration)
    {
        if (_visual == null) return;

        ResetStatus();

        Vector3 startPos = Vector3.zero;
        Vector3 knockPos = startPos + (Vector3)direction.normalized * distance;

        _visual.localPosition = startPos;

        // DOTween 시퀀스로 밀렸다가 복귀
        Sequence seq = DOTween.Sequence();
        seq.Append(_visual.DOLocalMove(knockPos, duration * 0.4f).SetEase(Ease.OutQuad));
        seq.Append(_visual.DOLocalMove(startPos, duration * 0.6f).SetEase(Ease.InQuad));
    }

    private void ResetStatus()
    {
        _statusCts?.Cancel();
        _statusCts?.Dispose();
        _statusCts = null;

        _currentSpeed = _baseSpeed;
        IsSlowed.Value = false;

        if (_visual != null)
            _visual.localPosition = Vector3.zero;
    }

    private void Die()
    {
        _onDie?.Invoke();
        _statusCts?.Cancel();
        Destroy(gameObject);
    }
}
