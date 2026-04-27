using UnityEngine;

public class EmojiPanelController : MonoBehaviour
{
    public RectTransform panel;

    public float speed = 10f;

    private bool isOpen = false;

    private Vector2 openPos;
    private Vector2 closedPos;

    void Start()
    {
        // 현재 위치 = 열린 위치
        openPos = panel.anchoredPosition;

        // 오른쪽 밖으로 이동 = 닫힌 위치
        closedPos = openPos + new Vector2(300f, 0);

        // 시작은 닫힘
        panel.anchoredPosition = closedPos;
    }

    public void Toggle()
    {
        Debug.Log("버튼 눌림"); // 👈 확인용
        isOpen = !isOpen;
    }

    void Update()
    {
        Vector2 target = isOpen ? openPos : closedPos;

        panel.anchoredPosition = Vector2.Lerp(
            panel.anchoredPosition,
            target,
            Time.deltaTime * speed
        );
    }
}