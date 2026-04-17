using UnityEngine;

//타겟 대상의 타입 Enum
//                              타겟X,돌 있는 곳,빈 곳,십자가,대각선,수평,수직,정사각형
public enum SpellTargetType { None, SingleStone, Empty, Cross, diagonal,Horizontal, Vertical, square}

[CreateAssetMenu(fileName = "NewSpellData", menuName = "Gomoku/Spell Data")]
public class SpellData : ScriptableObject
{
    [Header("기본 정보")]
    public string spellID;        // 마법 고유 ID
    public string spellName;      // 마법 이름
    public string description;    // 마법 설명
    public Sprite icon;           // UI에 표시될 아이콘

    [Header("전투 수치")]
    public int manaCost;          // 마나 소모량
    public int cooldownTurn;      // 쿨타임 (턴 단위)
    public SpellTargetType targetType; // 타겟팅 방식

    [Header("시각 효과")]
    public GameObject castEffect; // 마법 발동 시 공통으로 나올 이펙트
}
