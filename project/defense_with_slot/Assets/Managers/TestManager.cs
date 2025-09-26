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
        // 🔹 Null 체크
        if (_fireButton == null ||
            _elementButton == null ||
            _lightningButton == null ||
            _iceButton == null ||
            _poisonButton == null ||
            _rockButton == null ||
            _randomButton == null)
        {
            Debug.LogError("[TestManager] 버튼 바인딩 누락 있음");
            return;
        }

        // 🔹 리스너 바인딩
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
        var field = FieldManager.Instance.CurrentField;
        if (field == null)
        {
            Debug.LogError("[TestManager] 필드가 없음");
            return;
        }

        var cell = field.GetRandomEmptyCell();
        if (cell == null)
        {
            Debug.LogWarning("[TestManager] 빈 셀이 없음");
            return;
        }

        HeroManager.Instance.SpawnHeroAtCell(type, 1, cell);
        Debug.Log($"[TestManager] {type}Hero 스폰 요청");
    }

    private void OnSpawnRandomHeroClicked()
    {
        HeroManager.Instance.MakeRandomHeroOnRandomCell(); 
        Debug.Log("[TestManager] 랜덤 Hero 스폰 요청");
    }
}
