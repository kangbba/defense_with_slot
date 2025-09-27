using UnityEngine;

public class MergeManager : SingletonMono<MergeManager>
{
    protected override bool UseDontDestroyOnLoad => false;
    protected override void Release() { }

  public void TryMerge(Hero hero, Hero other)
    {
        if (hero == null || other == null) return;
        if (hero.Type != other.Type) return;
        if (hero.Level != other.Level) return;

        var bf = FieldManager.Instance?.CurBattleField;
        if (bf == null) return;

        var cell = bf.GetHeroCell(other);
        if (cell == null) return;

        int newLevel = hero.Level + 1;
        HeroType type = hero.Type;

        // 안전하게 제거
        FieldManager.Instance.CurBattleField.RemoveHero(hero);
        FieldManager.Instance.CurBattleField.RemoveHero(other);

        // 새 히어로 배치
        HeroManager.Instance.SpawnHeroAtCell(type, newLevel, cell);

        Debug.Log($"[MergeManager] {type} Lv.{hero.Level}+{other.Level} → Lv.{newLevel} 병합!");
    }


}
