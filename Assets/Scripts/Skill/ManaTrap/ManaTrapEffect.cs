using UnityEngine;

[CreateAssetMenu(fileName = "ManaTrapEffect", menuName = "OmokEffects/ManaTrap")]
public class ManaTrapEffect : SkillEffect
{
    [Header("마나 덫 프리팹")]
    public GameObject trapPrefab;

    [Header("상대의 마나를 얼마나 깎을 것인가?")]
    public int manaPenalty = 3;

    public override void OnExecute(SkillContext context, Vector3 spawnPos)
    {
        if (!context.IsReplay)
        {
            // 덫은 빈자리에만 설치할 수 있음
            if (OmokManager.Instance.GetBoardData(context.TargetX, context.TargetY) != StoneType.Empty)
            {
                Debug.LogWarning("빈자리에만 마나 덫을 설치할 수 있습니다");
                return;
            }
        }

        // 덫 프리팹 생성 및 로직 부착
        if (trapPrefab != null)
        {
            // 리플레이 모드인지 아닌지에 따라 덫이 담길 폴더(부모)를 다르게 설정
            Transform parentTransform = null;

            if (context.IsReplay)
            {
                parentTransform = ReplayManager.ReplayEffectsContainer;
            }
            else
            {
                // 실제 게임 모드에서는 평소대로 Board에 넣음
                parentTransform = GameObject.Find("Board")?.transform;
            }

            // 결정된 부모 폴더 아래에 덫 생성
            GameObject trapObj = Instantiate(trapPrefab, spawnPos, Quaternion.identity, parentTransform);

            ManaTrapObject trapLogic = trapObj.AddComponent<ManaTrapObject>();
            trapLogic.Setup(context.TargetX, context.TargetY, context.Caster, manaPenalty, context.IsReplay);
        }
    }
}