using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "FakeStoneEffect", menuName = "OmokEffects/FakeStone")]
public class FakeStoneEffect : SkillEffect
{
    [Header("가짜 돌 기본 프리팹")]
    public GameObject fakeStonePrefab;

    [Header("블라인드 효과 프리팹")]
    public GameObject blindPrefab;

    public override void OnExecute(SkillContext context, Vector3 spawnPos)
    {
        if (fakeStonePrefab == null || blindPrefab == null) return;

        // 1. 이미 돌이 있으면 실패
        if (OmokManager.Instance.GetBoardData(context.TargetX, context.TargetY) != StoneType.Empty)
        {
            Debug.LogWarning("이미 돌이 있는 자리입니다");
            return;
        }

        // 리플레이 모드일 때 컨테이너를 부모로 지정
        Transform parent = context.IsReplay ? ReplayManager.ReplayEffectsContainer : null;

        // 2. 가짜 돌 생성
        GameObject fakeStone = Instantiate(fakeStonePrefab, spawnPos, Quaternion.identity, parent);

        // 3. 피아 식별
        bool isMine = context.IsMine();

        // 4. 스킨 적용 로직
        SpriteRenderer sr = fakeStone.GetComponent<SpriteRenderer>();
        Animator anim = fakeStone.GetComponent<Animator>();
        if (sr != null)
        {
            if (isMine)
            {
                if (anim != null) anim.enabled = true;
                sr.color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                if (anim != null) anim.enabled = false;
                sr.sprite = StoneSkinRegistry.Instance.GetStoneSkin(context.SkinID);
                sr.color = new Color(1f, 1f, 1f, 1f);

                // 1.5초 화면 가리기
                GameObject blind = Instantiate(blindPrefab, new Vector3(0f, 0f, -0.2f), Quaternion.identity);
                Destroy(blind, 1.5f);
            }
        }

        // 5. 3턴 뒤 사라지도록 타이머 세팅
        TurnDuration timer = fakeStone.AddComponent<TurnDuration>();
        timer.Setup(context.GetCasterStoneType(), 3, context.IsReplay);

        // 6. 자물쇠 잠금 컴포넌트 추가 및 좌표 전달
        FakeStoneCleanup cleanup = fakeStone.AddComponent<FakeStoneCleanup>();
        cleanup.LockSpot(context.TargetX, context.TargetY);
    }
}