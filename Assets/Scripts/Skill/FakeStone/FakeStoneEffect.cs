using UnityEngine;

[CreateAssetMenu(fileName = "FakeStoneEffect", menuName = "OmokEffects/FakeStone")]
public class FakeStoneEffect : SkillEffect
{
    [Header("가짜 돌 기본 프리팹")]
    public GameObject fakeStonePrefab;

    public override void OnExecute(SkillContext context)
    {
        BoardInteraction bi = FindFirstObjectByType<BoardInteraction>();
        NetworkOmokManager netManager = FindFirstObjectByType<NetworkOmokManager>();

        if (bi == null || fakeStonePrefab == null) return;

        // 1. 이미 돌이 있으면 실패
        if (OmokManager.Instance.GetBoardData(context.TargetX, context.TargetY) != StoneType.Empty)
        {
            Debug.LogWarning("이미 돌이 있는 자리입니다");
            return;
        }

        // 2. 가짜 돌 생성
        Vector3 spawnPos = bi.GetWorldPositionFromIndex(context.TargetX, context.TargetY);
        GameObject fakeStone = Instantiate(fakeStonePrefab, spawnPos, Quaternion.identity);

        // 3. 피아 식별
        StoneType casterStoneType = (context.Caster == PlayerType.Black) ? StoneType.Black : StoneType.White;
        bool isMine = (casterStoneType == netManager.MyPlayerType);

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
                sr.sprite = netManager.GetStoneSkin(context.SkinID);
                sr.color = new Color(1f, 1f, 1f, 1f);

                // 1.5초 화면 가리기
                bi.ShowBlindEffect(1.5f);
            }
        }

        // 5. 3턴 뒤 사라지도록 타이머 세팅
        TurnDuration timer = fakeStone.AddComponent<TurnDuration>();
        timer.Setup(casterStoneType, 3);

        // 6. 자물쇠 잠금 컴포넌트 추가 및 좌표 전달
        FakeStoneCleanup cleanup = fakeStone.AddComponent<FakeStoneCleanup>();
        cleanup.LockSpot(context.TargetX, context.TargetY);
    }
}