using UnityEngine;
using System;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float _speed = 2f;

    [Header("Visual (회전 대상)")]
    [SerializeField] private SpriteRenderer _renderer; // 바인딩(없으면 자동 탐색)
    [SerializeField] private float _spinSpeed = 90f;  // deg/sec

    private Transform _visual; // _renderer.transform 캐시

    private List<Vector2> _path;
    private int _currentIndex;
    private Action _onDie;

    private const float ArriveEps = 0.01f;

    private void Awake()
    {
        if (_renderer == null)
            _renderer = GetComponentInChildren<SpriteRenderer>();

        if (_renderer != null)
            _visual = _renderer.transform;
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

        // 1) 이동
        Vector2 target = _path[_currentIndex];
        transform.position = Vector2.MoveTowards(transform.position, target, _speed * Time.deltaTime);

        // 2) 도착 체크(오차 허용)
        if (((Vector2)transform.position - target).sqrMagnitude <= ArriveEps * ArriveEps)
        {
            _currentIndex++;
            if (_currentIndex >= _path.Count)
            {
                Die();
                return;
            }
        }

        // 3) 비주얼만 회전
        if (_visual != null && _spinSpeed != 0f)
            _visual.Rotate(0f, 0f, _spinSpeed * Time.deltaTime, Space.Self);
    }

    private void Die()
    {
        _onDie?.Invoke();
        Destroy(gameObject);
    }
}
