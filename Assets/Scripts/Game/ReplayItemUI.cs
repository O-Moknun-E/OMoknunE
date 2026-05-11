using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 리플레이 목록의 개별 항목 UI
/// </summary>
public class ReplayItemUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _playersText;  // 흑, 백
    [SerializeField] private TextMeshProUGUI _winnerText;   // 승자
    [SerializeField] private TextMeshProUGUI _dateText;     // 날짜
    [SerializeField] private TextMeshProUGUI _durationText; // 게임 시간
    [SerializeField] private Button _playButton;            // 재생 버튼

    private string _replayID;

    // 이벤트
    public event Action OnPlayClicked;  // 재생 버튼이 클릭되었을 때 발생하는 이벤트

    private void Awake()
    {
        // 버튼 이벤트 연결
        if(_playButton != null)
            _playButton.onClick.AddListener(() => OnPlayClicked?.Invoke());
    }

    /// <summary>
    /// 리플레이 메타데이터로 UI 설정
    /// </summary>
    /// <param name="metadata"></param>
    public void SetData(Replay.ReplayMetadata metadata)
    {
        _replayID = metadata.replayID;

        // 플레이어 정보
        if(_playersText != null)
        {
            _playersText.text = $"{metadata.blackPlayerName} vs {metadata.whitePlayerName}";
        }

        // 승자
        if(_winnerText != null)
        {
            _winnerText.text = $"승자: {metadata.winner}";
        }

        // 날짜
        if(_dateText != null)
        {
            _dateText.text = metadata.gameDate;
        }

        // 게임 시간
        if(_durationText != null)
        {
            TimeSpan time = TimeSpan.FromSeconds(metadata.totalGameTime);
            _durationText.text = $"{time.Minutes:D2}:{time.Seconds:D2}";
        }
    }
}
