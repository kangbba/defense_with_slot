using UnityEngine;

public class FieldManager : SingletonMono<FieldManager>
{
    [SerializeField] private BattleField _battleFieldPrefab;

    // 단일 BattleField 참조
    public BattleField CurBattleField { get; private set; }

    protected override bool UseDontDestroyOnLoad => false;
    protected override void Release() { }

    public BattleField MakeBattleField()
    {
        if (_battleFieldPrefab == null)
        {
            Debug.LogError("[FieldManager] BattleField 프리팹이 바인딩 안됨");
            return null;
        }

        // 기존 필드 정리
        if (CurBattleField != null)
        {
            Destroy(CurBattleField.gameObject);
            CurBattleField = null;
        }

        // 새 필드 생성
        var field = Instantiate(_battleFieldPrefab, Vector3.zero, Quaternion.identity);
        field.Init(4, 4);

        CurBattleField = field;
        Debug.Log("[FieldManager] BattleField 인스턴스화 완료");
        return field;
    }
}
