using UnityEngine;

[CreateAssetMenu(fileName = "SilenceEffect", menuName = "OmokEffects/Silence")]
public class SilenceEffect : SkillEffect
{
    [Header("침묵 지속 턴 수")]
    public int durationTurns = 3;

    [Header("침묵 자물쇠 연출 대기 시간")]
    public float syncDisplayTime = 2.0f;

    [Header("침묵 자물쇠 프리팹")]
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
                Vector3 centerPos = new Vector3(1.15f, 0.35f, -2f);

                GameObject trackerObj = new GameObject("SilenceTracker");
                if (parent != null) trackerObj.transform.SetParent(parent);
                trackerObj.transform.position = centerPos;

                GameObject lockAnimObj = Instantiate(silencePrefab, centerPos, Quaternion.identity, parent);

                float lockDelay = 1.0f;
                Animator animator = lockAnimObj.GetComponent<Animator>();
                if (animator != null && animator.runtimeAnimatorController != null)
                {
                    foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
                    {
                        if (clip.name.ToLower().Contains("lock"))
                        {
                            lockDelay = clip.length;
                            break;
                        }
                    }
                }
                // 애니메이션이 먼저 끝나면 마지막 프레임 상태로 멈춘 채 대기하게 됩니다.
                float finalWaitTime = Mathf.Max(lockDelay, syncDisplayTime);
                Destroy(lockAnimObj, finalWaitTime);

                SilenceCleanup cleanup = trackerObj.AddComponent<SilenceCleanup>();
                cleanup.Setup(silencePrefab);

                StoneType casterStoneType = (context.Caster == PlayerType.Black) ? StoneType.Black : StoneType.White;
                TurnDuration timer = trackerObj.AddComponent<TurnDuration>();

                timer.Setup(casterStoneType, durationTurns, context.IsReplay);
            }
        }
    }
}