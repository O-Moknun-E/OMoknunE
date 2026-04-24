using UnityEngine;

[CreateAssetMenu(fileName = "ForcePlaceStoneEffect", menuName = "OmokEffects/ForcePlaceStone")]
public class ForcePlaceStoneEffect : SkillEffect
{
    public override void OnExecute(SkillContext context)
    {
        // 1. 이미 돌이 있는지 체크 (데이터상)
        StoneType currentStone = OmokManager.Instance.GetBoardData(context.TargetX, context.TargetY);
        if (currentStone != StoneType.Empty)
        {
            Debug.LogWarning("이미 돌이 있는 자리에는 강제 착수가 불가능합니다.");
            return;
        }

        // 2. 시전자(Caster)의 색상 결정
        StoneType stoneType = (context.Caster == PlayerType.Black) ? StoneType.Black : StoneType.White;

        // 3. 데이터 강제 업데이트
        OmokManager.Instance.SetBoardData(context.TargetX, context.TargetY, stoneType);

        // 4. 시각적 강제 업데이트 (BoardInteraction 활용)
        // NetworkOmokManager에 등록된 스킨 정보를 가져와서 뿌려줍니다.
        BoardInteraction bi = FindObjectOfType<BoardInteraction>();
        if (bi != null)
        {
            // 실제 프로젝트에서는 시전자의 스킨 ID를 Context에 포함하거나 
            // 기본 스킨을 사용하도록 처리합니다.
            // 여기서는 예시로 기본 PlaceStoneRemote 호출
            // (팀원들이 만든 Sprite 관리 로직에 따라 Sprite를 할당하세요)
            Debug.Log($"[Effect] {context.TargetX}, {context.TargetY}에 {stoneType} 돌을 강제 생성!");
        }
    }
}