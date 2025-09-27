using UnityEngine;
using UniRx;

public class DataManager : SingletonMono<DataManager>
{
    private const string COIN_KEY = "PLAYER_COIN";

    private readonly ReactiveProperty<int> _coin = new(0);
    public IReadOnlyReactiveProperty<int> Coin => _coin;

    protected override bool UseDontDestroyOnLoad => true;
    protected override void Release() { }

    protected override void Awake()
    {
        base.Awake();
        // 시작 시 PlayerPrefs에서 불러오기
        _coin.Value = PlayerPrefs.GetInt(COIN_KEY, 0);
    }

    public void AddCoin(int amount)
    {
        _coin.Value += amount;
        PlayerPrefs.SetInt(COIN_KEY, _coin.Value);
        PlayerPrefs.Save();
    }

    public bool SpendCoin(int amount)
    {
        if (_coin.Value < amount) return false;

        _coin.Value -= amount;
        PlayerPrefs.SetInt(COIN_KEY, _coin.Value);
        PlayerPrefs.Save();
        return true;
    }
}
