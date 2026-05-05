using UnityEngine;

/// <summary>
/// 보드판에 놓여지는 돌의 종류
/// </summary>
public enum StoneType
{
    Empty,
    Black,
    White,
    Fake // 가짜 돌 / 검사 로직에서 판정 x
}

/// <summary>
/// 플레이어 타입(흑 or 백)
/// </summary>
public enum PlayerType
{
    Black,
    White
}

/// <summary>
/// 턴 내에서 발생하는 행동의 종류
/// </summary>
public enum ActionType
{
    UseMagic,   // 마법 사용
    PlaceStone  // 돌 착수
}

/// <summary>
/// 오목판 한 칸의 변경 타입
/// </summary>
public enum CellChangeType
{
    PlaceStone,     // 돌 배치
    RemoveStone,    // 돌 제거
    VisualEffect    // 시각 효과만 (실제 돌 변화 없음)
}