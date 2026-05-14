using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SkillCardSilenceUI : MonoBehaviour
{
    [Header("연결: 호버 스크립트와 카드 버튼")]
    public CardHoverEffect hoverEffect;
    public Button cardButton;

    [Header("연결: 사슬 시각 요소 및 애니메이터")]
    public Image chainVisualImage;
    public Animator chainAnimator;

    [Header("연출 타이밍 설정")]
    [Tooltip("사슬 감기는/풀리는 속도")]
    public float chainAnimSpeed = 0.8f;
    public float cardMoveWait = 0.25f;
    [Tooltip("카드 사슬과 오목판 자물쇠가 화면에 머무르는 총 시간")]
    public float syncDisplayTime = 2.0f;
    public float openAnimWait = 0.6f;

    private void Awake()
    {
        if (chainVisualImage != null)
        {
            chainVisualImage.enabled = false;
        }
    }

    [ContextMenu("테스트: 침묵 걸기")]
    public void ApplySilence()
    {
        if (chainVisualImage != null && chainVisualImage.enabled) return;
        StartCoroutine(SilenceRoutine());
    }

    [ContextMenu("테스트: 침묵 해제")]
    public void ReleaseSilence()
    {
        if (chainVisualImage != null && !chainVisualImage.enabled) return;
        StartCoroutine(ReleaseRoutine());
    }

    private IEnumerator SilenceRoutine()
    {
        // 유저 간섭 차단
        if (hoverEffect != null) hoverEffect.enabled = false;
        if (cardButton != null) cardButton.interactable = false;

        // 카드 위로 올리기
        if (hoverEffect != null) hoverEffect.OnPointerEnter(null);
        yield return new WaitForSeconds(cardMoveWait);

        // 사슬 이미지 켜고 애니메이션 천천히 재생
        if (chainVisualImage != null) chainVisualImage.enabled = true;
        if (chainAnimator != null)
        {
            chainAnimator.speed = chainAnimSpeed; // 설정한 속도로 느리게 재생
            chainAnimator.Play("Chain_Lock");
        }

        // 설정된 총 시간만큼 대기
        // 애니메이션이 일찍 끝나면 마지막 묶인 상태로 멈춰서 기다림
        yield return new WaitForSeconds(syncDisplayTime);

        // 대기가 끝나면 카드 다시 아래로 내리기
        if (hoverEffect != null) hoverEffect.OnPointerExit(null);
    }

    private IEnumerator ReleaseRoutine()
    {
        // 카드 다시 위로 올리기
        if (hoverEffect != null) hoverEffect.OnPointerEnter(null);
        yield return new WaitForSeconds(cardMoveWait);

        // 사슬 풀림 애니메이션 역재생
        if (chainAnimator != null)
        {
            chainAnimator.speed = chainAnimSpeed;
            chainAnimator.SetTrigger("UnlockTrigger");
        }

        // 풀리는 연출은 속도가 느려진 만큼 대기 시간도 비례해서 늘림
        yield return new WaitForSeconds(openAnimWait / (chainAnimSpeed > 0 ? chainAnimSpeed : 1f));

        // 애니메이션이 끝나면 사슬 이미지 끄고 카드 내리기
        if (chainVisualImage != null) chainVisualImage.enabled = false;
        if (hoverEffect != null) hoverEffect.OnPointerExit(null);

        if (hoverEffect != null) hoverEffect.enabled = true;
        if (cardButton != null) cardButton.interactable = true;
    }
}