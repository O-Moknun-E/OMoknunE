using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReplayManager : MonoBehaviour
{
    [Header("UI 연결")]
    public TextMeshProUGUI turnLabel;
    public Slider turnSlider;
    public Button btnPrev, btnPlayPause, btnNext;
    public TMP_InputField jumpInput;
    public Button btnJump;
    public TextMeshProUGUI playPauseLabel; // BtnPlayPause 안의 텍스트

    private int currentTurn = 0;
    private int totalTurns = 20;       // ← 본인 총 턴 수로 바꾸기
    private bool isPlaying = false;
    private float playInterval = 1f;   // ← 몇 초마다 다음 턴으로 넘길지
    private float timer = 0f;

    void Start()
    {
        turnSlider.minValue = 0;
        turnSlider.maxValue = totalTurns;
        turnSlider.wholeNumbers = true;

        btnPrev.onClick.AddListener(() => GoTo(currentTurn - 1));
        btnNext.onClick.AddListener(() => GoTo(currentTurn + 1));
        btnPlayPause.onClick.AddListener(TogglePlay);
        btnJump.onClick.AddListener(JumpToInput);
        turnSlider.onValueChanged.AddListener(v => GoTo((int)v));

        GoTo(0);
    }

    void Update()
    {
        if (!isPlaying) return;

        timer += Time.deltaTime;
        if (timer >= playInterval)
        {
            timer = 0f;

            if (currentTurn >= totalTurns)
            {
                // 끝까지 가면 자동으로 정지
                isPlaying = false;
                playPauseLabel.text = "재생";
                return;
            }

            GoTo(currentTurn + 1);
        }
    }

    void TogglePlay()
    {
        isPlaying = !isPlaying;
        playPauseLabel.text = isPlaying ? "일시정지" : "재생";
        timer = 0f;
    }

    void GoTo(int turn)
    {
        currentTurn = Mathf.Clamp(turn, 0, totalTurns);

        // ↓ 여기에 본인 리플레이 함수 넣기
        // 예: MyReplay.ShowTurn(currentTurn);

        turnLabel.text = $"{currentTurn}턴 / {totalTurns}턴";
        turnSlider.SetValueWithoutNotify(currentTurn);

        btnPrev.interactable = currentTurn > 0;
        btnNext.interactable = currentTurn < totalTurns;
    }

    void JumpToInput()
    {
        if (int.TryParse(jumpInput.text, out int turn))
        {
            GoTo(turn);
        }
    }
}