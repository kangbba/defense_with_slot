using UnityEngine;
using UnityEngine.UI;

public class TestManager : MonoBehaviour
{
    [Header("Hero Spawn Buttons")]
    [SerializeField] private Button _fireButton;
    [SerializeField] private Button _elementButton;
    [SerializeField] private Button _lightningButton;
    [SerializeField] private Button _iceButton;
    [SerializeField] private Button _poisonButton;
    [SerializeField] private Button _rockButton;
    [SerializeField] private Button _randomButton;

    private void Awake()
    {
        _fireButton.onClick.AddListener(() => SpawnSpecificHero(HeroType.Fire));
        _elementButton.onClick.AddListener(() => SpawnSpecificHero(HeroType.Element));
        _lightningButton.onClick.AddListener(() => SpawnSpecificHero(HeroType.Lightning));
        _iceButton.onClick.AddListener(() => SpawnSpecificHero(HeroType.Ice));
        _poisonButton.onClick.AddListener(() => SpawnSpecificHero(HeroType.Poison));
        _rockButton.onClick.AddListener(() => SpawnSpecificHero(HeroType.Rock));
        _randomButton.onClick.AddListener(OnSpawnRandomHeroClicked);
    }

    private void SpawnSpecificHero(HeroType type)
    {
        var bf = FieldManager.Instance.CurBattleField;
        var cell = bf?.CellManager?.GetRandomEmptyCell();
        if (cell == null) return;

        HeroManager.Instance.SpawnHeroAtCell(type, 1, cell);
    }

    private void OnSpawnRandomHeroClicked()
    {
        HeroManager.Instance.MakeRandomHeroOnRandomCell();
    }
}
