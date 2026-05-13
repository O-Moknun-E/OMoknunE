using UnityEngine;

[CreateAssetMenu(fileName = "SilenceEffect", menuName = "OmokEffects/Silence")]
public class SilenceEffect : SkillEffect
{
    [Header("침묵 지속 턴 수")]
    public int durationTurns = 3;

    [Header("침묵 자물쇠 프리팹 (Lock/Open 애니메이션 포함)")]
    public GameObject silencePrefab;

    public override void OnExecute(SkillContext context, Vector3 spawnPos)
    {
        if (!context.IsReplay)
        {
            NetworkOmokManager netManager = FindFirstObjectByType<NetworkOmokManager>();
            if (netManager != null)
            {
                PlayerType myType = (netManager.MyPlayerType == StoneType.Black) ? PlayerType.Black : PlayerType.White;

                if (context.Caster != myType)
                {
                    netManager.ApplySilence(durationTurns);
                }
                else
                {
                    Debug.Log($"<color=green>[System] 상대방에게 {durationTurns}턴 동안 침묵을 걸었습니다</color>");
                }
            }
        }

        if (silencePrefab != null)
        {
            NetworkOmokManager netManager = FindFirstObjectByType<NetworkOmokManager>();
            PlayerType myType = (netManager != null && netManager.MyPlayerType == StoneType.Black) ? PlayerType.Black : PlayerType.White;

            if (context.Caster != myType || context.IsReplay)
            {
                Transform parent = context.IsReplay ? ReplayManager.ReplayEffectsContainer : null;

                Vector3 centerPos = new Vector3(0f, 0f, -2f);

                // 프리팹 생성 시 기본 상태인 Lock 애니메이션이 자동으로 재생됩니다
                GameObject lockObj = Instantiate(silencePrefab, centerPos, Quaternion.identity, parent);

                SilenceCleanup cleanup = lockObj.AddComponent<SilenceCleanup>();
                cleanup.Setup(silencePrefab);

                StoneType casterStoneType = (context.Caster == PlayerType.Black) ? StoneType.Black : StoneType.White;
                TurnDuration timer = lockObj.AddComponent<TurnDuration>();
                timer.Setup(casterStoneType, durationTurns, context.IsReplay);
            }
        }
    }
}