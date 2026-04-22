using System;
using UnityEngine;

public class BoardInteraction : MonoBehaviour
{
    // 1. 내 마우스 클릭을 외부(민정님)로 알리는 이벤트
    public event Action<int, int> OnStoneClicked;

    [Header("바둑판 설정")]
    [SerializeField] private int _lineCount = 16;
    [SerializeField] private float _borderMargin = 0.5f;

    [Header("프리팹 설정")]
    [SerializeField] private GameObject _baseStonePrefab;

    private Sprite _currentStoneSprite;
    private SpriteRenderer _boardRenderer;
    private GameObject _previewStone;
    private SpriteRenderer _previewRenderer;
    private bool[,] _isStonePlaced;

    // 2. 내 턴일 때만 클릭이 작동하도록 제어하는 변수
    private bool _canIPlace = false;

    private void Start()
    {
        _boardRenderer = GetComponent<SpriteRenderer>();
        _isStonePlaced = new bool[_lineCount, _lineCount];
    }

    private void Update()
    {
        if (_boardRenderer == null || _currentStoneSprite == null) return;

        // 내 턴일 때만 마우스 미리보기와 클릭 감지 실행
        if (_canIPlace)
        {
            HandleBoardInteraction();
        }
        else
        {
            ClearPreview(); // 내 턴 아니면 미리보기 숨김
        }
    }

    // 턴 권한 설정 함수 안전
    public void SetMyTurn(bool canPlace) => _canIPlace = canPlace;

    public void ChangeStoneSkin(Sprite newSkin)
    {
        _currentStoneSprite = newSkin;
        if (_previewRenderer != null) _previewRenderer.sprite = _currentStoneSprite;
    }

    // 서버에서 누가 어디에 뒀다 알려줄 때 실행할 함수
    // 마우스 클릭 없이도 화면에 돌을 그림
    public void PlaceStoneRemote(int x, int y, Sprite stoneSkin)
    {
        if (_isStonePlaced[x, y]) return;

        // 실제 플레이 영역 계산
        Vector2 size = _boardRenderer.bounds.size;
        float pWidth = size.x - (_borderMargin * 2f);
        float pHeight = size.y - (_borderMargin * 2f);
        float sX = transform.position.x - (pWidth / 2f);
        float sY = transform.position.y - (pHeight / 2f);

        float snapX = sX + (x * (pWidth / (_lineCount - 1)));
        float snapY = sY + (y * (pHeight / (_lineCount - 1)));

        // 돌 생성 및 설정
        GameObject stone = Instantiate(_baseStonePrefab, new Vector2(snapX, snapY), Quaternion.identity);
        stone.GetComponent<SpriteRenderer>().sprite = stoneSkin;

        _isStonePlaced[x, y] = true;
    }

    private void HandleBoardInteraction()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 size = _boardRenderer.bounds.size;
        float pWidth = size.x - (_borderMargin * 2f);
        float pHeight = size.y - (_borderMargin * 2f);
        float sX = transform.position.x - (pWidth / 2f);
        float sY = transform.position.y - (pHeight / 2f);

        float pctX = (mousePos.x - sX) / pWidth;
        float pctY = (mousePos.y - sY) / pHeight;

        if (pctX >= -0.05f && pctX <= 1.05f && pctY >= -0.05f && pctY <= 1.05f)
        {
            int xIdx = Mathf.Clamp(Mathf.RoundToInt(pctX * (_lineCount - 1)), 0, _lineCount - 1);
            int yIdx = Mathf.Clamp(Mathf.RoundToInt(pctY * (_lineCount - 1)), 0, _lineCount - 1);

            if (_isStonePlaced[xIdx, yIdx]) { ClearPreview(); return; }

            float snapX = sX + (xIdx * (pWidth / (_lineCount - 1)));
            float snapY = sY + (yIdx * (pHeight / (_lineCount - 1)));

            UpdatePreview(snapX, snapY);

            if (Input.GetMouseButtonDown(0))
            {
                // 클릭 시 이벤트만 보냄
                OnStoneClicked?.Invoke(xIdx, yIdx);
            }
        }
        else { ClearPreview(); }
    }

    private void UpdatePreview(float x, float y)
    {
        if (_previewStone == null)
        {
            _previewStone = Instantiate(_baseStonePrefab);
            _previewRenderer = _previewStone.GetComponent<SpriteRenderer>();
            _previewRenderer.sprite = _currentStoneSprite;
            Color c = _previewRenderer.color; c.a = 0.5f; _previewRenderer.color = c;
        }
        _previewStone.transform.position = new Vector2(x, y);
    }

    private void ClearPreview()
    {
        if (_previewStone != null) { Destroy(_previewStone); _previewStone = null; }
    }
}