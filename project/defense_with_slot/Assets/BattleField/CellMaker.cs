using UnityEngine;

public class CellMaker : MonoBehaviour
{
    [SerializeField] private Cell _cellPrefab;
    [SerializeField] private Transform _cellParent;
    [SerializeField] private float _cellSize = 1.2f; // 칸 간격

    // 중심(center)을 기준으로 XY 평면에 그리드 배치, Z=0
    private Vector3 ComputeWorldPos(Vector2Int gridPos, int width, int height, Vector3 center)
    {
        float halfW = (width  - 1) * 0.5f;
        float halfH = (height - 1) * 0.5f;

        float x = (gridPos.x - halfW) * _cellSize + center.x;
        float y = (gridPos.y - halfH) * _cellSize + center.y;
        return new Vector3(x, y, 0f);
    }

    public Cell CreateCell(Vector2Int gridPos, int width, int height, Vector3 center)
    {
        Vector3 worldPos = ComputeWorldPos(gridPos, width, height, center);
        var cellObj = Object.Instantiate(_cellPrefab, worldPos, Quaternion.identity, _cellParent);
        cellObj.Init(gridPos, worldPos);
        return cellObj;
    }
}
