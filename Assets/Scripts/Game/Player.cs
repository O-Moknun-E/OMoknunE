using System;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    [Tooltip("한 게임당 사용할 수 있는 최대 마법 갯수")]
    [SerializeField] private int MaxMagicLimit = 3;
    [Tooltip("최대 마나")]
    [SerializeField] private int MaxMana = 100;

    public string Name { get; private set; }            // 플레이어 이름
    public int CurrentMana { get; private set; }        // 현재 마나
    public int UsedMagicCount { get; private set; }     // 사용한 마법 갯수
    public bool UsedMagicThisTurn { get; private set; } // 이번 턴에 마법을 사용했는지 여부

    private List<IMagic> _magics; // 보유한 마법 리스트

    private Action<int, int, StoneType> _resetTurnHandler; // 구독 해제용 참조

    // 초기 마나를 지정하는 생성자
    public Player(string name, int initialMana)
    {
        Name = name;
        CurrentMana = initialMana;
        UsedMagicCount = 0;
        UsedMagicThisTurn = false;
        _magics = new();

        // 델리게이트 참조 저장
        _resetTurnHandler = (row, col, type) => ResetTurn();

        //==============NetworkOmokManager를 구독하도록 수정==============
        // 어떤 돌이 놓이든 서버에서 확정 신호가 오면 마법 사용 턴을 리셋함
        // AI모드가 아닐때만
        if(!AIMatchManager.IsAIMode)
            NetworkOmokManager.OnStonePlaced += _resetTurnHandler;
        // AI 모드
        else
            GameEvents.OnStonePlaced += _resetTurnHandler;
    }

    /// <summary>
    /// 이벤트 구독 해제 및 정리
    /// </summary>
    public void Cleanup()
    {
        if(_resetTurnHandler != null)
        {
            // 모드에 따른 이벤트 구독 해제
            if(!AIMatchManager.IsAIMode)
                NetworkOmokManager.OnStonePlaced -= _resetTurnHandler;
            else
                GameEvents.OnStonePlaced -= _resetTurnHandler;

            _resetTurnHandler = null;
        }
    }

    // 마법 사용 시도 메서드
    // 사용 가능할 시 true, 사용 불가능할 시 false 반환
    public bool TryUseMagic(int cost)
    {
        // 사용 조건 체크
        if (UsedMagicThisTurn) return false;
        if (UsedMagicCount >= MaxMagicLimit) return false;
        if (CurrentMana < cost) return false;

        // 전부 만족했다면
        AddMana(-cost);
        UsedMagicCount++;
        UsedMagicThisTurn = true;

        return true;
    }

    // 마나 증감 메서드
    public void AddMana(int amount)
    {
        if (CurrentMana + amount > MaxMana)
            CurrentMana = MaxMana;
        else
            CurrentMana += amount;
    }

    // 착수 후 이벤트로 호출되는 메서드, 마법 사용 상태를 초기화
    private void ResetTurn()
    {
        UsedMagicThisTurn = false;
    }
}
