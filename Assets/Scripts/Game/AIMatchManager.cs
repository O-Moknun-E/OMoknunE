using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// AI 매치 설정 및 시작을 담당하는 매니저
/// </summary>
public class AIMatchManager : MonoBehaviour
{
    [Header("UI Buttons")]
    [SerializeField] private Button _easyBtn;
    [SerializeField] private Button _normalBtn;
    [SerializeField] private Button _hardBtn;

    public static AIDifficulty SelectedDifficulty { private set; get; } // 선택된 난이도
    public static bool IsAIMode { private set; get; } = false;          // AI 모드 여부

    private void Start()
    {
        // 버튼 이벤트 등록
        _easyBtn.onClick.AddListener(() => StartAIMatch(AIDifficulty.Easy));
        _normalBtn.onClick.AddListener(() => StartAIMatch(AIDifficulty.Normal));
        _hardBtn.onClick.AddListener(() => StartAIMatch(AIDifficulty.Hard));
    }

    /// <summary>
    /// AI 매치를 시작하는 메서드
    /// </summary>
    /// <param name="difficulty">선택된 AI 난이도</param>
    private void StartAIMatch(AIDifficulty difficulty)
    {
        // 선택된 난이도 및 AI 모드 저장후 씬 전환
        SelectedDifficulty = difficulty;
        IsAIMode = true;

        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// AI 모드 플래그 초기화 (게임 종료 후 호출)
    /// </summary>
    public static void ResetAIMode()
    {
        IsAIMode = false;
    }
}
