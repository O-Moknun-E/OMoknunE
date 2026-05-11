using UnityEngine;

// 이 스크립트는 어떤 오브젝트에 붙든, 정해진 턴이 지나면 스스로를 파괴.
public class TurnDuration : MonoBehaviour
{
    private StoneType _casterType; // 시전자
    private int _targetTurns;      // 목표 턴 수
    private int _currentTurns = 0; // 현재 지난 턴 수
    private bool _isReplayMode;    // 리플레이 모드 여부

    // 스킬이 생성될 때 이 함수로 "누가 썼고, 몇 턴 뒤에 사라질지"를 전달
    public void Setup(StoneType caster, int duration, bool isReplayMode = false)
    {
        _casterType = caster;
        _targetTurns = duration;
        _isReplayMode = isReplayMode;

        // 돌이 놓일 때 & 제거될 때마다 알려달라고 구독
        GameEvents.OnStonePlaced += CountOpponentTurn;
        GameEvents.OnStoneRemoved += UnCountOpponentTurn;
    }

    private void OnDestroy()
    {
        GameEvents.OnStonePlaced -= CountOpponentTurn;
        GameEvents.OnStoneRemoved -= UnCountOpponentTurn;
    }

    private void CountOpponentTurn(int x, int y, StoneType placedType)
    {
        // 상대방이 돌을 두었을 때만 카운트 증가
        if (placedType != _casterType)
        {
            _currentTurns++;

            // 목표 턴에 도달하면 나 자신을 비활성화 or 파괴
            if (_currentTurns >= _targetTurns)
            {
                if(_isReplayMode)
                {
                    // 리플레이 모드에서는 비활성화
                    gameObject.SetActive(false);
                } else
                {
                    // 게임 모드면 파괴
                    Destroy(gameObject);
                }
            }
        }
    }

    /// <summary>
    /// 턴 되돌리기(상대방의 돌이 제거될 때만)
    /// </summary>
    private void UnCountOpponentTurn(int x, int y, StoneType removedType)
    {
        // 상대방의 돌이 제거되었을 때 카운트 감소
        if(removedType != _casterType)
        {
            _currentTurns--;

            // 비활성화된 상태에서 카운트가 목표카운트보다 줄어들면 다시 활성화
            if (_currentTurns < _targetTurns && _isReplayMode)
            {
                gameObject.SetActive(true);
            }

            // 음수 방지
            if (_currentTurns < 0)
                _currentTurns = 0;
        }
    }
}