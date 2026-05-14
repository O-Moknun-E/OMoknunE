using UnityEngine;

public class ManaTrapObject : MonoBehaviour
{
    private int _targetX;
    private int _targetY;
    private PlayerType _casterType;
    private int _manaPenalty;
    private bool _isReplay;

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
                    // 상대방 화면에서는 마나 덫을 숨김 처리합니다
                    sr.enabled = false;
                }
                else
                {
                    // 시전자 화면에서는 반투명하게 표시합니다
                    sr.color = new Color(1f, 1f, 1f, 0.5f);
                }
            }
            else
            {
                sr.enabled = true;
                sr.color = new Color(1f, 1f, 1f, 0.5f);
            }
        }

        GameEvents.OnStonePlaced += OnTriggerTrap;
    }

    private void OnTriggerTrap(int x, int y, StoneType placedType)
    {
        if (x == _targetX && y == _targetY)
        {
            GameEvents.OnStonePlaced -= OnTriggerTrap;

            PlayerType placedPlayer = (placedType == StoneType.Black) ? PlayerType.Black : PlayerType.White;

            if (placedPlayer != _casterType)
            {
                if (!_isReplay)
                {
                    Player victim = OmokManager.Instance.GetPlayer(placedPlayer);
                    if (victim != null)
                    {
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

            // 시각적 연출을 위한 코루틴을 시작합니다
            StartCoroutine(RevealAndDestroyTrap());
        }
    }

    private System.Collections.IEnumerator RevealAndDestroyTrap()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            // 숨겨져 있던 마나 덫을 활성화합니다
            sr.enabled = true;

            // 시전자가 보는 모습과 동일하게 색상을 원상 복구하고 렌더링 순서를 위해 제트축을 조절합니다
            sr.color = new Color(1f, 1f, 1f, 1f);
            transform.position = new Vector3(transform.position.x, transform.position.y, -1f);
        }

        // 오브젝트의 크기를 1.2배로 확대합니다
        Vector3 targetScale = new Vector3(1.2f, 1.2f, 1f);
        transform.localScale = targetScale;

        // 설정된 애니메이션 재생 시간인 1.02초 동안 대기합니다
        yield return new WaitForSeconds(1.02f);

        // 0.2초 동안 크기를 줄이며 소멸하는 연출을 진행합니다
        float shrinkDuration = 0.2f;
        float elapsedTime = 0f;

        while (elapsedTime < shrinkDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / shrinkDuration;

            transform.localScale = Vector3.Lerp(targetScale, Vector3.zero, progress);
            yield return null;
        }

        // 연출 완료 후 오브젝트를 파괴합니다
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        GameEvents.OnStonePlaced -= OnTriggerTrap;
    }
}