using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameUIManager : SceneSingleton<GameUIManager>
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _manaText;              // 마나 표시 텍스트 UI
    [SerializeField] private TextMeshProUGUI _myTurnTimerText;       // 내 턴 타이머 텍스트 UI
    [SerializeField] private TextMeshProUGUI _opponentTurnTimerText; // 상대 턴 타이머 텍스트 UI
    [SerializeField] private TextMeshProUGUI _myNicknameText;        // 내 닉네임 텍스트 UI
    [SerializeField] private TextMeshProUGUI _opponentNicknameText;  // 상대 닉네임 텍스트 UI
    [SerializeField] private Button _surrenderButton;                // 기권 버튼 UI
    
    [Header("초상화 UI")]
    [SerializeField] private Image _myPortraitImage;       // 내 초상화 이미지
    [SerializeField] private Image _opponentPortraitImage; // 상대 초상화 이미지

    private PhotonView _photonView;

    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
    }

    [Header("Emoji Settings")]
    [SerializeField] private GameObject _mySpeechBubble;              // 내 말풍선
    [SerializeField] private GameObject _opponentSpeechBubble;        // 상대 말풍선
    [SerializeField] private Sprite[] _emojiSprites;                  // 이모지 스프라이트 배열
    [SerializeField] private float _speechBubbleDisplayDuration = 2f; // 말풍선 표시 지속 시간

    private Coroutine _mySpeechBubbleCoroutine;         // 내 말풍선 코루틴 참조
    private Coroutine _opponentSpeechBubbleCoroutine;   // 상대 말풍선 코루틴 참조
    private NetworkOmokManager _networkOmokManager;     // 네트워크 오목 매니저 참조

    private void Start()
    {
        // NetworkOmokManager 찾기
        _networkOmokManager = FindFirstObjectByType<NetworkOmokManager>();

        // 시작 시 말풍선 비활성화
        if (_mySpeechBubble != null)
        {
            _mySpeechBubble.SetActive(false);
        }

        if(_opponentSpeechBubble != null)
        {
            _opponentSpeechBubble.SetActive(false);
        }
    }

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

        // 진행 중인 코루틴 정리
        if(_mySpeechBubbleCoroutine != null)
        {
            StopCoroutine(_mySpeechBubbleCoroutine);
        }

        if(_opponentSpeechBubbleCoroutine != null)
        {
            StopCoroutine(_opponentSpeechBubbleCoroutine);
        }
    }

    /// <summary>
    /// 이모티콘 전송 (내 화면 표시 + 상대방에게 RPC 전송)
    /// </summary>
    public void SendEmoji(EmojiType emojiType)
    {
        // 내 화면에 이모지 표시
        ShowEmojiLocal(emojiType, true);

        // PvP일때만 상대방에게 RPC로 이모지 전송
        if(OmokManager.Instance.GameMode == GameMode.PvP)
        {
            _networkOmokManager.SendEmojiToOpponent(emojiType);
        }
    }

    /// <summary>
    /// 상대방으로부터 이모지 수신 (외부 호출용)
    /// </summary>
    public void ReceiveEmojiFromOpponent(EmojiType emojiType)
    {
        // 상대방 위치에 표시
        ShowEmojiLocal(emojiType, false);
    }

    /// <summary>
    /// 로컬에서 이모지 말풍선 표시
    /// </summary>
    /// <param name="emojiType">표시할 이모지 타입</param>
    /// <param name="isMyEmoji">내가 보낸 이모지인지 여부</param>
    private void ShowEmojiLocal(EmojiType emojiType, bool isMyEmoji)
    {
        GameObject targetBubble = isMyEmoji ? _mySpeechBubble : _opponentSpeechBubble;
        ref Coroutine targetCoroutine = ref (isMyEmoji ? ref _mySpeechBubbleCoroutine : ref _opponentSpeechBubbleCoroutine);

        // 말풍선이 없는 경우
        if(targetBubble == null)
        {
            Debug.LogWarning($"GameUIManager: {(isMyEmoji ? "내" : "상대")} 말풍선이 설정되지 않았습니다.");
            return;
        }

        // 말풍선의 자식에서 Image 컴포넌트 찾기
        Transform emojiTransform = targetBubble.transform.Find("Emoji");

        if (emojiTransform == null)
        {
            Debug.LogWarning($"GameUIManager: {(isMyEmoji ? "내" : "상대")} 말풍선 자식에 'Emoji'라는 이름의 GameObject가 없습니다.");
            return;
        }

        Image emojiImage = emojiTransform.GetComponent<Image>();

        // 말풍선의 자식에 Image 컴포넌트가 없는 경우
        if (emojiImage == null)
        {
            Debug.LogWarning($"GameUIManager: {(isMyEmoji ? "내" : "상대")} 말풍선 자식에 Image 컴포넌트가 없습니다.");
            return;
        }

        // 유효한 이모지 타입인지 체크
        int emojiIndex = (int)emojiType;

        if(emojiIndex < 0 || emojiIndex >= _emojiSprites.Length)
        {
            Debug.LogWarning($"GameUIManager: 유효하지 않은 이모지 타입 {emojiType}입니다.");
            return;
        }

        // 이전 코루틴이 실행 중이면 중단
        if(targetCoroutine != null)
        {
            StopCoroutine(targetCoroutine);
        }

        // 이모지 이미지 설정
        emojiImage.sprite = _emojiSprites[emojiIndex];

        // 말풍선 표시 코루틴 시작
        targetCoroutine = StartCoroutine(ShowSpeechBubbleCoroutine(targetBubble));
    }

    /// <summary>
    /// 말풍선을 일정시간 동안 표시하고 자동으로 비활성화하는 코루틴
    /// </summary>
    private IEnumerator ShowSpeechBubbleCoroutine(GameObject speechBubble)
    {
        // 말풍선 활성화
        speechBubble.SetActive(true);

        // 지정된 시간 동안 대기
        yield return new WaitForSeconds(_speechBubbleDisplayDuration);

        // 말풍선 비활성화
        speechBubble.SetActive(false);
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

        int myPortraitID = 0;  //민정추가
        PlayerEquipItem pm = PlayerEquipItem.Instance;

        if (pm.customPicture != null)
            myPortraitID = SkinRegistry.Instance.GetPictureID(pm.customPicture);


        // PvP에서만
        if(OmokManager.Instance.GameMode == GameMode.PvP)
            _photonView.RPC("ReceivePortraitID", RpcTarget.Others, myPortraitID);

        UpdatePortraitUI(OmokManager.Instance.MyPlayerType, myPortraitID);
    }

    public void UpdatePortraitUI(PlayerType playerType, int portraitID) //민정추가
    {
        Sprite portraitSprite = SkinRegistry.Instance.GetPictureSkin(portraitID);

        // portraitID에 해당하는 animator controller 가져오기
        RuntimeAnimatorController animatorController = SkinRegistry.Instance.GetAnimatorController(portraitID);

        if (playerType == OmokManager.Instance.MyPlayerType)
        {
            _myPortraitImage.sprite = portraitSprite;

            // 해당 이미지가 달려있는 게임오브젝트의 Animator 컴포넌트 가져오기
            Animator animator = _myPortraitImage.GetComponent<Animator>();

            if (animator != null & animatorController != null)
            {
                animator.runtimeAnimatorController = animatorController;
            }
        }
        else
        {
            // 해당 이미지가 달려있는 게임오브젝트의 Animator 컴포넌트 가져오기
            Animator animator = _opponentPortraitImage.GetComponent<Animator>();

            if (animator != null & animatorController != null)
            {
                animator.runtimeAnimatorController = animatorController;
            }

            _opponentPortraitImage.sprite = portraitSprite;
        }
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

    [PunRPC]
    public void ReceivePortraitID(int portraitID) //민정추가
    {
        PlayerType opponentType = (OmokManager.Instance.MyPlayerType == PlayerType.Black)
                                  ? PlayerType.White : PlayerType.Black;

        UpdatePortraitUI(opponentType, portraitID);
    }

}
