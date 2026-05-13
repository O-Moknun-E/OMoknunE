using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameUIManager : SceneSingleton<GameUIManager>
{
    [SerializeField] private TextMeshProUGUI _manaText;              // 마나 표시 텍스트 UI
    [SerializeField] private TextMeshProUGUI _myTurnTimerText;       // 내 턴 타이머 텍스트 UI
    [SerializeField] private TextMeshProUGUI _opponentTurnTimerText; // 상대 턴 타이머 텍스트 UI
    [SerializeField] private TextMeshProUGUI _myNicknameText;        // 내 닉네임 텍스트 UI
    [SerializeField] private TextMeshProUGUI _opponentNicknameText;  // 상대 닉네임 텍스트 UI
    [SerializeField] private Button _surrenderButton;                // 기권 버튼 UI

    private void OnEnable()
    {
        // 기권 버튼 이벤트 등록
        if (_surrenderButton != null)
        {
            _surrenderButton.onClick.AddListener(OnSurrenderButtonClicked);
        }

        if (OmokManager.Instance != null)
        {
            // 게임 시작 될 때의 이벤트 구독
            OmokManager.Instance.OnGameStarted += UpdateInitUI;

            // 마나 변경 될 때의 이벤트 구독
            OmokManager.Instance.OnManaChanged += UpdateManaUI;

            // 턴 타이머 변경 될 때의 이벤트 구독
            OmokManager.Instance.OnTurnTimerSecondChanged += UpdateTurnTimerUI;

            // 이미 게임이 시작된 상태라면 수동으로 시작 될때 이벤트 발생
            if(OmokManager.Instance != null)
            {
                UpdateInitUI();
            }
        }
    }

    private void OnDisable()
    {
        if (OmokManager.Instance != null)
        {
            // 이벤트 구독 해제
            OmokManager.Instance.OnGameStarted -= UpdateInitUI;
            OmokManager.Instance.OnManaChanged -= UpdateManaUI;
            OmokManager.Instance.OnTurnTimerSecondChanged -= UpdateTurnTimerUI;
        }
    }

    /// <summary>
    /// 마우스 포인터가 UI 위에 있는지 체크하는 메서드
    /// </summary>
    /// <returns>UI 위에 있으면 true, 아니면 false</returns>
    public bool IsPointerOverUI()
    {
        // EventSystem이 없으면 false 반환
        if (EventSystem.current == null)
            return false;

        // PointerEventData를 사용하여 레이캐스트
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        // 레이캐스트 결과 중 실제 UI 요소가 있는지 확인
        foreach(RaycastResult result in results)
        {
            // Graphic 컴포넌트가 있고 raycastTarget이 활성화된 경우만 UI로 간주
            Graphic graphic = result.gameObject.GetComponent<Graphic>();

            if(graphic != null && graphic.raycastTarget)
            {
                return true;
            }
        }

        // 마우스 위치가 UI 위에 있는지 체크
        return false;
    }

    /// <summary>
    /// 게임 시작 시 UI 업데이트
    /// </summary>
    private void UpdateInitUI()
    {
        PlayerType myPlayerType = OmokManager.Instance.MyPlayerType;
        PlayerType opponentPlayerType = OmokManager.Instance.MyPlayerType == PlayerType.Black ? PlayerType.White : PlayerType.Black;

        // 닉네임 UI 업데이트
        _myNicknameText.text = OmokManager.Instance.GetPlayer(myPlayerType).Name;
        _opponentNicknameText.text = OmokManager.Instance.GetPlayer(opponentPlayerType).Name;
    }

    /// <summary>
    /// 마나 UI 업데이트
    /// </summary>
    private void UpdateManaUI(int mana)
    {
        _manaText.text = mana.ToString();
    }

    /// <summary>
    /// 턴 타이머 UI 업데이트
    /// </summary>
    /// <param name="remainingTime">남은 착수 시간</param>
    private void UpdateTurnTimerUI(int remainingTime)
    {
        // 내턴일 땐 내 텍스트에 아니면 상대 텍스트에 표시
        if (OmokManager.Instance.IsMyTurn)
        {
            // "MM:SS" 형식으로 시간 표시
            _myTurnTimerText.text = $"{remainingTime / 60:00}:{remainingTime % 60:00}";

            // 상대 타이머는 00:00으로 표시
            _opponentTurnTimerText.text = "00:00";
        }
        else
        {
            // 상대 타이머에 시간 표시
            _opponentTurnTimerText.text = $"{remainingTime / 60:00}:{remainingTime % 60:00}";

            // 내 타이머는 00:00으로 표시
            _myTurnTimerText.text = "00:00";
        }
    }

    /// <summary>
    /// 기권 버튼 클릭 시 호출되는 메서드
    /// </summary>
    private void OnSurrenderButtonClicked()
    {
        OmokManager.Instance.Surrender();
    }
}
