using UnityEngine;

// 이 스크립트는 어떤 오브젝트에 붙든, 정해진 턴이 지나면 스스로를 파괴.
public class TurnDuration : MonoBehaviour
{
    private StoneType _casterType; // 시전자
    private int _targetTurns;      // 목표 턴 수
    private int _currentTurns = 0; // 현재 지난 턴 수

    // 스킬이 생성될 때 이 함수로 "누가 썼고, 몇 턴 뒤에 사라질지"를 전달
    public void Setup(StoneType caster, int duration)
    {
        _casterType = caster;
        _targetTurns = duration;

        // 돌이 놓일 때마다 알려달라고 구독
        NetworkOmokManager.OnStonePlaced += CountOpponentTurn;
    }

    private void OnDestroy()
    {
        NetworkOmokManager.OnStonePlaced -= CountOpponentTurn;
    }

    private void CountOpponentTurn(int x, int y, StoneType placedType)
    {
        // 상대방이 돌을 두었을 때만 카운트 증가
        if (placedType != _casterType)
        {
            _currentTurns++;

            // 목표 턴에 도달하면 나 자신을 파괴
            if (_currentTurns >= _targetTurns)
            {
                Destroy(gameObject);
            }
        }
    }
}