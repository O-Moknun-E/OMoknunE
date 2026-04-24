using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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

    private Sprite _currentStoneSprite;
    private SpriteRenderer _boardRenderer;
    private GameObject _previewStone;
    private SpriteRenderer _previewRenderer;
    private bool[,] _isStonePlaced;

    //↓필드 추가했습니다 !! - SKill
    //돌 지우는 스킬 만든다고 돌 오브젝트 관리용 딕셔너리 하나 추가했어요
    private Dictionary<Vector2Int, GameObject> _placedStones = new Dictionary<Vector2Int, GameObject>();


    // 2. 내 턴일 때만 클릭이 작동하도록 제어하는 변수
    private bool _canIPlace = false;
    private bool _isGameOver = false;
    public void SetGameOver() => _isGameOver = true;

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

    // 서버에서 누가 어디에 뒀다 알려줄 때 실행할 함수 (화면에 돌 그리기)

    public void PlaceStoneRemote(int x, int y, Sprite stoneSprite)
    {
        Vector3 worldPos = GetWorldPositionFromIndex(x, y);
        GameObject stone = Instantiate(_baseStonePrefab, worldPos, Quaternion.identity);
        stone.GetComponent<SpriteRenderer>().sprite = stoneSprite;

        // 좌표 정보를 키로 해서 저장
        Vector2Int posKey = new Vector2Int(x, y);
        if (!_placedStones.ContainsKey(posKey))
        {
            _placedStones.Add(posKey, stone);
        }

        _isStonePlaced[x, y] = true;
    }
    //도헌님 원본코드↓
    //public void PlaceStoneRemote(int x, int y, Sprite stoneSkin)
    //{
    //    // 범위 밖이거나 이미 돌이 있으면 무시
    //    if (x < 0 || x >= _gridSize || y < 0 || y >= _gridSize) return;
    //    if (_isStonePlaced[x, y]) return;

    //    // 정확한 좌표를 구해서 돌 생성
    //    Vector3 targetPos = GetWorldPositionFromIndex(x, y);
    //    GameObject stone = Instantiate(_baseStonePrefab, targetPos, Quaternion.identity);
    //    stone.GetComponent<SpriteRenderer>().sprite = stoneSkin;

    //    _isStonePlaced[x, y] = true;
    //}

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
            UpdatePreview(snapPos);

            if (Input.GetMouseButtonDown(0))
            {
                OnStoneClicked?.Invoke(xIdx, yIdx);
            }
        }
        else { ClearPreview(); }
    }

    // 인덱스(x, y)를 넣으면 화면상의 정확한 월드 좌표를 반환
    private Vector3 GetWorldPositionFromIndex(int x, int y)
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

    /////////////////////////////////////////////////////////////////
    // BoardInteraction에 추가한 내용입니다! - Skill
    

    public void RemoveStoneVisual(int x, int y)
    {
        Vector2Int posKey = new Vector2Int(x, y);

        if (_placedStones.TryGetValue(posKey, out GameObject stone))
        {
            Destroy(stone);             // 1. 화면에서 제거
            _placedStones.Remove(posKey); // 2. 관리 목록에서 제거
            _isStonePlaced[x, y] = false; // 3. 클릭 방지 해제
            Debug.Log($"[Visual] {x}, {y} 좌표의 돌 오브젝트를 파괴했습니다.");
        }
    }
    /////////////////////////////////////////////////////////////////


}