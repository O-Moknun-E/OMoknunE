using UnityEngine;

[CreateAssetMenu(fileName = "FogEffect", menuName = "OmokEffects/Fog")]
public class FogEffect : SkillEffect
{
    [Header("안개 파티클 프리팹 (3x3 크기)")]
    public GameObject fogPrefab;

    public override void OnExecute(SkillContext context, Vector3 spawnPos)
    {
        if (fogPrefab == null) return;

        // 1. 돌보다 앞쪽에 보이도록 설정
        spawnPos.z = -0.2f;

        // 리플레이 모드일 때 컨테이너를 부모로 지정
        Transform parent = context.IsReplay ? ReplayManager.ReplayEffectsContainer : null;

        // 2. 안개 파티클 오브젝트 생성
        GameObject fog = Instantiate(fogPrefab, spawnPos, Quaternion.identity, parent);

        // 3. 시전자 확인 및 피아 식별
        bool isMine = context.IsMine();

        // 4. 시각적 처리 (파티클 시스템의 Start Color 알파값 조절!)
        ParticleSystemRenderer psr = fog.GetComponent<ParticleSystemRenderer>();
        if (psr == null) psr = fog.GetComponentInChildren<ParticleSystemRenderer>();

        if (psr != null)
        {
            Material mat = psr.material;

            if (mat.HasProperty("_BaseColor"))
            {
                Color c = mat.GetColor("_BaseColor");
                c.a = isMine ? 0.2f : 1.0f;
                mat.SetColor("_BaseColor", c);
            }

            else if (mat.HasProperty("_Color"))
            {
                Color c = mat.color;
                c.a = isMine ? 0.2f : 1.0f;
                mat.color = c;
            }
        }

        // 5. 공용 턴제 타이머 부착
        TurnDuration timer = fog.AddComponent<TurnDuration>();
        timer.Setup(context.GetCasterStoneType(), 3, context.IsReplay);

        Debug.Log($"[Skill] ({context.TargetX}, {context.TargetY}) 좌표에 3턴 지속 안개가 생성되었습니다.");
    }
}