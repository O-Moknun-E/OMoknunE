using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SkillCardSilenceUI : MonoBehaviour
{
    [Header("연결: 호버 스크립트와 카드 버튼")]
    public CardHoverEffect hoverEffect;     // 같은 Use_Skill 오브젝트에 있는 스크립트 할당
    public Button cardButton;               // 같은 Use_Skill 오브젝트에 있는 Button 컴포넌트 할당

    [Header("연결: 사슬 시각 요소 및 애니메이터")]
    // 중요: 여기에 하위 자식인 "Chain_Visuals" 오브젝트의 Image 컴포넌트를 연결하세요.
    public Image chainVisualImage;
    public Animator chainAnimator;          // Chain_Visuals에 있는 Animator 연결

    [Header("연출 지연 시간 (초)")]
    public float cardMoveWait = 0.25f;      // 카드가 위로 완전히 올라가기까지 대기
    public float lockAnimWait = 0.6f;       // 우리가 만든 Chain_Lock 애니메이션 길이만큼 대기
    public float openAnimWait = 0.6f;       // 우리가 만든 Chain_Unlock(역재생) 길이만큼 대기

    private void Awake()
    {
        // [질문 해결] 게임 시작 시 하얀색 네모칸(Image)을 코드로 확실하게 끕니다.
        if (chainVisualImage != null)
        {
            chainVisualImage.enabled = false;
        }
    }

    /// <summary>
    /// 침묵 스킬 발동 시 매니저 스크립트에서 호출
    /// </summary>
    [ContextMenu("테스트: 침묵 걸기")] // 인스펙터 창 스크립트 우클릭으로 테스트 가능
    public void ApplySilence()
    {
        // 이미 침묵 중이라면 중복 실행 방지
        if (chainVisualImage != null && chainVisualImage.enabled) return;
        StartCoroutine(SilenceRoutine());
    }

    /// <summary>
    /// 3턴 뒤 침묵 해제 시 매니저 스크립트에서 호출
    /// </summary>
    [ContextMenu("테스트: 침묵 해제")]
    public void ReleaseSilence()
    {
        // 침묵 중이 아니라면 실행 방지
        if (chainVisualImage != null && !chainVisualImage.enabled) return;
        StartCoroutine(ReleaseRoutine());
    }

    private IEnumerator SilenceRoutine()
    {
        // 1. 유저 간섭 차단
        if (hoverEffect != null) hoverEffect.enabled = false;
        if (cardButton != null) cardButton.interactable = false;

        // 2. 카드 위로 올리기
        if (hoverEffect != null) hoverEffect.OnPointerEnter(null);
        yield return new WaitForSeconds(cardMoveWait);

        // 3. [핵심] 사슬 이미지 컴포넌트를 켜고, 즉시 애니메이션 재생
        if (chainVisualImage != null) chainVisualImage.enabled = true;
        if (chainAnimator != null) chainAnimator.Play("Chain_Lock");

        // Chain_Lock 애니메이션이 끝날 때까지 대기 (그래야 Loop로 자연스럽게 넘어감)
        yield return new WaitForSeconds(lockAnimWait);

        // 4. 카드 다시 아래로 내리기 (사슬은 묶여서 찰랑거리는 상태 유지)
        if (hoverEffect != null) hoverEffect.OnPointerExit(null);
    }

    private IEnumerator ReleaseRoutine()
    {
        // 1. 카드 다시 위로 올리기
        if (hoverEffect != null) hoverEffect.OnPointerEnter(null);
        yield return new WaitForSeconds(cardMoveWait);

        // 2. 사슬 풀림 애니메이션 트리거 작동 (역재생)
        if (chainAnimator != null) chainAnimator.SetTrigger("UnlockTrigger");

        // 풀리는 애니메이션 길이만큼 대기
        yield return new WaitForSeconds(openAnimWait);

        // 3. [핵심] 애니메이션이 끝나면 사슬 이미지 컴포넌트를 다시 끕니다.
        if (chainVisualImage != null) chainVisualImage.enabled = false;

        // 4. 카드 최종적으로 아래로 내리기
        if (hoverEffect != null) hoverEffect.OnPointerExit(null);

        // 5. 유저 간섭 다시 활성화
        if (hoverEffect != null) hoverEffect.enabled = true;
        if (cardButton != null) cardButton.interactable = true;
    }
}