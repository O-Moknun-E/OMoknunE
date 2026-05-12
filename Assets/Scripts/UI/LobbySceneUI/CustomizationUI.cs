using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomizationUI : MonoBehaviour
{
    [Header("오목판 슬롯 (3개)")]
    public Image[] boardSlots;
    public Sprite[] boardSprites;

    [Header("흑돌 슬롯 (7개)")]
    public Image[] blackstoneSlots;
    public Sprite[] blackstoneSprites;

    [Header("백돌 슬롯 (7개)")]
    public Image[] whitestoneSlots;
    public Sprite[] whitestoneSprites;

    [Header("초상화 슬롯 (6개)")]
    public Image[] portraitSlots;
    public Sprite[] portraitSprites;

    [Header("버튼 오브젝트들")]
    public Button[] boardButtons;
    public Button[] blackStoneButtons;
    public Button[] whiteStoneButtons;
    public Button[] portraitButtons;

    [Header("Apply 버튼")]
    public Button applyButton;

    private Color selectedColor = new Color(0.5f, 0.3f, 1f, 1f);
    private Color normalColor = new Color(1f, 1f, 1f, 1f);

    private int selectedBoard = 0;
    private int selectedBlackStone = 0;
    private int selectedWhiteStone = 0;
    private int selectedPortrait = 0;

    void Start()
    {
        selectedBoard = PlayerPrefs.GetInt("SelectedBoard", 0);
        selectedBlackStone = PlayerPrefs.GetInt("SelectedBlackStone", 0);
        selectedWhiteStone = PlayerPrefs.GetInt("SelectedWhiteStone", 0);
        selectedPortrait = PlayerPrefs.GetInt("SelectedPortrait", 0);

        for (int i = 0; i < boardSlots.Length; i++)
            boardSlots[i].sprite = boardSprites[i];

        for (int i = 0; i < blackstoneSlots.Length; i++)
            blackstoneSlots[i].sprite = blackstoneSprites[i];

        for (int i = 0; i < whitestoneSlots.Length; i++)
            whitestoneSlots[i].sprite = whitestoneSprites[i];

        for (int i = 0; i < portraitSlots.Length; i++)
            portraitSlots[i].sprite = portraitSprites[i];

        applyButton.onClick.AddListener(OnApplyClicked);
        RefreshAllColors();
    }

    public void OnBoardSelected(int index)
    {
        selectedBoard = index;
        RefreshBoardColors();
    }

    public void OnBlackStoneSelected(int index)
    {
        selectedBlackStone = index;
        RefreshBlackStoneColors();
    }

    public void OnWhiteStoneSelected(int index)
    {
        selectedWhiteStone = index;
        RefreshWhiteStoneColors();
    }

    public void OnPortraitSelected(int index)
    {
        selectedPortrait = index;
        RefreshPortraitColors();
    }

    void OnApplyClicked()
    {
        PlayerPrefs.SetInt("SelectedBoard", selectedBoard);
        PlayerPrefs.SetInt("SelectedBlackStone", selectedBlackStone);
        PlayerPrefs.SetInt("SelectedWhiteStone", selectedWhiteStone);
        PlayerPrefs.SetInt("SelectedPortrait", selectedPortrait);
        PlayerPrefs.Save();

        Debug.Log($"적용 완료! 판:{selectedBoard} 흑돌:{selectedBlackStone} 백돌:{selectedWhiteStone} 초상화:{selectedPortrait}");
    }

    void RefreshAllColors()
    {
        RefreshBoardColors();
        RefreshBlackStoneColors();
        RefreshWhiteStoneColors();
        RefreshPortraitColors();
    }

    void RefreshBoardColors()
    {
        for (int i = 0; i < boardButtons.Length; i++)
            boardButtons[i].image.color = (i == selectedBoard) ? selectedColor : normalColor;
    }

    void RefreshBlackStoneColors()
    {
        for (int i = 0; i < blackStoneButtons.Length; i++)
            blackStoneButtons[i].image.color = (i == selectedBlackStone) ? selectedColor : normalColor;
    }

    void RefreshWhiteStoneColors()
    {
        for (int i = 0; i < whiteStoneButtons.Length; i++)
            whiteStoneButtons[i].image.color = (i == selectedWhiteStone) ? selectedColor : normalColor;
    }

    void RefreshPortraitColors()
    {
        for (int i = 0; i < portraitButtons.Length; i++)
            portraitButtons[i].image.color = (i == selectedPortrait) ? selectedColor : normalColor;
    }
}

//using UnityEngine;  -------기존 스크립트----오류없으면 그때 지우기!
//using UnityEngine.UI;
//using TMPro;

//public class CustomizationUI : MonoBehaviour
//{
//    [Header("오목판 슬롯 (3개)")]
//    public Image[] boardSlots;
//    public Sprite[] boardSprites;

//    [Header("흑돌 슬롯 (7개)")]
//    public Image[] blackstoneSlots;
//    public Sprite[] blackstoneSprites;

//    [Header("백돌 슬롯 (7개)")]
//    public Image[] whitestoneSlots;
//    public Sprite[] whitestoneSprites;

//    [Header("초상화 슬롯 (6개)")]
//    public Image[] portraitSlots;
//    public Sprite[] portraitSprites;

//    [Header("Apply 버튼")]
//    public Button applyButton;

//    private int selectedBoard = 0;
//    private int selectedBlackStone = 0;
//    private int selectedWhiteStone = 0;
//    private int selectedPortrait = 0;

//    void Start()
//    {
//        for (int i = 0; i < boardSlots.Length; i++)
//            boardSlots[i].sprite = boardSprites[i];

//        for (int i = 0; i < blackstoneSlots.Length; i++)
//            blackstoneSlots[i].sprite = blackstoneSprites[i];

//        for (int i = 0; i < whitestoneSlots.Length; i++)
//            whitestoneSlots[i].sprite = whitestoneSprites[i];

//        for (int i = 0; i < portraitSlots.Length; i++)
//            portraitSlots[i].sprite = portraitSprites[i];

//        applyButton.onClick.AddListener(OnApplyClicked);
//    }

//    public void OnBoardSelected(int index)
//    {
//        selectedBoard = index;
//    }

//    public void OnBlackStoneSelected(int index)
//    {
//        selectedBlackStone = index;
//    }

//    public void OnWhiteStoneSelected(int index)
//    {
//        selectedWhiteStone = index;
//    }

//    public void OnPortraitSelected(int index)
//    {
//        selectedPortrait = index;
//    }

//    void OnApplyClicked()
//    {
//        Debug.Log($"적용 완료! 판:{selectedBoard} 흑돌:{selectedBlackStone} 백돌:{selectedWhiteStone} 초상화:{selectedPortrait}");
//        // 여기에 실제 적용 코드 추가
//    }
//}

