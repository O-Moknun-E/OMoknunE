using UnityEngine;

[CreateAssetMenu(fileName = "SilenceEffect", menuName = "OmokEffects/Silence")]
public class SilenceEffect : SkillEffect
{
    [Header("침묵 지속 턴 수")]
    public int durationTurns = 3;

    public override void OnExecute(SkillContext context, Vector3 spawnPos)
    {
        if (!context.IsReplay)
        {
            NetworkOmokManager netManager = FindFirstObjectByType<NetworkOmokManager>();
            if (netManager != null)
            {
                PlayerType myType = (netManager.MyPlayerType == StoneType.Black) ? PlayerType.Black : PlayerType.White;

                // 시전자가 내가 아니면(상대방이면) 나에게 침묵 적용
                if (context.Caster != myType)
                {
                    netManager.ApplySilence(durationTurns);
                }
                else
                {
                    Debug.Log($"<color=green>[System] 상대방에게 {durationTurns}턴 동안 침묵을 걸었습니다!</color>");
                }
            }
        }

        // 나중에 침묵 UI 이펙트가 생기면 이곳에서 캔버스에 띄워주면 됩니다
    }
}