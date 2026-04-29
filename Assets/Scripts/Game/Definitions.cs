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