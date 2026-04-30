using UnityEngine;

[CreateAssetMenu(fileName = "RemoveStone", menuName = "OmokEffects/RemoveStone")]
public class RemoveStoneEffect : SkillEffect
{
    public override void OnExecute(SkillContext context)
    {
        // 1. 데이터 삭제
        OmokManager.Instance.SetBoardData(context.TargetX, context.TargetY, StoneType.Empty);

        // 2. 시각적 삭제 (BoardInteraction에 직접 명령)
        BoardInteraction bi = FindFirstObjectByType<BoardInteraction>();
        if (bi != null)
        {
           // bi.RemoveStoneVisual(context.TargetX, context.TargetY);
        }
        Debug.Log("해당 좌표 돌 삭제 완료");
        
    }
}