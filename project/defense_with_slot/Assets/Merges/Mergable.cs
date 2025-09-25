// Mergable.cs — ReactiveProperty 제거, 일반 필드 + 캡슐화(+인스펙터 노출)
using UnityEngine;

public abstract class Mergable : MonoBehaviour
{
    [SerializeField] private int _level = 1; // Show in Inspector (private)
    public int Level => _level;              // 읽기 전용 공개

    public virtual bool CanMergeWith(Mergable other) =>
        other != null && _level == other._level;

    public void SetLevel(int lv)
    {
        _level = Mathf.Max(1, lv);
    }

    public void LevelUp()
    {
        _level++;
    }
}
