using System;
using UnityEngine;

/// <summary>
/// 게임 전역 이벤트 관리
/// </summary>
public static class GameEvents
{
    public static event Action<int, int, StoneType> OnStonePlaced;  // 돌이 놓였을 때 발생하는 이벤트. 게임과 리플레이 모두 사용 가능 (x, y, StoneType)
    public static event Action<int, int, StoneType> OnStoneRemoved; // 돌이 제거되었을 때 발생하는 이벤트. (x, y, StoneType)

    /// <summary>
    /// 돌 배치 이벤트 발생
    /// </summary>
    public static void TriggerStonePlaced(int x, int y, StoneType stoneType)
    {
        OnStonePlaced?.Invoke(x, y, stoneType);
    }

   public static void TriggerStoneRemoved(int x, int y, StoneType stoneType)
    {
        OnStoneRemoved?.Invoke(x, y, stoneType);
    }
}
