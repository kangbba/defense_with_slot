using UnityEngine;
using UnityEngine.UI;
using UniRx;
using DG.Tweening; // 🔹 DOFillAmount 사용

public class HpBar : MonoBehaviour
{
    [SerializeField] private Image _fill;
    [SerializeField] private Vector3 _worldOffset = new Vector3(0, 1f, 0); // 머리 위 오프셋
    [SerializeField] private float _tweenDuration = 0.25f; // 🔹 HP 변화 애니메이션 시간

    private Enemy _target;
    private int _maxHp;

    public void Bind(Enemy enemy)
    {
        if (enemy == null)
        {
            Debug.LogError("[HpBar] Enemy is null, binding failed.");
            Destroy(gameObject);
            return;
        }

        _target = enemy;
        _maxHp = enemy.Hp.Value;

        // HP 변화 → fillAmount Tween
        enemy.Hp
            .Subscribe(cur =>
            {
                if (_fill != null)
                {
                    float targetFill = (float)cur / _maxHp;
                    _fill.DOKill(); // 🔹 이전 Tween 중단
                    _fill.DOFillAmount(targetFill, _tweenDuration)
                         .SetEase(Ease.OutCubic);
                }
            })
            .AddTo(this);

        // HP 0 이하 → HP바 파괴
        enemy.Hp
            .Where(cur => cur <= 0)
            .Subscribe(_ => Destroy(gameObject))
            .AddTo(this);
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        // WorldSpace Canvas라 바로 월드 좌표 적용
        transform.position = _target.transform.position + _worldOffset;
    }
}
