using UnityEngine;

public class MergeManager : SingletonMono<MergeManager>
{
    protected override bool UseDontDestroyOnLoad => false;
    protected override void Release() { }

    public void TryMerge(Hero hero, Hero other)
    {
        if (hero == null || other == null)
        {
            Debug.LogWarning("[MergeManager] 병합 실패: Hero가 null임");
            return;
        }
        if (hero.Type != other.Type)
        {
            Debug.LogWarning($"[MergeManager] 병합 실패: 타입 불일치 ({hero.Type} vs {other.Type})");
            return;
        }
        if (hero.Level != other.Level)
        {
            Debug.LogWarning($"[MergeManager] 병합 실패: 레벨 불일치 (Lv.{hero.Level} vs Lv.{other.Level})");
            return;
        }

        var bf = FieldManager.Instance?.CurBattleField;
        if (bf == null)
        {
            Debug.LogWarning("[MergeManager] 병합 실패: BattleField 없음");
            return;
        }

        var cellMgr = bf.CellManager;
        if (cellMgr == null)
        {
            Debug.LogWarning("[MergeManager] 병합 실패: CellManager 없음");
            return;
        }

        var cell = cellMgr.GetHeroCell(other);
        if (cell == null)
        {
            Debug.LogWarning("[MergeManager] 병합 실패: 대상 Hero가 셀에 없음");
            return;
        }

        int newLevel = hero.Level + 1;
        HeroType type = hero.Type;

        // 안전하게 제거
        cellMgr.RemoveHero(hero);
        cellMgr.RemoveHero(other);

        // 새 히어로 배치
        HeroManager.Instance.SpawnHeroAtCell(type, newLevel, cell);

        Debug.Log($"[MergeManager] {type} Lv.{hero.Level}+{other.Level} → Lv.{newLevel} 병합 성공!");
    }
}
