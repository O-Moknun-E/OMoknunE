using UnityEngine;

[CreateAssetMenu(fileName = "TimeOverloadEffect", menuName = "OmokEffects/TimeOverload")]
public class TimeOverloadEffect : SkillEffect
{
    [Header("과부하 지속 턴 수")]
    public int durationTurns = 2;

    [Header("과부하 발동 이펙트 (시계 애니메이션 프리팹)")]
    public GameObject overloadVfxPrefab;

    [Header("이펙트 유지 시간 (애니메이션 길이)")]
    public float vfxDuration = 2.0f;

    public override void OnExecute(SkillContext context, Vector3 spawnPos)
    {
        // ========================================================
        // 게임이든 리플레이든 시계 애니메이션은 무조건 재생
        // ========================================================
        if (overloadVfxPrefab != null)
        {
            Vector3 vfxPos = new Vector3(0, 0, -5f);

            // 리플레이 모드일 때는 리플레이 전용 폴더(컨테이너)
            Transform parent = context.IsReplay ? ReplayManager.ReplayEffectsContainer : null;

            GameObject vfx = Instantiate(overloadVfxPrefab, vfxPos, Quaternion.identity, parent);
            Destroy(vfx, vfxDuration); // 애니메이션 다 끝나면 삭제
        }

        // ========================================================
        // 리플레이가 아닌 게임일 때만 상대방 시간을 깎는다
        // ========================================================
        if (!context.IsReplay)
        {
            PlayerType targetType = (context.Caster == PlayerType.Black) ? PlayerType.White : PlayerType.Black;

            if (OmokManager.Instance != null)
            {
                OmokManager.Instance.ApplyTimeOverload(targetType, durationTurns);
            }
        }
        else
        {
            // 리플레이일 때는 시간은 안 깎고 안내 로그만 띄움
            Debug.Log($"<color=magenta>[Replay] {context.Caster} 진영이 시간 과부하 스킬을 사용했습니다!</color>");
        }
    }
}