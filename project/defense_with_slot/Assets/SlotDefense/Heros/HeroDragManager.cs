using UnityEngine;

public class HeroDragManager : SingletonMono<HeroDragManager>
{
    private Hero _draggingHero;
    private Vector2 _dragStartPos;

    protected override bool UseDontDestroyOnLoad => false;
    protected override void Release() { }

    private void Update()
    {
        // ✅ UI 위 클릭이면 전부 무시
        if (MouseUtils.IsPointerOverUI()) return;

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

        var cm = bf.CellManager;
        if (cm == null) return;

        Vector2 worldPos = MouseUtils.GetPointerWorldPosition();

        var cell = cm.GetClosestCell(worldPos);
        if (cell == null) return;

        var hero = cm.GetOccupant(cell);
        if (hero == null) return;

        _draggingHero = hero;
        _draggingHero.SetDragging(true);   // ✅ 드래그 상태 ON
        _dragStartPos = hero.transform.position;
    }

    private void OnPointerDrag()
    {
        Vector2 worldPos = MouseUtils.GetPointerWorldPosition();
        _draggingHero.SnapTo(worldPos, 0f);
    }

    private void OnPointerUp()
    {
        var bf = FieldManager.Instance.CurBattleField;
        if (bf == null) { CancelDrag(); return; }

        var cm = bf.CellManager;
        if (cm == null) { CancelDrag(); return; }

        Vector2 worldPos = MouseUtils.GetPointerWorldPosition();
        var targetCell = cm.GetClosestCell(worldPos);
        if (targetCell == null) { CancelDrag(); return; }

        var prevCell = cm.GetHeroCell(_draggingHero);
        var occupant = cm.GetOccupant(targetCell);

        if (prevCell == targetCell)
        {
            _draggingHero.SnapTo(targetCell.WorldPosition);
            EndDrag();
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
            EndDrag();
            return;
        }

        // 빈 셀 배치
        if (cm.Occupy(_draggingHero, targetCell))
            _draggingHero.SnapTo(targetCell.WorldPosition);
        else
            _draggingHero.RevertTo(_dragStartPos);

        EndDrag();
    }

    private void CancelDrag()
    {
        if (_draggingHero != null)
            _draggingHero.RevertTo(_dragStartPos);
        EndDrag();
    }

    private void EndDrag()
    {
        if (_draggingHero != null)
            _draggingHero.SetDragging(false);  // ✅ 드래그 상태 OFF
        _draggingHero = null;
    }
}
