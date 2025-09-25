// FieldManager.cs — 생성/파괴만 책임
using UnityEngine;

public class FieldManager : SingletonMono<FieldManager>
{
    [SerializeField] private BattleField _battleFieldPrefab;
    private BattleField _currentField;

    public BattleField CurrentField => _currentField;

    protected override bool UseDontDestroyOnLoad => false;
    protected override void Release() { }

    public void MakeBattleField()
    {
        if (_battleFieldPrefab == null)
        {
            Debug.LogError("[FieldManager] BattleField 프리팹이 바인딩 안됨");
            return;
        }

        if (_currentField != null)
        {
            Destroy(_currentField.gameObject);
            _currentField = null;
        }

        _currentField = Instantiate(_battleFieldPrefab, Vector3.zero, Quaternion.identity);
        _currentField.transform.localPosition = Vector3.zero;
        _currentField.Init(4, 4);
        Debug.Log("[FieldManager] BattleField 프리팹 인스턴스화 완료");
    }


}
