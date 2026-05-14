using UnityEngine;
using UnityEngine.UI;

public class EmojiPanel : MonoBehaviour
{
    [SerializeField] private Button[] _emojiButtons;    // 이모지 버튼
    [SerializeField] private float _cooldownTime = 1f;  // 이모지 버튼 쿨다운 시간 (스팸 방지)

    private float _lastSendTime;

    private void Awake()
    {
        // 각 버튼에 클릭 이벤트 등록
        for (int i = 0; i < _emojiButtons.Length; i++)
        {
            // 클로저 문제 방지
            int index = i;

            _emojiButtons[i].onClick.AddListener(() => OnEmojiButtonClicked(index));
        }
    }


    private void OnEmojiButtonClicked(int emojiIndex)
    {
        // 쿨다운 체크 (스팸 방지)
        if(Time.time - _lastSendTime < _cooldownTime)
        {
            Debug.Log("이모지 전송 쿨다운 중입니다.");
            return;
        }

        _lastSendTime = Time.time;

        // 이모지 전송
        EmojiType emojiType = (EmojiType)emojiIndex;
        GameUIManager.Instance.SendEmoji(emojiType);
    }
}
