using UnityEngine;

public class ManaTrapObject : MonoBehaviour
{
    private int _targetX;
    private int _targetY;
    private PlayerType _casterType;
    private int _manaPenalty;
    private bool _isReplay;

    // 덫이 생성될 때 정보를 세팅하는 함수
    public void Setup(int x, int y, PlayerType caster, int penalty, bool isReplay)
    {
        _targetX = x;
        _targetY = y;
        _casterType = caster;
        _manaPenalty = penalty;
        _isReplay = isReplay;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (!isReplay)
            {
                NetworkOmokManager netManager = FindFirstObjectByType<NetworkOmokManager>();
                PlayerType myType = (netManager != null && netManager.MyPlayerType == StoneType.Black) ? PlayerType.Black : PlayerType.White;

                if (caster != myType)
                {
                    // 상대방 화면에서는 렌더러를 꺼서 완전 투명하게 만듦
                    sr.enabled = false;
                }
                else
                {
                    // 내 화면에서는 덫이라는 걸 알 수 있게 반투명(Alpha 0.5) 처리
                    sr.color = new Color(1f, 1f, 1f, 0.5f);
                }
            }
            else
            {
                // 리플레이 모드에서는 덫이 어디 있었는지 관전자에게 다 보여줌
                sr.enabled = true;
                sr.color = new Color(1f, 1f, 1f, 0.5f);
            }
        }

        // 돌이 놓이는 이벤트를 구독하여 누군가 밟기를 기다림
        GameEvents.OnStonePlaced += OnTriggerTrap;
    }

    private void OnTriggerTrap(int x, int y, StoneType placedType)
    {
        // 방금 놓인 돌이 내 덫의 위치와 같다면
        if (x == _targetX && y == _targetY)
        {
            PlayerType placedPlayer = (placedType == StoneType.Black) ? PlayerType.Black : PlayerType.White;

            // 시전자가 아닌 상대방이 밟았을 때만 마나 강탈(내가 실수로 밟으면 덫만 부서짐)
            if (placedPlayer != _casterType)
            {
                if (!_isReplay)
                {
                    Player victim = OmokManager.Instance.GetPlayer(placedPlayer);
                    if (victim != null)
                    {
                        // 마나가 마이너스가 되지 않도록 계산해서 깎음
                        int deduction = Mathf.Min(victim.CurrentMana, _manaPenalty);
                        victim.AddMana(-deduction);

                        Debug.Log($"<color=magenta>[System] {placedPlayer}가 마나 덫을 밟아 마나를 {deduction} 잃었습니다</color>");
                    }
                }
                else
                {
                    Debug.Log($"<color=magenta>[Replay] {placedPlayer}가 마나 덫을 밟았습니다</color>");
                }
            }

            // 누가 밟았든 돌이 놓였으므로 덫은 파괴됨
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // 파괴될 때 이벤트 구독 해제 (메모리 누수 방지)
        GameEvents.OnStonePlaced -= OnTriggerTrap;
    }
}