using UnityEngine;

public class FakeStoneCleanup : MonoBehaviour
{
    private int _targetX;
    private int _targetY;

    // 전달받은 원본 프리팹을 저장하는 변수입니다
    private GameObject _originalPrefab;

    public void LockSpot(int x, int y, GameObject originalPrefab)
    {
        _targetX = x;
        _targetY = y;
        _originalPrefab = originalPrefab;

        OmokManager.Instance.SetBoardData(_targetX, _targetY, StoneType.Fake);

        BoardInteraction bi = FindFirstObjectByType<BoardInteraction>();
        if (bi != null) bi.SetStonePlacedState(_targetX, _targetY, true);
    }

    private void OnDestroy()
    {
        if (!gameObject.scene.isLoaded) return;

        // 게임 로직 데이터 처리를 즉시 진행합니다
        if (OmokManager.Instance != null && OmokManager.Instance.GetBoardData(_targetX, _targetY) == StoneType.Fake)
        {
            OmokManager.Instance.SetBoardData(_targetX, _targetY, StoneType.Empty);
        }

        BoardInteraction bi = FindFirstObjectByType<BoardInteraction>();
        if (bi != null) bi.SetStonePlacedState(_targetX, _targetY, false);

        // 시각적 연출을 위해 원본 프리팹을 복제하여 생성합니다
        if (_originalPrefab != null)
        {
            Vector3 pos = new Vector3(transform.position.x, transform.position.y, -1f);
            GameObject dummy = Instantiate(_originalPrefab, pos, Quaternion.identity);

            // 연출용 오브젝트가 오목판 데이터에 영향을 주지 않도록 로직 스크립트를 제거합니다
            TurnDuration td = dummy.GetComponent<TurnDuration>();
            if (td != null) Destroy(td);

            FakeStoneCleanup fsc = dummy.GetComponent<FakeStoneCleanup>();
            if (fsc != null) Destroy(fsc);

            SpriteRenderer sr = dummy.GetComponent<SpriteRenderer>();
            Animator anim = dummy.GetComponent<Animator>();

            // 시전자가 보는 원본 모습과 동일하게 렌더링 설정 및 애니메이션을 활성화합니다
            if (sr != null) sr.color = new Color(1f, 1f, 1f, 1f);
            if (anim != null) anim.enabled = true;

            // 일초 동안 반복 애니메이션을 보여준 후 연출 오브젝트를 파괴합니다
            Destroy(dummy, 2.06f);
        }
    }
}