using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomizationUI : MonoBehaviour
{
    [Header("오목판 슬롯 (3개)")]
    public Image[] boardSlots;
    public Sprite[] boardSprites;

    [Header("오목알 슬롯 (14개)")]
    public Image[] stoneSlots;
    public Sprite[] stoneSprites;

    [Header("초상화 슬롯 (6개)")]
    public Image[] portraitSlots;
    public Sprite[] portraitSprites;

    [Header("Apply 버튼")]
    public Button applyButton;

    private int selectedBoard = 0;
    private int selectedStone = 0;
    private int selectedPortrait = 0;

    void Start()
    {
        for (int i = 0; i < boardSlots.Length; i++)
            boardSlots[i].sprite = boardSprites[i];

        for (int i = 0; i < stoneSlots.Length; i++)
            stoneSlots[i].sprite = stoneSprites[i];

        for (int i = 0; i < portraitSlots.Length; i++)
            portraitSlots[i].sprite = portraitSprites[i];

        applyButton.onClick.AddListener(OnApplyClicked);
    }

    public void OnBoardSelected(int index)
    {
        selectedBoard = index;
    }

    public void OnStoneSelected(int index)
    {
        selectedStone = index;
    }

    public void OnPortraitSelected(int index)
    {
        selectedPortrait = index;
    }

    void OnApplyClicked()
    {
        Debug.Log($"적용 완료! 판:{selectedBoard} 알:{selectedStone} 초상화:{selectedPortrait}");
        // 여기에 실제 적용 코드 추가
    }
}

//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;

//public class CustomizationUI : MonoBehaviour
//{
//    [Header("오목판 슬롯 (3개)")]
//    public Image[] boardSlots;
//    public Sprite[] boardSprites;

//    [Header("오목알 슬롯 (14개)")]
//    public Image[] stoneSlots;
//    public Sprite[] stoneSprites;

//    [Header("초상화 슬롯 (6개)")]
//    public Image[] portraitSlots;
//    public Sprite[] portraitSprites;

//    [Header("버튼 오브젝트들")]
//    public Button[] boardButtons;      // Board 버튼 3개
//    public Button[] stoneButtons;      // Stone 버튼 14개
//    public Button[] portraitButtons;   // Portrait 버튼 6개

//    [Header("Apply 버튼")]
//    public Button applyButton;

//    // 선택 색깔
//    private Color selectedColor = new Color(0.5f, 0.3f, 1f, 1f);   // 보라색
//    private Color normalColor = new Color(1f, 1f, 1f, 1f);          // 흰색

//    private int selectedBoard = 0;
//    private int selectedStone = 0;
//    private int selectedPortrait = 0;

//    void Start()
//    {
//        for (int i = 0; i < boardSlots.Length; i++)
//            boardSlots[i].sprite = boardSprites[i];

//        for (int i = 0; i < stoneSlots.Length; i++)
//            stoneSlots[i].sprite = stoneSprites[i];

//        for (int i = 0; i < portraitSlots.Length; i++)
//            portraitSlots[i].sprite = portraitSprites[i];

//        applyButton.onClick.AddListener(OnApplyClicked);

//        RefreshAllColors();
//    }

//    public void OnBoardSelected(int index)
//    {
//        selectedBoard = index;
//        RefreshBoardColors();
//    }

//    public void OnStoneSelected(int index)
//    {
//        selectedStone = index;
//        RefreshStoneColors();
//    }

//    public void OnPortraitSelected(int index)
//    {
//        selectedPortrait = index;
//        RefreshPortraitColors();
//    }

//    void OnApplyClicked()
//    {
//        Debug.Log($"적용 완료! 판:{selectedBoard} 알:{selectedStone} 초상화:{selectedPortrait}");
//        // 여기에 실제 적용 코드 추가
//    }

//    void RefreshAllColors()
//    {
//        RefreshBoardColors();
//        RefreshStoneColors();
//        RefreshPortraitColors();
//    }

//    void RefreshBoardColors()
//    {
//        for (int i = 0; i < boardButtons.Length; i++)
//            boardButtons[i].image.color = (i == selectedBoard) ? selectedColor : normalColor;
//    }

