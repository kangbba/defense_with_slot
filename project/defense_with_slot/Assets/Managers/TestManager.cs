using UnityEngine;
using UnityEngine.UI;

public class TestManager : MonoBehaviour
{
    [SerializeField] private Button _spawnHeroButton;

    private void Awake()
    {
        if (_spawnHeroButton == null)
        {
            Debug.LogError("[TestManager] _spawnHeroButton 바인딩 안됨");
            return;
        }

        _spawnHeroButton.onClick.AddListener(OnSpawnHeroClicked);
    }

    private void OnSpawnHeroClicked()
    {
        HeroManager.Instance.MakeRandomHeroOnRandomCell(); 
        Debug.Log("[TestManager] 랜덤 타입 Hero 스폰 요청");
    }
}
