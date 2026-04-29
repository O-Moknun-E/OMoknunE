using UnityEngine;
using UnityEngine.UI;

public class EmojiToggle : MonoBehaviour
{
    public GameObject emojiPanel;
    public Image displayImage;       // 선택된 이모지 이미지 표시
    public Button[] emojiButtons;    // 버튼 배열
    public Sprite[] emojiSprites;    // 이미지(스프라이트) 배열 ← 팀원 이미지 여기에

    private bool isOpen = false;

    void Start()
    {
        displayImage.gameObject.SetActive(false); // 처음엔 숨김

        for (int i = 0; i < emojiButtons.Length; i++)
        {
            int index = i;
            emojiButtons[i].onClick.AddListener(() => OnEmojiSelected(index));

            // 버튼 안 Icon 이미지도 자동으로 스프라이트 적용
            emojiButtons[i].transform.Find("Icon")
                .GetComponent<Image>().sprite = emojiSprites[i];
        }
    }

    public void OnToggleClicked()
    {
        isOpen = !isOpen;
        emojiPanel.SetActive(isOpen);
    }

    void OnEmojiSelected(int index)
    {
        displayImage.sprite = emojiSprites[index];
        displayImage.gameObject.SetActive(true);
        isOpen = false;
        emojiPanel.SetActive(false);
    }
}