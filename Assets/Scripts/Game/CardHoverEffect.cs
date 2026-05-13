using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 카드 호버 효과를 담당하는 클래스
/// </summary>
public class CardHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float _hoverHeight = 200f;   // 카드에 마우스 호버시 올라갈 높이
    [SerializeField] private float _hoverDuration = 0.2f; // 호버 애니메이션 지속 시간(올라거나 내려가는데 걸리는 시간)

    private Vector3 _originalPosition;      // 카드의 원래 위치 저장
    private RectTransform _rectTransform;   // 카드의 RectTransform 컴포넌트

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

        // 카드의 원래 위치 저장
        _originalPosition = _rectTransform.anchoredPosition;

    }

    /// <summary>
    /// 호버 시작 시 호출되는 메서드
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 현재 진행 중인 트윈 애니메이션이 있다면 종료
        _rectTransform.DOKill();

        // 카드가 올라가는 애니메이션 시작
        _rectTransform.DOAnchorPosY(_originalPosition.y + _hoverHeight, _hoverDuration).SetEase(Ease.OutCubic);
    }

    /// <summary>
    /// 호버 종료 시 호출되는 메서드
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        // 현재 진행 중인 트윈 애니메이션이 있다면 종료
        _rectTransform.DOKill();

        // 카드가 원래 위치로 내려가는 애니메이션 시작
        _rectTransform.DOAnchorPosY(_originalPosition.y, _hoverDuration).SetEase(Ease.OutCubic);
    }
}
