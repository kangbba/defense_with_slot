using UnityEngine;
using UniRx;

[RequireComponent(typeof(BoxCollider2D))]
public class Draggable : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float _moveSmooth = 20f; // 높을수록 빠르게 따라감

    private Camera _cam;

    private readonly ReactiveProperty<bool> _isDragging = new(false);
    private readonly ReactiveProperty<Vector3> _currentWorldPos = new(Vector3.zero);

    public IReadOnlyReactiveProperty<bool> IsDragging => _isDragging;
    public IReadOnlyReactiveProperty<Vector3> CurrentWorldPos => _currentWorldPos;

    private void Awake()
    {
        _cam = Camera.main;
    }

    public void SetCamera(Camera cam) => _cam = cam;

    private void OnMouseDown()
    {
        if (TryGetMouseWorldOnZ0(out var p))
        {
            p.z = 0f;
            _currentWorldPos.Value = p;
            _isDragging.Value = true;
            transform.position = p; // 초기 튕김 방지
        }
    }

    private void OnMouseDrag()
    {
        if (!_isDragging.Value) return;
        if (TryGetMouseWorldOnZ0(out var p))
        {
            p.z = 0f;
            _currentWorldPos.Value = p;
        }
    }

    private void OnMouseUp()
    {
        if (!_isDragging.Value) return;
        _isDragging.Value = false;
    }

    private void OnDisable()
    {
        _isDragging.Value = false;
    }

    private void LateUpdate()
    {
        if (_isDragging.Value)
        {
            var to = _currentWorldPos.Value; to.z = 0f;
            transform.position = Vector3.Lerp(
                transform.position,
                to,
                Time.deltaTime * _moveSmooth
            );
        }
    }

    // 오쏘그래픽 카메라 기준: 마우스 스크린 좌표를 Z=0으로 투영
    private bool TryGetMouseWorldOnZ0(out Vector3 result)
    {
        result = Vector3.zero;
        var cam = _cam ? _cam : Camera.main;
        if (!cam) return false;

        // 현재 오브젝트의 스크린 Z를 기준으로 월드 변환
        float zRef = -cam.transform.position.z; // Z=0 평면까지의 카메라 거리
        var sp = Input.mousePosition;
        sp.z = zRef;

        result = cam.ScreenToWorldPoint(sp);
        result.z = 0f; // 2D 고정
        return true;
    }
}
