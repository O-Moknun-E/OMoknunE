using UnityEngine;

/// <summary>
/// 오목 게임의 규칙을 정의하는 인터페이스
/// </summary>
public interface IOmokRule
{
    // 오목판에 돌을 놓을 수 있는지 여부를 판단하는 메서드
    bool CanPlaceStone(StoneType[,] board, int row, int col, StoneType type);

    // 오목판에서 승리 조건을 만족하는지 여부를 판단하는 메서드
    bool CheckWin(StoneType[,] board, int row, int col, StoneType type);
}
