using UnityEngine;

[CreateAssetMenu(fileName = "SealEffect", menuName = "OmokEffects/Seal")]
public class SealEffect : SkillEffect
{
    [Header("결계(운석 등) 프리팹")]
    public GameObject sealPrefab;

    [Header("유지 턴 수")]
    public int durationTurns = 3;

    public override void OnExecute(SkillContext context, Vector3 spawnPos)
    {
        if (!context.IsReplay)
        {
            // 이미 무언가 있는 자리면 스킬 사용 실패(리플레이에서는 X)
            if (OmokManager.Instance.GetBoardData(context.TargetX, context.TargetY) != StoneType.Empty)
            {
                Debug.LogWarning("이미 무언가가 있는 자리입니다");
                return;
            }
        }

        // 리플레이 모드일 때 컨테이너를 부모로 지정
        Transform parent = context.IsReplay ? ReplayManager.ReplayEffectsContainer : null;

        // 결계 프리팹 소환
        GameObject sealObj = Instantiate(sealPrefab, spawnPos, Quaternion.identity, parent);

        // 아까 만든 Cleanup 컴포넌트 부착 및 좌표 잠금(리플레이에서는 X)
        if (!context.IsReplay)
        {
            FakeStoneCleanup cleanup = sealObj.AddComponent<FakeStoneCleanup>();
            cleanup.LockSpot(context.TargetX, context.TargetY);
        }

        // 타이머 부착
        StoneType casterStoneType = (context.Caster == PlayerType.Black) ? StoneType.Black : StoneType.White;
        TurnDuration timer = sealObj.AddComponent<TurnDuration>();
        timer.Setup(casterStoneType, durationTurns, context.IsReplay);

        Debug.Log($"[Skill] 결계가 ({context.TargetX}, {context.TargetY}) 좌표에 설치되었습니다! ({durationTurns}턴 유지)");
    }
}