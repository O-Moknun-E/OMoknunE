using UnityEngine;
using System.Collections.Generic;


public struct SkillContext
{
    public int TargetX;
    public int TargetY;
    public PlayerType Caster; 
}

//SkillEffect 상속받아서 스킬 효과 구현하기
public abstract class SkillEffect : ScriptableObject
{
    public abstract void OnExecute(SkillContext context);
}


#region 2. 확장된 스킬 베이스 (껍데기 및 조립기)
[CreateAssetMenu(fileName = "NewSkill", menuName = "OmokSkills/CompositeSkill")]
public class SkillBase : ScriptableObject, IMagic
{
    [Header("스킬 기본 정보")]
    [SerializeField] private string _skillName;
    [SerializeField][TextArea] private string _description;
    [SerializeField] private int _cost;

    [Header("효과 조립 (순서대로 실행됨)")]
    [SerializeField] private List<SkillEffect> _effects = new List<SkillEffect>();

    // IMagic 인터페이스 구현
    public string Name => _skillName;
    public string Description => _description;
    public int Cost => _cost;

    // 내부 상태 저장용
    private SkillContext _currentContext;

    public void SetTarget(int x, int y,PlayerType caster)
    {
        _currentContext = new SkillContext { TargetX = x, TargetY = y };
    }

    // 인터페이스의 Execute를 구현
    public void Execute()
    {
        if (_effects == null || _effects.Count == 0)
        {
            Debug.LogWarning($"{_skillName} 스킬에 등록된 효과가 없습니다!");
            return;
        }

        // 리스트에 담긴 효과들을 순차적으로 실행
        foreach (var effect in _effects)
        {
            effect.OnExecute(_currentContext);
        }
    }
}
#endregion

//팀원분들께 부탁드릴 부분들..
// 예: 지정한 위치의 돌을 제거하는 효과
//[CreateAssetMenu(fileName = "RemoveStoneEffect", menuName = "OmokEffects/RemoveStone")]
//public class RemoveStoneEffect : SkillEffect
//{
//    public override void OnExecute(SkillContext context)
//    {
//        // OmokManager를 통해 실제 데이터 삭제
//       // OmokManager.Instance.ClearBoardData(context.TargetX, context.TargetY);
//        Debug.Log($"[효과] ({context.TargetX}, {context.TargetY})의 돌을 제거했습니다.");
//    }
//}
