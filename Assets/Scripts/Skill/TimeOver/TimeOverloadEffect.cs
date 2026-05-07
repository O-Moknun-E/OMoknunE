using UnityEngine;

[CreateAssetMenu(fileName = "TimeOverloadEffect", menuName = "OmokEffects/TimeOverload")]
public class TimeOverloadEffect : SkillEffect
{
    [Header("과부하 지속 턴 수")]
    public int durationTurns = 2;

    public override void OnExecute(SkillContext context, Vector3 spawnPos)
    {
        if (!context.IsReplay)
        {
            // 타겟은 시전자의 '반대 진영'
            PlayerType targetType = (context.Caster == PlayerType.Black) ? PlayerType.White : PlayerType.Black;

            if (OmokManager.Instance != null)
            {
                // OmokManager에게 상대방 시간 절반으로 깎으라고 명령!
                OmokManager.Instance.ApplyTimeOverload(targetType, durationTurns);
            }
        }
        else
        {
            Debug.Log($"<color=magenta>[Replay] {context.Caster}가 시간 과부하 스킬을 발동했습니다</color>");
        }
    }
}