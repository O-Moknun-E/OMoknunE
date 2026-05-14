using System;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.VisualScripting;

public class NetworkOmokManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private BoardInteraction _boardInteraction;
    
    // 전달 데이터: x좌표, y좌표, 진영번호
    public static event Action<int, int, StoneType> OnStonePlaced;

    private int _mySkinIndex = 0;
    private StoneType _myPlayerType = StoneType.Black;

    //외부(스킬 효과 등)에서 내 진영을 확인할 수 있게 열어줍니다.
    public StoneType MyPlayerType => _myPlayerType;

    private bool _isMasterTurn = true;

    private string _loadedSkillName = "";

    private int _silencedTurnsLeft = 0; // 침묵 턴 변수

    [Header("게임 오버 UI")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private TextMeshProUGUI _resultText;

    //public static bool IsReturningFromGame = false;

    //스킬 사용 여부 체크
    private bool _hasUsedSkillThisTurn = false;

    private GameObject _loadedSkillCardObj; 

    public override void OnEnable()
    {
        base.OnEnable();

        // AI 모드가 아닐때 보드인터랙션 이벤트 구독
        if (_boardInteraction != null && !AIMatchManager.IsAIMode)
            _boardInteraction.OnStoneClicked += HandleBoardClick;
        if (OmokManager.Instance != null)
            OmokManager.Instance.OnGameOver += ShowGameOverUI;
    }

    public override void OnDisable()
    {
        base.OnDisable();

        // AI 모드가 아닐때 보드인터랙션 이벤트 해제
        if (_boardInteraction != null && !AIMatchManager.IsAIMode)
            _boardInteraction.OnStoneClicked -= HandleBoardClick;
        if (OmokManager.Instance != null)
            OmokManager.Instance.OnGameOver -= ShowGameOverUI;
    }


    private void Start()
    {
        // AI모드면 무시
        if (AIMatchManager.IsAIMode) return;

        if (OmokManager.Instance != null)
        {
            OmokManager.Instance.InitGame();
        }

        _mySkinIndex = PlayerPrefs.GetInt("MySkinID", 0);

        if (PhotonNetwork.InRoom) SetupGame();
        else
        {
            if (_boardInteraction != null) _boardInteraction.SetMyTurn(false);
        }
        // 방장(MasterClient) 한 명만 이 명령을 서버에 내림
        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
        {
            // 1. IsOpen = false : 아무도 이 방에 들어올 수 없음
            PhotonNetwork.CurrentRoom.IsOpen = false;

            // 2. IsVisible = false : 로비 목록에서 아예 방 이름 지움
            PhotonNetwork.CurrentRoom.IsVisible = false;

            Debug.Log("게임이 시작되어 방을 비공개로 잠갔습니다");
        }
    }

    public override void OnJoinedRoom() => SetupGame();

    private void SetupGame()
    {
        if (_boardInteraction == null || StoneSkinRegistry.Instance.GetStoneSkinCount() == 0) return;

        if (PhotonNetwork.IsMasterClient)
        {
            _myPlayerType = StoneType.Black;

            _mySkinIndex = 0;
        }
        else
        {
            _myPlayerType = StoneType.White;
            _mySkinIndex = 1;
        }
        if(PlayerEquipItem.Instance.customStone != null)
             _boardInteraction.ChangeStoneSkin(PlayerEquipItem.Instance.customStone); //민정추가
        else
            _boardInteraction.ChangeStoneSkin(SkinRegistry.Instance.GetStoneSkin(_mySkinIndex));

        CheckAndApplyTurn();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log("새 플레이어 입장 게임을 준비합니다.");
        CheckAndApplyTurn();
    }

    private void CheckAndApplyTurn()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            _boardInteraction.SetMyTurn(false);
            return;
        }

        // 현재 내 턴인지 여부 계산
        bool isMyTurnNow = (_isMasterTurn && _myPlayerType == StoneType.Black) ||
                           (!_isMasterTurn && _myPlayerType == StoneType.White);

        _boardInteraction.SetMyTurn(isMyTurnNow);
    }

    private void HandleBoardClick(int x, int y)
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            Debug.LogWarning("상대방이 아직 입장하지 않았습니다.");
            return;
        }

        // 1. 장전된 스킬이 있는지 확인
        if (!string.IsNullOrEmpty(_loadedSkillName))
        {
            if (_loadedSkillName == "SilenceSkill" || _loadedSkillName == "TimeOverloadSkill")
                UseSkill(_loadedSkillName, -1, -1);
            else
                UseSkill(_loadedSkillName, x, y);

            _hasUsedSkillThisTurn = true;
            _loadedSkillName = "";

            // 스킬 발사 성공 시 카드를 화면에서 영원히 파괴
            if (_loadedSkillCardObj != null)
            {
                Destroy(_loadedSkillCardObj);
                _loadedSkillCardObj = null;
            }

            if (_boardInteraction != null) _boardInteraction.SetSkillLoadedState(false);
        }
        else
        {
            if (PlayerEquipItem.Instance.customStone != null) //민정추가
                 _mySkinIndex = SkinRegistry.Instance.GetStoneID(PlayerEquipItem.Instance.customStone);

            photonView.RPC("RPC_ReceiveAndDrawStone", RpcTarget.All, x, y, _myPlayerType, _mySkinIndex);
        }
    }

    public void ApplySilence(int turns)
    {
        _silencedTurnsLeft = turns;

        _loadedSkillName = "";
        if (_boardInteraction != null) _boardInteraction.SetSkillLoadedState(false);

        Debug.Log($"<color=red>[System] 침묵에 걸렸습니다 앞으로 {_silencedTurnsLeft}턴 동안 스킬을 사용할 수 없습니다.</color>");

        SkillCardSilenceUI[] mySkillCards = FindObjectsByType<SkillCardSilenceUI>(FindObjectsSortMode.None);

        foreach (SkillCardSilenceUI card in mySkillCards)
        {
            card.ApplySilence();
        }
    }

    public void ReleaseSilenceUI()
    {
        // 침묵이 풀릴 때 UI도 같이 풀어주는 함수
        SkillCardSilenceUI[] mySkillCards = FindObjectsByType<SkillCardSilenceUI>(FindObjectsSortMode.None);

        foreach (SkillCardSilenceUI card in mySkillCards)
        {
            card.ReleaseSilence();
        }
    }

    // 나중에 UI 상점 버튼이 누를 함수
    public void UIButton_LoadSkill(string skillName)
    {
        _loadedSkillName = skillName;
        Debug.Log($"{skillName} 장전 완료!");
    }

    private void Update()
    {
        // 스킬 테스트 로직 정리 (키보드 입력 삭제)
        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom.PlayerCount < 2) return;

        bool isMyTurnNow = (_isMasterTurn && _myPlayerType == StoneType.Black) ||
                           (!_isMasterTurn && _myPlayerType == StoneType.White);

        if (!isMyTurnNow) return;

        // 마우스 우클릭(1)을 누르면 스킬 장전 취소 로직만 남김
        if (Input.GetMouseButtonDown(1) && !string.IsNullOrEmpty(_loadedSkillName))
        {
            _loadedSkillName = "";
            _loadedSkillCardObj = null; // [추가됨] 카드 파괴 취소
            if (_boardInteraction != null) _boardInteraction.SetSkillLoadedState(false);
        }
    }

    // 덱에서 카드를 클릭했을 때 실행될 함수
    public void LoadSkillFromDeck(string skillName, GameObject cardObj)
    {
        // 침묵 상태 검사
        if (_silencedTurnsLeft > 0)
        {
            Debug.Log($"<color=red>[System] 침묵 상태입니다 남은 턴: {_silencedTurnsLeft}</color>");
            return;
        }

        // 이번 턴에 스킬을 썼는지 검사
        if (_hasUsedSkillThisTurn)
        {
            Debug.Log("<color=red>[System] 이미 이번 턴에 스킬을 사용했습니다</color>");
            return;
        }

        // 장전 처리 및 카드 기억하기
        _loadedSkillName = skillName;
        _loadedSkillCardObj = cardObj; 

        if (_boardInteraction != null) _boardInteraction.SetSkillLoadedState(true);
        Debug.Log($"{skillName} 장전 완료");
    }

    public void UseSkill(string skillName, int x, int y)
    {
        PlayerType myType = (_myPlayerType == StoneType.Black) ? PlayerType.Black : PlayerType.White;
        string path = "Skills/" + skillName;

        if(PlayerEquipItem.Instance.customStone != null) //민정추가
            _mySkinIndex = SkinRegistry.Instance.GetStoneID(PlayerEquipItem.Instance.customStone);

        // _mySkinIndex 를 맨 뒤에 추가해서 전송
        photonView.RPC("RPC_ExecuteSkill", RpcTarget.All, path, x, y, myType, _mySkinIndex);
    }

    [PunRPC]
    public void RPC_ReceiveAndDrawStone(int x, int y, StoneType playerType, int skinID)
    {

        Sprite stoneSkin = SkinRegistry.Instance.GetStoneSkin(skinID);

        _boardInteraction.PlaceStoneRemote(x, y, stoneSkin);

        if (playerType == _myPlayerType && _silencedTurnsLeft > 0)
        {
            _silencedTurnsLeft--;
            if (_silencedTurnsLeft == 0)
            {
                Debug.Log("<color=green>[System] 침묵이 해제되었습니다 이제 스킬을 사용할 수 있습니다.</color>");
                ReleaseSilenceUI();
            }
        }

        _isMasterTurn = !_isMasterTurn;
        CheckAndApplyTurn();

        _hasUsedSkillThisTurn = false;

        OnStonePlaced?.Invoke(x, y, playerType);

        // 전역 이벤트(TurnDuration 등 범용)
        GameEvents.TriggerStonePlaced(x, y, playerType);
    }

    [PunRPC]
    public void RPC_ExecuteSkill(string skillPath, int x, int y, PlayerType casterType, int skinID)
    {
        SkillBase skill = Resources.Load<SkillBase>(skillPath);
        if (skill == null) return;

        // 아까 수정한 SetTarget으로 skinID 전달
        skill.SetTarget(x, y, casterType, skinID);

        if (OmokManager.Instance.TryUseMagic(skill))
        {
            Debug.Log($"[Skill] {casterType}가 {skill.Name}를 ({x}, {y})에 시전!");
        }
    }

    /// <summary>
    /// 기권 정보를 네트워크로 전송
    /// </summary>
    public void SendSurrender(StoneType winner)
    {
        photonView.RPC(nameof(RPC_Surrender), RpcTarget.All, winner);
    }

    /// <summary>
    /// 기권 정보를 받아서 게임 종료 처리
    /// </summary>
    [PunRPC]
    private void RPC_Surrender(StoneType winner)
    {
        Debug.Log($"<color=orange>[Surrender] 플레이어가 기권했습니다. {winner} 승리!</color>");

        // 게임 종료 이벤트 발생
        OmokManager.Instance.TriggerGameOver(winner);
    }

    /// <summary>
    /// 이모지를 상대방에게 전송
    /// </summary>
    /// <param name="emojiType"></param>
    public void SendEmojiToOpponent(EmojiType emojiType)
    {
        photonView.RPC("RPC_ReceiveEmoji", RpcTarget.Others, emojiType);
    }

    /// <summary>
    /// 상대방으로부터 이모지 수신
    /// </summary>
    /// <param name="emojiType">수신한 이모지 타입</param>
    [PunRPC]
    private void RPC_ReceiveEmoji(EmojiType emojiType)
    {
        // 상대방 위치에 표시
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ReceiveEmojiFromOpponent(emojiType);
        }
    }

    // 게임 종료 시 승자 정보를 받아서 UI를 띄워주는 함수
    private void ShowGameOverUI(StoneType winner)
    {
        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(true);

            if (_resultText != null)
            {
                if (winner == StoneType.Black)
                    _resultText.text = "Black Win / White Lose";
                else if (winner == StoneType.White)
                    _resultText.text = "White Win / Black Lose";
            }
        }
    }
    public void ReturnToMainMenu()
    {
        // 로그인 되어있는지 체크하는 식으로 변경
        //IsReturningFromGame = true;

        AchievementManager.Instance.achievementTracker.UpdatePlayerGameCount(); //민정추가

        // 방이 존재할때만
        if (PhotonNetwork.CurrentRoom != null)
        {
            PhotonNetwork.LeaveRoom();
        }
        // 없으면 로비씬으로 바로 이동
        else
        {
            SceneManager.LoadScene("LobbyScene");
        }

    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("LobbyScene");// 후에 메인메뉴 씬으로 수정 필요 
    }
}