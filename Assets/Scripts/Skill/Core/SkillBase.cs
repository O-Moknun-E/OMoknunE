using UnityEngine;
using System.Collections.Generic;
using System;

// 1. 실행 데이터 컨텍스트 (x, y 모두 유지)
[Serializable]
public class SkillContext
{
    public int TargetX;
    public int TargetY;
    public PlayerType Caster;
    public int SkinID;
    public bool IsReplay;

    public bool IsMine()
    {
        if (IsReplay) return true;
        var netManager = UnityEngine.Object.FindFirstObjectByType<NetworkOmokManager>();
        if (netManager == null) return false;
        StoneType casterStoneType = (Caster == PlayerType.Black) ? StoneType.Black : StoneType.White;
        return casterStoneType == netManager.MyPlayerType;
    }
    public StoneType GetCasterStoneType() => (Caster == PlayerType.Black) ? StoneType.Black : StoneType.White;
}

// 2. [복구 완료] 효과 데이터들의 부모 클래스
public abstract class SkillEffect : ScriptableObject
{
    public abstract void OnExecute(SkillContext context, Vector3 spawnPos);
}

// 3. 스킬 본체 클래스
public abstract class SkillBase : ScriptableObject, IMagic
{
    [Header("상점 UI 표시 정보")]
    [SerializeField] private Sprite _icon;
    [SerializeField] private string _displayName;
    [SerializeField, TextArea(3, 10)] private string _description;

    public Sprite Icon => _icon;
    public string DisplayName => _displayName;
    public string Description => _description;

    [Header("기본 설정")]
    [SerializeField] protected string _skillName;
    [SerializeField] protected int _id;
    [SerializeField] protected int _cost;

    public string Name => _skillName;
    public int ID => _id;
    public int Cost => _cost;

    [Header("효과 데이터 리스트")]
    [SerializeField] protected List<SkillEffect> _effects;

    protected SkillContext _currentContext;
    public SkillContext CurrentContext => _currentContext;

    // x, y 좌표를 모두 저장합니다.
    public void SetTarget(int x, int y, PlayerType caster, int skinID)
    {
        _currentContext = new SkillContext
        {
            TargetX = x,
            TargetY = y,
            Caster = caster,
            SkinID = skinID
        };
    }

    public void SetContext(int x, int y, PlayerType caster, int skinID) => SetTarget(x, y, caster, skinID);

    // ReplayManager에서 객체 통째로 넘길 때 사용
    public void SetContext(SkillContext context) => _currentContext = context;

    public virtual void Execute(bool isReplay = false)
    {
        if (_effects == null || _effects.Count == 0) return;
        if (_currentContext == null) _currentContext = new SkillContext();
        _currentContext.IsReplay = isReplay;

        foreach (var effect in _effects)
        {
            Vector3 spawnPos = Vector3.zero;

            if (isReplay)
            {
                var rm = UnityEngine.Object.FindFirstObjectByType<ReplayManager>();
                // [해결] x와 y 인자를 두 개 다 전달합니다.
                if (rm != null) spawnPos = rm.GetWorldPositionFromIndex(_currentContext.TargetX, _currentContext.TargetY);
            }
            else
            {
                var bi = UnityEngine.Object.FindFirstObjectByType<BoardInteraction>();
                // BoardInteraction도 보통 x, y를 받으므로 맞춰줍니다.
                if (bi != null) spawnPos = bi.GetWorldPositionFromIndex(_currentContext.TargetX, _currentContext.TargetY);
            }
            effect.OnExecute(_currentContext, spawnPos);
        }
    }
}