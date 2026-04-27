using UnityEngine;

public class EmojiPanelController : MonoBehaviour
{
    public RectTransform panel;

    [Header("열림 위치 (화면 안)")]
    public Vector2 openPos;   // 예: (0, 0)

    [Header("닫힘 위치 (화면 밖)")]
    public Vector2 closedPos; // 예: (600, 0)

    [Header("속도")]
    public float speed = 10f;

    [Header("닫힘 타이밍")]
    public float closeThreshold = 0.1f;

    private bool isOpen = false;

    void Start()
    {
        // 시작은 닫힘 상태
        panel.anchoredPosition = closedPos;
        panel.gameObject.SetActive(false);
    }

    public void Toggle()
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            panel.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (!panel.gameObject.activeSelf) return;

        Vector2 target = isOpen ? openPos : closedPos;

        panel.anchoredPosition = Vector2.Lerp(
            panel.anchoredPosition,
            target,
            Time.deltaTime * speed
        );

        // 닫힐 때 완전히 숨김
        if (!isOpen && Vector2.Distance(panel.anchoredPosition, closedPos) < closeThreshold)
        {
            panel.anchoredPosition = closedPos;
            panel.gameObject.SetActive(false);
        }
    }
}