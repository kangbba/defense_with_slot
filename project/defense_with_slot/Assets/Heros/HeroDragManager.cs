// HeroDragManager.cs — 전역 드래그/머지 컨트롤러 (UI 클릭 무시 지원)
using UnityEngine;
using UnityEngine.EventSystems;

public class HeroDragManager : SingletonMono<HeroDragManager>
{
    private Hero _draggingHero;
    private Vector2 _dragStartPos;

    protected override bool UseDontDestroyOnLoad => false;
    protected override void Release() { }

    private void Update()
    {
        // ✅ UI 위 클릭이면 전부 무시 (버튼 클릭 시 히어로가 끌려가는 문제 방지)
        if (IsPointerOverUI()) return;

        if (Input.GetMouseButtonDown(0))
        {
            OnPointerDown();
        }
        else if (Input.GetMouseButton(0) && _draggingHero != null)
        {
            OnPointerDrag();
        }
        else if (Input.GetMouseButtonUp(0) && _draggingHero != null)
        {
            OnPointerUp();
        }
    }

    private void OnPointerDown()
    {
        var bf = FieldManager.Instance.CurBattleField;
        if (bf == null) return;

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 마우스 아래 셀/히어로 찾기
        var cell = bf.GetClosestCell(worldPos);
        if (cell == null) return;

        var hero = bf.GetOccupant(cell);
        if (hero == null) return;

        _draggingHero = hero;
        _dragStartPos = hero.transform.position;
        // 필요하면 선택 이펙트 등
    }

    private void OnPointerDrag()
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _draggingHero.SnapTo(worldPos, 0f);
    }

    private void OnPointerUp()
    {
        var bf = FieldManager.Instance.CurBattleField;
        if (bf == null) { CancelDrag(); return; }

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var targetCell = bf.GetClosestCell(worldPos);
        if (targetCell == null) { CancelDrag(); return; }

        var prevCell = bf.GetHeroCell(_draggingHero);
        var occupant = bf.GetOccupant(targetCell);

        if (prevCell == targetCell)
        {
            _draggingHero.SnapTo(targetCell.WorldPosition);
            _draggingHero = null;
            return;
        }

        if (occupant != null && occupant != _draggingHero)
        {
            if (occupant.Type == _draggingHero.Type)
            {
                // 병합 시도
                _draggingHero.RevertTo(_dragStartPos);
                MergeManager.Instance.TryMerge(_draggingHero, occupant);
            }
            else
            {
                // 다른 타입이면 배치 불가
                _draggingHero.RevertTo(_dragStartPos);
            }
            _draggingHero = null;
            return;
        }

        // 빈 셀 배치
        if (bf.Occupy(_draggingHero, targetCell))
            _draggingHero.SnapTo(targetCell.WorldPosition);
        else
            _draggingHero.RevertTo(_dragStartPos);

        _draggingHero = null;
    }

    private void CancelDrag()
    {
        if (_draggingHero != null)
            _draggingHero.RevertTo(_dragStartPos);
        _draggingHero = null;
    }

    // ─────────────────────────────────────────────
    // UI 위 클릭 감지 (마우스/터치 모두 지원)
    // ─────────────────────────────────────────────
    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount > 0)
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        return false;
#else
        return EventSystem.current.IsPointerOverGameObject();
#endif
    }
}
