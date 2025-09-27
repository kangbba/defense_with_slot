using UnityEngine;
using TMPro;
using UniRx;

public class CoinDisplayer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _coinText;

    private CompositeDisposable _disposable = new();

    private void Start()
    {
        DataManager.Instance.Coin
            .Subscribe(coin =>
            {
                _coinText.text = $"코인 : {coin}";
            })
            .AddTo(_disposable);
    }

    private void OnDisable()
    {
        _disposable.Clear();
    }
}
