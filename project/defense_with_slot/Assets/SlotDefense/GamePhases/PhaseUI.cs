using UnityEngine;
using DG.Tweening;

public enum UIShowAnimType
{
    None,
    Fade,       // CanvasGroup alpha
    Scale,      // scale 0 → original
    FadeAndScale
}

[RequireComponent(typeof(CanvasGroup))]
public class PhaseUI : MonoBehaviour
{
    [SerializeField] private UIShowAnimType _animType = UIShowAnimType.Fade;
    [SerializeField] private float _duration = 0.3f;

    private CanvasGroup _canvasGroup;
    private Vector3 _originalScale;
    private Tween _tween;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _originalScale = transform.localScale;

        // 초기 상태를 알파 0 / 스케일 0으로 숨김 처리
        _canvasGroup.alpha = 0f;
        transform.localScale = (_animType == UIShowAnimType.Scale || _animType == UIShowAnimType.FadeAndScale)
            ? Vector3.zero
            : _originalScale;
    }

    public void Show() => PlayAnimation(true);
    public void Hide() => PlayAnimation(false);

    private void PlayAnimation(bool show)
    {
        _tween?.Kill();

        switch (_animType)
        {
            case UIShowAnimType.None:
                _canvasGroup.alpha = show ? 1f : 0f;
                transform.localScale = show ? _originalScale : Vector3.zero;
                break;

            case UIShowAnimType.Fade:
                _canvasGroup.DOFade(show ? 1f : 0f, _duration);
                break;

            case UIShowAnimType.Scale:
                transform.DOScale(show ? _originalScale : Vector3.zero, _duration)
                    .SetEase(show ? Ease.OutBack : Ease.InBack);
                break;

            case UIShowAnimType.FadeAndScale:
                _canvasGroup.DOFade(show ? 1f : 0f, _duration);
                transform.DOScale(show ? _originalScale : Vector3.zero, _duration)
                    .SetEase(show ? Ease.OutBack : Ease.InBack);
                break;
        }
    }
}
