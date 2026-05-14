using UnityEngine;

[CreateAssetMenu(fileName = "SealEffect", menuName = "OmokEffects/Seal")]
public class SealEffect : SkillEffect
{
    [Header("결계 프리팹")]
    public GameObject sealPrefab;

    [Header("유지 턴 수")]
    public int durationTurns = 3;

    public override void OnExecute(SkillContext context, Vector3 spawnPos)
    {
        if (!context.IsReplay)
        {
            if (OmokManager.Instance.GetBoardData(context.TargetX, context.TargetY) != StoneType.Empty)
            {
                Debug.LogWarning("이미 무언가가 있는 자리입니다");
                return;
            }
        }

        Transform parent = context.IsReplay ? ReplayManager.ReplayEffectsContainer : null;

        GameObject sealObj = Instantiate(sealPrefab, spawnPos, Quaternion.identity, parent);

        if (!context.IsReplay)
        {
            FakeStoneCleanup cleanup = sealObj.AddComponent<FakeStoneCleanup>();

            // 결계 스킬은 소멸 시 연출 프리팹이 필요하지 않으므로 빈 값을 전달합니다
            cleanup.LockSpot(context.TargetX, context.TargetY, null);
        }

        StoneType casterStoneType = (context.Caster == PlayerType.Black) ? StoneType.Black : StoneType.White;
        TurnDuration timer = sealObj.AddComponent<TurnDuration>();
        timer.Setup(casterStoneType, durationTurns, context.IsReplay);

        Debug.Log($"[Skill] 결계가 ({context.TargetX}, {context.TargetY}) 좌표에 설치되었습니다 ({durationTurns}턴 유지)");
    }
}