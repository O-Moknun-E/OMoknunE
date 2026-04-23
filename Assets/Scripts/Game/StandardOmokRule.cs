using UnityEngine;

// 오목 게임의 표준 규칙을 구현하는 클래스
public class StandardOmokRule : IOmokRule
{
    // 승리 조건을 판단할 때 사용할 방향 벡터 배열
    private Vector2Int[] _directions =
    {
        new (1, 0), // 가로
        new (0, 1), // 세로
        new (1, 1), // 우상향 대각선
        new (1, -1) // 우하향 대각선
    };

    // 오목판에 돌을 놓을 수 있는지 여부를 판단하는 메서드
    // 비어 있으면 true, 이미 돌이 놓여 있으면 false를 반환
    public bool CanPlaceStone(StoneType[,] board, int row, int col, StoneType type)
    {
        // 해당 위치에 돌이 이미 놓여 있는지 확인
        return board[row, col] == StoneType.Empty;
    }

    // 오목판에서 특정 위치에 돌을 놓았을 때 승리 조건이 충족되는지 판단하는 메서드
    // 5목이면 true, 그렇지 않으면 false를 반환
    public bool CheckWin(StoneType[,] board, int row, int col, StoneType type)
    {
        foreach (var direction in _directions)
        {
            if (CountStones(board, row, col, direction, type) == 5)
                return true;
        }

        return false;
    }

    // 연속된 돌의 개수를 세는 메서드
    // 연속된 돌의 개수 반환
    private int CountStones(StoneType[,] board, int row, int col, Vector2Int dir, StoneType type)
    {
        int count = 1;

        // 양방향으로 세기
        count += CheckDirection(board, row, col, dir, type);
        count += CheckDirection(board, row, col, -dir, type);

        return count;
    }

    // 특정 방향으로 연속된 돌의 개수를 세는 메서드
    // 연속된 돌의 개수 반환
    private int CheckDirection(StoneType[,] board, int row, int col, Vector2Int dir, StoneType type)
    {
        int count = 0;
        int curRow = row + dir.y;
        int curCol = col + dir.x;
        int size = board.GetLength(0);

        // 보드판 범위 내에서 연속된 돌의 개수 카운트
        while (curRow >= 0 && curRow < size && curCol >= 0 && curCol < size && board[curRow, curCol] == type)
        {
            count++;
            curRow += dir.y;
            curCol += dir.x;
        }

        return count;
    }
}