//    void RefreshStoneColors()
//    {
//        for (int i = 0; i < stoneButtons.Length; i++)
//            stoneButtons[i].image.color = (i == selectedStone) ? selectedColor : normalColor;
//    }

//    void RefreshPortraitColors()
//    {
//        for (int i = 0; i < portraitButtons.Length; i++)
//            portraitButtons[i].image.color = (i == selectedPortrait) ? selectedColor : normalColor;
//    }
//}


//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;

//public class CustomizationUI : MonoBehaviour
//{
//    [Header("오목판 슬롯 (3개)")]
//    public Image[] boardSlots;       // 각 버튼의 ItemImage 연결
//    public Sprite[] boardSprites;    // 오목판 스프라이트 3개

//    [Header("오목알 슬롯 (14개)")]
//    public Image[] stoneSlots;
//    public Sprite[] stoneSprites;

//    [Header("초상화 슬롯 (6개)")]
//    public Image[] portraitSlots;
//    public Sprite[] portraitSprites;

//    [Header("선택 테두리 이미지들")]
//    public Image[] boardBorders;
//    public Image[] stoneBorders;
//    public Image[] portraitBorders;

//    [Header("Apply 버튼")]
//    public Button applyButton;

//    // 현재 선택된 인덱스
//    private int selectedBoard = 0;
//    private int selectedStone = 0;
//    private int selectedPortrait = 0;

//    // 실제로 적용된 선택값
//    private int appliedBoard = 0;
//    private int appliedStone = 0;
//    private int appliedPortrait = 0;

//    void Start()
//    {
//        // 스프라이트를 슬롯 이미지에 자동 배치
//        for (int i = 0; i < boardSlots.Length; i++)
//            boardSlots[i].sprite = boardSprites[i];

//        for (int i = 0; i < stoneSlots.Length; i++)
//            stoneSlots[i].sprite = stoneSprites[i];

//        for (int i = 0; i < portraitSlots.Length; i++)
//            portraitSlots[i].sprite = portraitSprites[i];

//        // Apply 버튼 이벤트 등록
//        applyButton.onClick.AddListener(OnApplyClicked);

//        // 초기 선택 표시
//        RefreshSelection();
//    }

//    // 오목판 클릭 시 호출 (버튼에 직접 연결)
//    public void OnBoardSelected(int index)
//    {
//        selectedBoard = index;
//        RefreshBoardBorders();
//    }

//    // 오목알 클릭 시 호출
//    public void OnStoneSelected(int index)
//    {
//        selectedStone = index;
//        RefreshStoneBorders();
//    }

//    // 초상화 클릭 시 호출
//    public void OnPortraitSelected(int index)
//    {
//        selectedPortrait = index;
//        RefreshPortraitBorders();
//    }

//    // Apply 버튼 클릭
//    void OnApplyClicked()
//    {
//        appliedBoard = selectedBoard;
//        appliedStone = selectedStone;
//        appliedPortrait = selectedPortrait;

//        Debug.Log($"적용 완료! 판:{appliedBoard} 알:{appliedStone} 초상화:{appliedPortrait}");

//        // 여기서 실제 게임 오브젝트에 스프라이트 적용하는 코드 추가
//        // 예: boardRenderer.sprite = boardSprites[appliedBoard];
//    }

//    void RefreshSelection()
//    {
//        RefreshBoardBorders();
//        RefreshStoneBorders();
//        RefreshPortraitBorders();
//    }

//    void RefreshBoardBorders()
//    {
//        for (int i = 0; i < boardBorders.Length; i++)
//            boardBorders[i].gameObject.SetActive(i == selectedBoard);
//    }

//    void RefreshStoneBorders()
//    {
//        for (int i = 0; i < stoneBorders.Length; i++)
//            stoneBorders[i].gameObject.SetActive(i == selectedStone);
//    }

//    void RefreshPortraitBorders()
//    {
//        for (int i = 0; i < portraitBorders.Length; i++)
//            portraitBorders[i].gameObject.SetActive(i == selectedPortrait);
//    }
//}