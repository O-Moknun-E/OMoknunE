using System;
using UnityEngine;

public class BoardInteraction : MonoBehaviour
{
    // 1. 내 마우스 클릭을 외부로 알리는 이벤트
    public event Action<int, int> OnStoneClicked;

    [Header("바둑판 설정 (기즈모와 동일하게 맞추세요)")]
    [SerializeField] private int _gridSize = 15;
    [SerializeField] private float _spacing = 1.0f;
    [SerializeField] private Vector2 _gridOffset = Vector2.zero;

    [Header("프리팹 설정")]
    [SerializeField] private GameObject _baseStonePrefab;

    [Header("UI 요소")]
    [SerializeField] private GameObject _blindPanel; // BlindPanel을 연결할 칸

    private Sprite _currentStoneSprite;
    private SpriteRenderer _boardRenderer;
    private GameObject _previewStone;
    private SpriteRenderer _previewRenderer;

    // 오목판은 배열만 관리합니다. (딕셔너리 삭제됨)
    private bool[,] _isStonePlaced;

    // 2. 내 턴일 때만 클릭이 작동하도록 제어하는 변수
    private bool _canIPlace = false;
    private bool _isGameOver = false;
    public void SetGameOver() => _isGameOver = true;

    // 현재 스킬이 장전된 상태인지 확인하는 변수
    private bool _isSkillLoaded = false;

    private void Start()
    {
        _boardRenderer = GetComponent<SpriteRenderer>();
        // 15x15 크기에 맞게 bool 배열 생성
        _isStonePlaced = new bool[_gridSize, _gridSize];
    }

    private void Update()
    {
        if (_boardRenderer == null || _currentStoneSprite == null) return;

        if (_isGameOver || !_canIPlace)
        {
            ClearPreview();
            return;
        }
        // 내 턴일 때만 마우스 감지 및 미리보기 실행
        HandleBoardInteraction();
    }

    // 턴 권한 설정 
    public void SetMyTurn(bool canPlace) => _canIPlace = canPlace;

    public void ChangeStoneSkin(Sprite newSkin)
    {
        _currentStoneSprite = newSkin;
        if (_previewRenderer != null) _previewRenderer.sprite = _currentStoneSprite;
    }

    public void SetStonePlacedState(int x, int y, bool isPlaced)
    {
        _isStonePlaced[x, y] = isPlaced;
    }

    // 서버에서 누가 어디에 뒀다 알려줄 때 실행할 함수
    public void PlaceStoneRemote(int x, int y, Sprite stoneSprite)
    {
        // 범위 밖이거나 이미 돌이 있으면 무시
        if (x < 0 || x >= _gridSize || y < 0 || y >= _gridSize) return;
        if (_isStonePlaced[x, y]) return;

        Vector3 worldPos = GetWorldPositionFromIndex(x, y);
        GameObject stone = Instantiate(_baseStonePrefab, worldPos, Quaternion.identity);
        stone.GetComponent<SpriteRenderer>().sprite = stoneSprite;

        _isStonePlaced[x, y] = true;
    }

    // 외부(매니저)에서 스킬 장전 상태를 켜고 끌 수 있는 함수
    public void SetSkillLoadedState(bool isLoaded)
    {
        _isSkillLoaded = isLoaded;

        // 스킬이 장전되었다면 즉시 바둑돌 미리보기를 지움
        if (_isSkillLoaded)
        {
            ClearPreview();
        }
    }

    private void HandleBoardInteraction()
    {
        // 1. 마우스 위치를 월드 좌표로 변환
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        // 2. 바둑판 기준 로컬 좌표로 변환 후 오프셋 적용
        Vector3 localPos = transform.InverseTransformPoint(mouseWorldPos);
        float adjustedX = localPos.x - _gridOffset.x;
        float adjustedY = localPos.y - _gridOffset.y;

        // 3. 인덱스 계산 (15x15의 정중앙을 기준으로)
        float halfSize = (_gridSize - 1) * _spacing / 2f;
        int xIdx = Mathf.RoundToInt((adjustedX + halfSize) / _spacing);
        int yIdx = Mathf.RoundToInt((adjustedY + halfSize) / _spacing);

        // 4. 범위 체크
        if (xIdx >= 0 && xIdx < _gridSize && yIdx >= 0 && yIdx < _gridSize)
        {
            if (_isStonePlaced[xIdx, yIdx]) { ClearPreview(); return; }

            Vector3 snapPos = GetWorldPositionFromIndex(xIdx, yIdx);

            // 스킬이 장전되지 않았을 때만 반투명 돌을 보여줌
            if (!_isSkillLoaded)
            {
                UpdatePreview(snapPos);
            }
            else
            {
                // 스킬 장전 중에는 돌 미리보기를 지움
                ClearPreview();
            }

            if (Input.GetMouseButtonDown(0))
            {
                OnStoneClicked?.Invoke(xIdx, yIdx);
            }
        }
        else { ClearPreview(); }
    }

    //외부 스킬들이 좌표를 물어볼 수 있도록 public으로 변경
    public Vector3 GetWorldPositionFromIndex(int x, int y)
    {
        float halfSize = (_gridSize - 1) * _spacing / 2f;
        float localX = (x * _spacing) - halfSize + _gridOffset.x;
        float localY = (y * _spacing) - halfSize + _gridOffset.y;

        // 돌이 바둑판보다 살짝 앞에(-0.1f) 보이게 처리
        return transform.TransformPoint(new Vector3(localX, localY, -0.1f));
    }

    private void UpdatePreview(Vector3 pos)
    {
        if (_previewStone == null)
        {
            _previewStone = Instantiate(_baseStonePrefab);
            _previewRenderer = _previewStone.GetComponent<SpriteRenderer>();
            _previewRenderer.sprite = _currentStoneSprite;
            Color c = _previewRenderer.color; c.a = 0.5f; _previewRenderer.color = c;
        }
        _previewStone.transform.position = pos;
    }

    private void ClearPreview()
    {
        if (_previewStone != null) { Destroy(_previewStone); _previewStone = null; }
    }

    // 따로 Blind 프리팹 만들어서 적용했습니다
    //// 화면을 잠시 가려주는 스위치 함수
    //public void ShowBlindEffect(float duration)
    //{
    //    if (_blindPanel != null)
    //    {
    //        _blindPanel.SetActive(true);
    //        Debug.Log($"[UI] {duration}초 동안 화면이 가려집니다!");

    //        // 지정된 시간(duration) 뒤에 화면을 다시 밝히는 함수를 예약합니다.
    //        Invoke(nameof(HideBlindEffect), duration);
    //    }
    //}
    //private void HideBlindEffect()
    //{
    //    if (_blindPanel != null)
    //    {
    //        _blindPanel.SetActive(false);
    //    }
    //}
}