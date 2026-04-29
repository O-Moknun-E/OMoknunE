using UnityEngine;

[CreateAssetMenu(fileName = "FogEffect", menuName = "OmokEffects/Fog")]
public class FogEffect : SkillEffect
{
    [Header("안개 프리팹 (3x3 크기)")]
    public GameObject fogPrefab;

    public override void OnExecute(SkillContext context)
    {
        BoardInteraction bi = FindFirstObjectByType<BoardInteraction>();
        NetworkOmokManager netManager = FindFirstObjectByType<NetworkOmokManager>();

        if (bi == null || fogPrefab == null) return;

        // 1. 오목판(만능 콘센트)에서 실제 월드 좌표 획득
        Vector3 spawnPos = bi.GetWorldPositionFromIndex(context.TargetX, context.TargetY);
        spawnPos.z = -0.2f; // 돌보다 앞쪽에 보이도록 설정

        // 2. 안개 오브젝트 생성
        GameObject fog = Instantiate(fogPrefab, spawnPos, Quaternion.identity);

        // 3. 시전자 확인 및 피아 식별
        StoneType casterStoneType = (context.Caster == PlayerType.Black) ? StoneType.Black : StoneType.White;
        bool isMine = (casterStoneType == netManager.MyPlayerType);

        // 4. 시각적 처리 (내 안개는 반투명, 적 안개는 불투명)
        SpriteRenderer sr = fog.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = isMine ? 0.4f : 1.0f;
            sr.color = c;
        }

        // 5. 공용 턴제 타이머 부착
        TurnDuration timer = fog.AddComponent<TurnDuration>();

        // 시전자 정보와 지속 시간을 인자로 넘김
        timer.Setup(casterStoneType, 3);

        Debug.Log($"[Skill] ({context.TargetX}, {context.TargetY}) 좌표에 3턴 지속 안개가 생성되었습니다.");
    }
}