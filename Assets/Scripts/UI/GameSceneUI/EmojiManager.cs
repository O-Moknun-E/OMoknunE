using UnityEngine;
using UnityEngine.UI;

public class EmojiManager : MonoBehaviour
{
    [Header("패널 & 버튼")]
    public GameObject emojiPanel;
    public Button[] emojiBtns;          // 이모지 이미지 버튼 8개

    [Header("이모지 스프라이트 (버튼 순서와 동일하게)")]
    public Sprite[] emojiSprites;       // 8개 이미지를 여기에 드래그

    [Header("이모지 표시 위치")]
    public RectTransform displayArea;   // 이모지가 생성될 DisplayArea

    private bool isPanelOpen = false;

    void Start()
    {
        emojiPanel.SetActive(false);

        for (int i = 0; i < emojiBtns.Length; i++)
        {
            int index = i;
            emojiBtns[i].onClick.AddListener(() => SelectEmoji(index));
        }
    }

    public void TogglePanel()
    {
        isPanelOpen = !isPanelOpen;
        emojiPanel.SetActive(isPanelOpen);
        Debug.Log("패널 열림: " + isPanelOpen);
    }

    void SelectEmoji(int index)
    {
        Debug.Log("이모지 선택: " + index);
        SpawnEmojiImage(emojiSprites[index]);
        emojiPanel.SetActive(false);
        isPanelOpen = false;
    }

    void SpawnEmojiImage(Sprite sprite)
    {
        // DisplayArea 안에 새 Image 오브젝트 생성
        GameObject newObj = new GameObject("EmojiImage");
        newObj.transform.SetParent(displayArea, false);

        // Image 컴포넌트 추가 후 선택한 스프라이트 적용
        Image img = newObj.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;

        // 크기 및 위치 설정
        RectTransform rt = newObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100, 100);

        // DisplayArea 중앙에 고정으로 띄우기
        rt.anchoredPosition = Vector2.zero;
    }
}