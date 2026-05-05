using UnityEngine;
using System.Collections.Generic;
using System;


[Serializable]
public class SkillContext
{
    public int TargetX;
    public int TargetY;
    public PlayerType Caster;
    public int SkinID;
    public bool IsReplay;   // 리플레이 모드 여부

    /// <summary>
    /// 시전자가 자신인지 확인(리플레이 모드에서는 항상 true)
    /// </summary>
    /// <returns>자신이면 true, 아니면 false(리플레이 모드는 항상 true)</returns>
    public bool IsMine()
    {
        // 리플레이 모드에서는 시전자가 자신이라고 가정하여 효과에 영향 안받게(안개 등)
        if (IsReplay) return true;

        NetworkOmokManager netManager = UnityEngine.Object.FindFirstObjectByType<NetworkOmokManager>();
        if (netManager == null) return false;

        StoneType casterStoneType = (Caster == PlayerType.Black) ? StoneType.Black : StoneType.White;
        return casterStoneType == netManager.MyPlayerType;

    }

    /// <summary>
    /// 시전자의 StoneType 반환
    /// </summary>
    /// <returns>시전자의 StoneType</returns>
    public StoneType GetCasterStoneType() => (Caster == PlayerType.Black) ? StoneType.Black : StoneType.White;
}

//SkillEffect 상속받아서 스킬 효과 구현하기
public abstract class SkillEffect : ScriptableObject
{
    public abstract void OnExecute(SkillContext context, Vector3 spawnPos);
}


#region 2. 확장된 스킬 베이스 (껍데기 및 조립기)
[CreateAssetMenu(fileName = "NewSkill", menuName = "OmokSkills/CompositeSkill")]
public class SkillBase : ScriptableObject, IMagic
{
    [Header("스킬 기본 정보")]
    [SerializeField] private int _id;
    [SerializeField] private string _skillName;
    [SerializeField][TextArea] private string _description;
    [SerializeField] private int _cost;
    [SerializeField] private CellChangeType _changeType;    // 오목판 변화 종류

    [Header("효과 조립 (순서대로 실행됨)")]
    [SerializeField] private List<SkillEffect> _effects = new List<SkillEffect>();

    // IMagic 인터페이스 구현
    public int ID => _id;
    public string Name => _skillName;
    public string Description => _description;
    public int Cost => _cost;
    public CellChangeType ChangeType => _changeType;
    public SkillContext CurrentContext => _currentContext;

    // 내부 상태 저장용
    private SkillContext _currentContext;

    public void SetTarget(int x, int y, PlayerType caster, int skinId)
    {
        _currentContext = new SkillContext { TargetX = x, TargetY = y, Caster = caster, SkinID = skinId };
    }

    /// <summary>
    /// SkillContext 설정
    /// </summary>
    public void SetContext(SkillContext skillContext) => _currentContext = skillContext;

    /// <summary>
    /// 마법을 실행하는 메서드
    /// </summary>
    /// <param name="isReplay">리플레이 모드 여부</param>
    public void Execute(bool isReplay)
    {
        if (_effects == null || _effects.Count == 0)
        {
            Debug.LogWarning($"{_skillName} 스킬에 등록된 효과가 없습니다");
            return;
        }

        // 리플레이 모드 여부 설정
        _currentContext.IsReplay = isReplay;

        // 리스트에 담긴 효과들을 순차적으로 실행
        foreach (var effect in _effects)
        {
            Vector3 spawnPos = Vector3.zero;

            if(isReplay)
            {
                // 리플레이 모드에서는 Board Transform으로
                ReplayManager rm = FindFirstObjectByType<ReplayManager>();

                spawnPos = rm.GetWorldPositionFromIndex(_currentContext.TargetX, _currentContext.TargetY);
            } else
            {
                // 아니면 BoardInteraction으로
                BoardInteraction bi = FindFirstObjectByType<BoardInteraction>();

                spawnPos = bi.GetWorldPositionFromIndex(_currentContext.TargetX, _currentContext.TargetY);
            }

            effect.OnExecute(_currentContext, spawnPos);
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
