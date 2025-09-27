using UnityEngine;

// EnemyUIManager.cs
public class EnemyUIManager : SingletonMono<EnemyUIManager>
{
    [SerializeField] private HpBar _hpBarPrefab;
    [SerializeField] private Transform _uiRoot;

    protected override bool UseDontDestroyOnLoad => false;

    protected override void Release()
    {
    }

    /// <summary>
    /// 특정 Enemy에 HP Bar 생성 및 바인딩
    /// </summary>
    public HpBar MakeHpBarFor(Enemy enemy)
    {
        if (enemy == null) return null;

        var hpBar = Instantiate(_hpBarPrefab, _uiRoot);
        hpBar.Bind(enemy);
        return hpBar;
    }
}
