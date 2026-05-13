using UnityEngine;

[CreateAssetMenu(fileName = "SilenceEffect", menuName = "OmokEffects/Silence")]
public class SilenceEffect : SkillEffect
{
    [Header("침묵 지속 턴 수")]
    public int durationTurns = 3;

    [Header("침묵 자물쇠 프리팹 (lock / Silence_Open 애니메이션 포함)")]
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

                // 1. 턴을 몰래 계산할 보이지 않는 빈 오브젝트(추적기) 생성
                GameObject trackerObj = new GameObject("SilenceTracker");
                if (parent != null) trackerObj.transform.SetParent(parent);
                trackerObj.transform.position = centerPos;

                // 2. 시전 즉시 잠기는 연출을 보여줄 자물쇠를 화면에 생성
                // (생성되자마자 애니메이터의 기본 상태인 lock이 자동으로 재생됩니다)
                GameObject lockAnimObj = Instantiate(silencePrefab, centerPos, Quaternion.identity, parent);
                
                float lockDelay = 1.0f;
                Animator animator = lockAnimObj.GetComponent<Animator>();
                if (animator != null && animator.runtimeAnimatorController != null)
                {
                    foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
                    {
                        // 잠김 애니메이션의 길이를 파악합니다
                        if (clip.name.ToLower().Contains("lock"))
                        {
                            lockDelay = clip.length;
                            break;
                        }
                    }
                }
                // 잠기는 애니메이션을 모두 보여준 직후 시각적인 자물쇠만 먼저 파괴(숨김)합니다
                Destroy(lockAnimObj, lockDelay);

                // 3. 투명한 추적기(Tracker)에 턴 타이머와 종료 연출 스크립트를 붙여둡니다
                SilenceCleanup cleanup = trackerObj.AddComponent<SilenceCleanup>();
                cleanup.Setup(silencePrefab);

                StoneType casterStoneType = (context.Caster == PlayerType.Black) ? StoneType.Black : StoneType.White;
                TurnDuration timer = trackerObj.AddComponent<TurnDuration>();
                
                // 지정된 턴이 끝나면 Tracker가 파괴되며 SilenceCleanup이 작동해 풀림 연출이 소환됩니다
                timer.Setup(casterStoneType, durationTurns, context.IsReplay);
            }
        }
    }
}